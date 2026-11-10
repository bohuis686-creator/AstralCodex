# Astral Codex · 星澜

[English](README.en.md) · [简体中文](README.md) · [Downloads](https://github.com/bohuis686-creator/AstralCodex/releases) · [Contributing](CONTRIBUTING.md)

[![Desktop launch recording](demo/launch-demo.gif)](demo/launch-demo.mp4)

Click the animation for the full recording with audio. The looping preview shows the desktop launch, character animation and handoff to Codex. Full media and details are in the [demo directory](demo/README.md).

An anime startup animation for the Windows Codex desktop client. Help us extend the same animation-to-window experience to more desktop applications. Contributions in code, animation, translation, testing and design are welcome.

![Character concept](media/scene.png)

The included 1080p video shows an original silver-haired character opening her eyes and summoning a holographic screen. The launcher starts Codex in parallel, then places a live DWM window thumbnail inside the screen and expands it into the actual application. Audio comes from the video, with a default volume of 30%.

## Try it

Download `AstralCodex-Windows.zip` from Releases and extract the entire archive to a permanent folder. Run `Preview.cmd` to preview the animation or `AstralCodex.exe` to launch Codex. Press Esc to skip.

Run `Install-Desktop.cmd` from the extracted runtime folder to retarget existing desktop shortcuts for the configured Codex client. It preserves their icons and backs up the originals. `Restore-Desktop.cmd` restores them. Keep the `.integration` backup folder and the runtime folder in place after installation.

Requires 64-bit Windows 10/11, .NET Framework 4.x/WPF, Windows media components and an installed Codex desktop client.

## Build

Run `Build.cmd`, or run this command from the source root.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

Output goes to `build/AstralCodex`. Builds preserve an existing runtime `launcher.json` and refresh bundled media. The script runs configuration, asset and transition geometry checks. CI performs the same build on Windows.

## Configure

Edit `launcher.json` in the runtime folder. `Volume` ranges from 0 to 1. `VideoHoldAt` defaults to 7.5 seconds, `VideoTransitionStart` to 8.25 and `VideoTransitionEnd` to 9.9. `MaxWaitSeconds` defaults to 60. A slow launch pauses the video before the transition until a stable, responsive client window is found.

`AppLaunch` is a `shell:AppsFolder\<AppID>` identifier. `ProcessNames` and `ProcessPathContains` select the client. Transition geometry in `source/Launcher.cs`, method `VideoDestination`, is calibrated for the supplied video. Different media needs new calibration.

The launcher resizes the selected Codex window to the monitor under the pointer. DWM failure uses a fade fallback. Window readiness is inferred from visibility and responsiveness. Esc leaves an already-issued application launch request running. The included video is 1920×1080, 24 fps, about 10.08 seconds, with AAC audio. Without `media/startup.mp4`, the legacy illustration mode uses `scene.png` and `startup.wav`. `--snapshot` exports only the legacy illustration scene.

## Build with us

The current preview supports Codex through MSIX activation. Win32 EXE launch, application profiles, configurable animation keyframes and broader display support are open contribution areas. See [Contributing](CONTRIBUTING.md), [extension design](docs/EXTENDING.md), and [open issues](https://github.com/bohuis686-creator/AstralCodex/issues).

Locally verified with build checks, video preview and handoff to an already-open Codex window. Cold starts, multiple monitors, mixed DPI and other client packages need wider community testing.

## Credits and license

Windows interop code is adapted from [Codex-WhiteDragon-AutoLaunch](https://github.com/suyuan132/Codex-WhiteDragon-AutoLaunch). Original notices are retained in LICENSE. Code is MIT licensed. AI-generated illustration, user-provided Doubao video and audio have their own provenance and service terms, described in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). This is a community compatibility tool.
