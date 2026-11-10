using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AstralCodex
{
    public sealed class Scene : FrameworkElement
    {
        private readonly BitmapImage illustration;
        private readonly Typeface font = new Typeface("Segoe UI");
        public double Seconds;
        public string Status = "正在连接 Codex";

        public Scene(string root)
        {
            illustration = new BitmapImage();
            illustration.BeginInit();
            illustration.CacheOption = BitmapCacheOption.OnLoad;
            illustration.UriSource = new Uri(Path.Combine(root, "media", "scene.png"));
            illustration.EndInit();
            illustration.Freeze();
        }

        private void Text(DrawingContext dc, string text, double size, Brush color, double x, double y)
        {
            dc.DrawText(new FormattedText(text, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, font, size, color), new Point(x, y));
        }

        protected override void OnRender(DrawingContext dc)
        {
            double width = ActualWidth, height = ActualHeight;
            if (width <= 0 || height <= 0) return;
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, width, height)));
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(3, 10, 25)), null, new Rect(0, 0, width, height));
            double scale = Math.Max(width / illustration.PixelWidth, height / illustration.PixelHeight);
            scale *= 1.015 + .008 * Math.Sin(Seconds * .5);
            double imageWidth = illustration.PixelWidth * scale, imageHeight = illustration.PixelHeight * scale;
            dc.DrawImage(illustration, new Rect((width - imageWidth) / 2, (height - imageHeight) / 2, imageWidth, imageHeight));
            // Keep HUD geometry in a stable 1920 by 1080 coordinate system.
            dc.PushTransform(new ScaleTransform(width / 1920, height / 1080));
            Brush cyan = new SolidColorBrush(Color.FromRgb(100, 224, 255));
            Pen faint = new Pen(new SolidColorBrush(Color.FromArgb(65, 100, 224, 255)), 1);
            double reveal = Geometry.Ease(Seconds / 1.4);
            dc.PushOpacity(reveal);
            Text(dc, "A S T R A L   /   C O D E X", 18, cyan, 900, 190);
            Text(dc, "星 澜", 80, Brushes.White, 890, 236);
            Text(dc, "与你一起，开启新的创造", 24, Brushes.LightSteelBlue, 900, 350);
            Rect panel = new Rect(900, 445, 880, 410);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(175, 5, 19, 40)), faint, panel, 18, 18);
            Text(dc, "WORKSPACE  /  INITIALIZING", 17, cyan, 940, 478);
            dc.DrawLine(faint, new Point(940, 522), new Point(1740, 522));
            Text(dc, "准备好，把灵感变成现实。", 32, Brushes.White, 940, 567);
            Text(dc, Status, 21, Brushes.LightSteelBlue, 940, 640);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(25, 50, 72)), null,
                new Rect(940, 720, 800, 4), 2, 2);
            double progress = Math.Min(.92, Seconds / 5);
            dc.DrawRoundedRectangle(cyan, null, new Rect(940, 720, 800 * progress, 4), 2, 2);
            Text(dc, "SYSTEM ONLINE", 15, cyan, 940, 770);
            Text(dc, "ESC  跳过开场", 16, Brushes.LightSteelBlue, 1575, 770);
            dc.Pop();
            // Deterministic particles drift independently of the illustration.
            for (int i = 0; i < 36; i++)
            {
                double x = (i * 137.17 + Seconds * (9 + i % 5)) % 1920;
                double y = 1080 - (i * 83.71 + Seconds * (12 + i % 7)) % 1080;
                dc.PushOpacity(.15 + .3 * (1 + Math.Sin(Seconds + i)) / 2);
                dc.DrawEllipse(cyan, null, new Point(x, y), 1 + i % 3, 1 + i % 3);
                dc.Pop();
            }
            dc.PushOpacity(1 - Geometry.Ease(Seconds / .8));
            dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, 1920, 1080));
            dc.Pop();
            dc.Pop();
            dc.Pop();
        }
    }

    public sealed class BootWindow : Window
    {
        private readonly Config config;
        private readonly bool preview;
        private readonly Scene scene;
        private readonly MediaElement video;
        private bool videoOpened, videoHolding;
        private string mediaError;
        private readonly MediaPlayer sound = new MediaPlayer();
        private readonly Stopwatch clock = new Stopwatch();
        private readonly string root;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private IntPtr host, target, stableTarget, thumbnail;
        private double stableSince, lastProbe = -1, transitionAt = -1;
        private bool closing, ready, thumbnailFailed;
        private Bounds bounds;

        public BootWindow(Config settings, string root, bool isPreview)
        {
            config = settings;
            this.root = root;
            preview = isPreview;
            Title = "星澜 · Astral Codex";
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Background = Brushes.Black;
            Topmost = !preview;
            WindowStartupLocation = WindowStartupLocation.Manual;
            var screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
            bounds = preview ? new Bounds(screen.Left + 40, screen.Top + 40, 1152, 648)
                : new Bounds(screen.Left, screen.Top, screen.Width, screen.Height);
            Left = bounds.Left;
            Top = bounds.Top;
            Width = bounds.Width;
            Height = bounds.Height;
            string videoPath = Path.Combine(root, "media", "startup.mp4");
            if (File.Exists(videoPath))
            {
                video = new MediaElement
                {
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Manual,
                    Stretch = Stretch.Uniform,
                    ScrubbingEnabled = true,
                    Volume = config.Volume,
                    Source = new Uri(videoPath)
                };
                video.MediaOpened += delegate
                {
                    videoOpened = true;
                    if (!video.HasVideo || !video.NaturalDuration.HasTimeSpan ||
                        video.NaturalDuration.TimeSpan.TotalSeconds <= config.VideoTransitionEnd)
                        mediaError = "视频时长不足或没有可播放的画面，请检查视频和转场时间。";
                };
                video.MediaFailed += delegate(object sender, ExceptionRoutedEventArgs e)
                {
                    mediaError = "视频播放失败：" + e.ErrorException.Message;
                };
                video.MediaEnded += delegate { Finish(null); };
                Content = video;
            }
            else
            {
                scene = new Scene(root);
                Content = scene;
            }
            PreviewKeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape) Finish(null);
            };
            SourceInitialized += delegate
            {
                host = new WindowInteropHelper(this).Handle;
                Native.SetWindowPos(host, IntPtr.Zero, bounds.Left, bounds.Top, bounds.Width, bounds.Height, 0x4);
            };
            Loaded += delegate
            {
                clock.Start();
                if (!preview)
                {
                    target = Native.FindApp(config);
                    if (target == IntPtr.Zero) Launch();
                }
                if (closing) return;
                if (video != null) video.Play();
                else
                {
                    sound.Volume = config.Volume;
                    sound.Open(new Uri(Path.Combine(root, "media", "startup.wav")));
                    sound.Play();
                }
                timer.Start();
            };
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += Tick;
            Closed += delegate
            {
                closing = true;
                timer.Stop();
                sound.Close();
                if (video != null) video.Close();
                DropThumbnail();
            };
        }

        private void Launch()
        {
            object activation = null;
            try
            {
                activation = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C"), true));
                uint processId;
                int result = ((IApplicationActivationManager)activation).ActivateApplication(
                    config.AppLaunch.Substring("shell:AppsFolder\\".Length), null, 0, out processId);
                if (result < 0) Marshal.ThrowExceptionForHR(result);
            }
            catch (Exception error)
            {
                Finish("Codex 启动失败，请确认已安装客户端，并检查 launcher.json 的 AppLaunch。\n" + error.Message);
            }
            finally
            {
                if (activation != null && Marshal.IsComObject(activation)) Marshal.FinalReleaseComObject(activation);
            }
        }

        private void Tick(object sender, EventArgs args)
        {
            if (closing) return;
            double elapsed = clock.Elapsed.TotalSeconds;
            if (mediaError != null) { Finish(mediaError); return; }
            if (video != null && !videoOpened && elapsed > 12)
            {
                Finish("视频打开超时，请检查 Windows 媒体组件。");
                return;
            }
            if (scene != null)
            {
                scene.Seconds = elapsed;
                scene.InvalidateVisual();
            }
            if (preview)
            {
                if (scene != null)
                {
                    scene.Status = "动画预览 · 按 ESC 关闭";
                    if (elapsed >= config.IntroSeconds + config.TransitionSeconds + 1) Finish(null);
                }
                if (elapsed > config.MaxWaitSeconds) Finish("预览播放超时。");
                return;
            }
            if (elapsed - lastProbe >= .25)
            {
                lastProbe = elapsed;
                target = Native.FindApp(config);
                if (target == IntPtr.Zero || !Native.Responsive(target))
                {
                    stableTarget = IntPtr.Zero;
                    ready = false;
                }
                else if (stableTarget != target)
                {
                    stableTarget = target;
                    stableSince = elapsed;
                    ready = false;
                }
                else ready = elapsed - stableSince >= .6;
            }
            if (scene != null) scene.Status = ready ? "Codex 已就绪 · 正在展开工作区" : "正在等待 Codex 窗口…";
            if (elapsed >= config.MaxWaitSeconds)
            {
                Finish("等待 Codex 窗口超时。请检查客户端能否正常启动，以及 launcher.json 中的进程匹配配置。");
                return;
            }
            double position = video == null ? elapsed : video.Position.TotalSeconds;
            if (video != null)
            {
                if (!videoOpened) return;
                if (!ready && position >= config.VideoHoldAt && transitionAt < 0)
                {
                    if (!videoHolding)
                    {
                        video.Pause();
                        video.Position = TimeSpan.FromSeconds(config.VideoHoldAt);
                        videoHolding = true;
                    }
                    return;
                }
                if (ready && videoHolding)
                {
                    videoHolding = false;
                    video.Play();
                }
            }
            if (!ready) return;
            if (position < (video == null ? config.IntroSeconds : config.VideoTransitionStart)) return;
            if (transitionAt < 0)
            {
                Native.AlignWholeWindow(target, bounds);
                transitionAt = elapsed;
                thumbnailFailed = Native.DwmRegisterThumbnail(host, target, out thumbnail) != 0;
            }
            double progress = video == null
                ? Geometry.Ease((elapsed - transitionAt) / config.TransitionSeconds)
                : Geometry.Ease((position - config.VideoTransitionStart) /
                    (config.VideoTransitionEnd - config.VideoTransitionStart));
            if (!thumbnailFailed && thumbnail != IntPtr.Zero)
            {
                Bounds start = new Bounds((int)(bounds.Width * .469), (int)(bounds.Height * .412),
                    (int)(bounds.Width * .458), (int)(bounds.Height * .38));
                Native.ThumbProperties properties = new Native.ThumbProperties
                {
                    Flags = 1 | 4 | 8 | 16,
                    Destination = video == null
                        ? Geometry.Lerp(start, new Bounds(0, 0, bounds.Width, bounds.Height), progress)
                        : VideoDestination(position, bounds.Width, bounds.Height),
                    Opacity = (byte)Math.Round(255 * progress), Visible = true, ClientOnly = false
                };
                thumbnailFailed = Native.DwmUpdateThumbnailProperties(thumbnail, ref properties) != 0;
                if (thumbnailFailed) DropThumbnail();
            }
            if (thumbnailFailed) Opacity = 1 - progress;
            if (progress >= 1) Finish(null);
        }

        // Measured inner screen bounds for the supplied 1080p animation.
        // Late reveal avoids overlaying a rectangular window on the rotating screen.
        public static Bounds VideoDestination(double seconds, int width, int height)
        {
            double[,] frames = {
                { 8.25, .060, .065, .875, .860 },
                { 8.50, .045, .050, .910, .890 },
                { 9.00, .019, .020, .962, .947 },
                { 9.50, 0, 0, 1, 1 }
            };
            int next = 1;
            while (next < 3 && seconds > frames[next, 0]) next++;
            double amount = Geometry.Clamp((seconds - frames[next - 1, 0]) /
                (frames[next, 0] - frames[next - 1, 0]));
            double scale = Math.Min(width / 1920.0, height / 1080.0);
            double fitWidth = 1920 * scale, fitHeight = 1080 * scale;
            double left = frames[next - 1, 1] + (frames[next, 1] - frames[next - 1, 1]) * amount;
            double top = frames[next - 1, 2] + (frames[next, 2] - frames[next - 1, 2]) * amount;
            double frameWidth = frames[next - 1, 3] + (frames[next, 3] - frames[next - 1, 3]) * amount;
            double frameHeight = frames[next - 1, 4] + (frames[next, 4] - frames[next - 1, 4]) * amount;
            Bounds rectangle = new Bounds((int)Math.Round((width - fitWidth) / 2 + left * fitWidth),
                (int)Math.Round((height - fitHeight) / 2 + top * fitHeight),
                (int)Math.Round(frameWidth * fitWidth), (int)Math.Round(frameHeight * fitHeight));
            return Geometry.Lerp(rectangle, new Bounds(0, 0, width, height), Geometry.Ease((seconds - 9.5) / .4));
        }

        private void DropThumbnail()
        {
            if (thumbnail == IntPtr.Zero) return;
            Native.DwmUnregisterThumbnail(thumbnail);
            thumbnail = IntPtr.Zero;
        }

        private void Finish(string error)
        {
            if (closing) return;
            closing = true;
            if (!preview)
            {
                try
                {
                    File.WriteAllText(Path.Combine(root, "last-run.json"), new JavaScriptSerializer().Serialize(new
                    {
                        elapsedSeconds = clock.Elapsed.TotalSeconds,
                        videoOpened = videoOpened,
                        videoPosition = video == null ? 0 : video.Position.TotalSeconds,
                        targetFound = target != IntPtr.Zero,
                        transitionStarted = transitionAt >= 0,
                        thumbnailFallback = thumbnailFailed,
                        error = error
                    }));
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            Close();
            if (!preview && target != IntPtr.Zero) Native.SetForegroundWindow(target);
            if (error != null) MessageBox.Show(error, "星澜启动器");
        }
    }

    public static class Entry
    {
        public static Config ReadConfig(string root)
        {
            Config config = new JavaScriptSerializer().Deserialize<Config>(File.ReadAllText(Path.Combine(root, "launcher.json")));
            if (config == null || String.IsNullOrWhiteSpace(config.AppLaunch) ||
                !System.Text.RegularExpressions.Regex.IsMatch(config.AppLaunch, @"^shell:AppsFolder\\[A-Za-z0-9_.-]+![A-Za-z0-9_.-]+$") ||
                String.IsNullOrWhiteSpace(config.ProcessPathContains) || config.ProcessNames == null || config.ProcessNames.Length == 0 ||
                config.ProcessNames.Any(name => String.IsNullOrWhiteSpace(name) || !System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z0-9_.-]+$")))
                throw new Exception("launcher.json 中的应用标识或进程配置无效。");
            double[] values = { config.Volume, config.IntroSeconds, config.TransitionSeconds, config.MaxWaitSeconds, config.VideoHoldAt, config.VideoTransitionStart, config.VideoTransitionEnd };
            if (values.Any(value => Double.IsNaN(value) || Double.IsInfinity(value)) || config.Volume < 0 || config.Volume > 1 ||
                config.IntroSeconds < 1 || config.TransitionSeconds < .2 || config.MaxWaitSeconds < config.IntroSeconds + config.TransitionSeconds + 1 || config.MaxWaitSeconds > 300)
                throw new Exception("音量须在 0 到 1 之间，动画时间和等待时间配置须有效，最长等待为 300 秒。");
            if (config.VideoHoldAt <= 0 || config.VideoTransitionStart <= config.VideoHoldAt ||
                config.VideoTransitionEnd <= config.VideoTransitionStart || config.VideoTransitionEnd >= config.MaxWaitSeconds)
                throw new Exception("视频等待点、转场开始和结束时间必须依次递增，且小于最长等待时间。");
            foreach (string asset in File.Exists(Path.Combine(root, "media", "startup.mp4"))
                ? new[] { "startup.mp4" } : new[] { "scene.png", "startup.wav" })
                if (!File.Exists(Path.Combine(root, "media", asset))) throw new FileNotFoundException("缺少素材 " + asset);
            return config;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                Config config = ReadConfig(root);
                if (args.Contains("--self-test"))
                {
                    Bounds end = Geometry.Lerp(new Bounds(100, 200, 800, 600), new Bounds(0, 0, 1920, 1080), 1);
                    if (end.Width != 1920 || end.Left != 0 || Geometry.Ease(0) != 0 || Geometry.Ease(1) != 1)
                        throw new Exception("Geometry self-test failed.");
                    foreach (int[] size in new[] { new[] { 1920, 1080 }, new[] { 2560, 1440 }, new[] { 3440, 1440 }, new[] { 1080, 1920 } })
                    {
                        for (double time = 8.25; time < 10; time += .05)
                        {
                            Bounds frame = BootWindow.VideoDestination(time, size[0], size[1]);
                            if (frame.Left < 0 || frame.Top < 0 || frame.Right > size[0] || frame.Bottom > size[1] || frame.Width <= 0 || frame.Height <= 0)
                                throw new Exception("Video destination outside monitor.");
                        }
                        Bounds final = BootWindow.VideoDestination(9.9, size[0], size[1]);
                        if (final.Left != 0 || final.Top != 0 || final.Width != size[0] || final.Height != size[1])
                            throw new Exception("Video handoff does not fill monitor.");
                    }
                    File.WriteAllText(Path.Combine(root, "self-test.json"), "{\"ok\":true,\"configuration\":true,\"assets\":true,\"geometry\":true}");
                    return 0;
                }
                if (args.Contains("--snapshot"))
                {
                    Scene scene = new Scene(root) { Seconds = 3.5 };
                    scene.Measure(new Size(1920, 1080));
                    scene.Arrange(new Rect(0, 0, 1920, 1080));
                    scene.UpdateLayout();
                    var bitmap = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(scene);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using (var stream = File.Create(Path.Combine(root, "preview.png"))) encoder.Save(stream);
                    return 0;
                }
                bool preview = args.Contains("--preview");
                bool owns;
                using (var mutex = new Mutex(true, preview ? "Local\\AstralCodexPreview" : "Local\\AstralCodexLauncher", out owns))
                {
                    if (!owns) return 0;
                    var app = new Application();
                    return app.Run(new BootWindow(config, root, preview));
                }
            }
            catch (Exception error)
            {
                if (args.Contains("--self-test") || args.Contains("--snapshot"))
                    File.WriteAllText(Path.Combine(root, "error.txt"), error.ToString());
                else MessageBox.Show(error.Message, "星澜启动器");
                return 1;
            }
        }
    }
}
