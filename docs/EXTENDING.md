# 扩展到更多软件 · Extending to other applications

星澜希望成为可以共同扩展的动画启动器项目。当前实现以 Codex 为首个可运行案例。

## 可复用的流程

播放动画时并行启动目标软件，寻找主窗口，等待窗口稳定，将实时窗口画面放入动画预留的屏幕区域，跟随关键帧放大，最后释放动画层并移交输入。

`source/Launcher.cs` 负责播放、等待和交接。`source/Native.cs` 提供进程及窗口发现、Windows 应用激活接口与 DWM 互操作。`scripts/integrate-entrypoints.ps1` 管理匹配的快捷方式、备份和恢复。

## 欢迎贡献的扩展方向

| 方向 | 当前行为 | 可交付的第一步 |
| --- | --- | --- |
| 普通 EXE | AppLaunch 只接受 MSIX AppsFolder 标识 | 提出带类型的启动配置，支持可执行路径、参数、工作目录，兼容现有配置 |
| 主窗口发现 | 按进程名、路径片段和可见性匹配 | 支持启动器子进程、多个窗口，避免匹配其他应用 |
| 动画主题 | 当前视频使用源码内的位置关键帧 | 将关键帧迁移到主题文件，验证时间递增和坐标范围 |
| 软件配置 | Codex 默认配置 | 设计可选择的软件档案，并说明适配版本及测试结果 |
| 显示器适配 | 鼠标所在屏幕，保持视频比例 | 混合 DPI、超宽屏、竖屏、多屏与远程桌面测试 |

调整 AppLaunch 字符串不足以适配普通 EXE。启动逻辑、配置校验、进程发现和快捷方式识别需要配套修改。建议先用一个具体软件完成端到端适配，再讨论通用接口。

## English

The reusable pipeline is animation playback → parallel app launch → stable main-window discovery → live-window composition inside the animation → final input handoff.

Current activation and validation accept MSIX AppsFolder identifiers. Win32 support needs a typed launch configuration with executable path, arguments and working directory, plus process/window matching and shortcut integration. Preserve existing configs and avoid matching unrelated apps. A concrete end-to-end adapter for one application is a useful first contribution.

Theme work can move the hardcoded screen bounds into validated keyframe files. Application profiles can package launch configuration and documented compatibility. Display testing should cover mixed DPI, ultrawide and portrait screens, multiple monitors and remote desktop. Proposed contributions and real-world test reports are welcome through Issues and PRs.
