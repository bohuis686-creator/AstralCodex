# 一起贡献 · Contributing

[中文首页](README.md) · [English README](README.en.md) · [扩展设计 / Extension design](docs/EXTENDING.md)

欢迎首次参与开源的朋友。代码、测试、翻译、动画制作和问题复现都可以成为贡献。可以直接修复小问题，大功能请先开 Issue 说明使用场景、方案和验证方式。中英文交流都欢迎。

## 当前可以贡献什么

| 类别 | 可以提交的成果 | 怎样验证 |
| --- | --- | --- |
| 新软件适配 | 普通 EXE 的启动路径、参数、工作目录和主窗口识别实现，附一个具体软件的配置示例 | 从快捷方式启动，检查等待、交接、Esc 与恢复行为，记录软件版本 |
| 动画与音效 | 原创 MP4、音效、角色设计或主题，附来源和使用范围说明 | 提供完整预览，标注屏幕内沿的时间和位置，确认音量与最后一帧衔接 |
| 转场与主题配置 | 把硬编码关键帧移入配置，改进旋转屏幕贴合、淡入或窗口尺寸处理 | 构建通过，旧配置可用，展示修改前后的衔接效果 |
| 测试与 Bug 修复 | 冷启动、双屏、混合 DPI、超宽屏、竖屏等测试记录，或具体问题的修复 PR | 提供环境、复现步骤、预期结果和实际结果，修复后复测 |
| 文档与翻译 | 中英文教程、常见问题、配置示例、录屏演示或其他语言说明 | 按文档实际操作，确认命令、路径与链接有效 |
| 使用体验与设计 | 设置界面设计、主题选择流程、快捷方式安装与恢复流程改进 | 给出草图或可运行实现，说明用户操作步骤，功能按实现范围验证 |

只会测试、画图、做视频或翻译，也欢迎参与。先在 Issue 里说明自己想做的部分，再提交成果或 PR。当前可领取的任务见 [Issues](https://github.com/bohuis686-creator/AstralCodex/issues)。

## 从一个小任务开始

- 补充英文说明或改进安装说明。
- 测试不同分辨率、DPI、冷启动与双屏，并提供可复现步骤。
- 将当前硬编码的窗口关键帧迁移到主题配置，保留旧配置行为。
- 为普通 Win32 EXE 设计启动配置与参数处理。
- 制作新的原创动画主题，标定屏幕内沿位置并说明素材来源。

## 提交步骤

1. Fork 仓库，创建自己的分支。
2. 完成一项具体修改，运行 `Build.cmd`。
3. 涉及界面时运行 `build/AstralCodex/Preview.cmd`，涉及启动和窗口交接时实际测试对应软件。
4. 提交 PR，说明修改目的、使用方式和实际验证结果。对尚未验证的环境明确标注。

保持改动集中、代码易读。修复已有问题时保留兼容行为。快捷方式变更应保留备份、验证目标，并支持恢复。报告问题前请移除日志中的本机用户名、私有路径和敏感界面内容。

代码贡献沿用 MIT 许可证并保留原有声明。动画、图片、音频请注明来源、生成工具和允许使用的范围。素材条款单独记录在 THIRD_PARTY_NOTICES.md。

## English

First-time contributors are welcome. Start with a focused bug fix, documentation improvement, translation, reproducible test report or an original animation theme. Open an issue before a substantial feature to discuss the use case, design and validation.

Fork the repository, create a branch, make a focused change and run `Build.cmd`. Use `Preview.cmd` for visual changes and manually test actual application startup for launch or handoff changes. Describe what you verified in the pull request, including unsupported or untested environments. Preserve compatibility, shortcut backups and restoration behavior.

Code contributions follow the existing MIT license. For media, document provenance and applicable usage terms separately. Remove private paths, usernames and sensitive window content from diagnostic reports.

## Contribution deliverables

| Area | What to submit | Validation |
| --- | --- | --- |
| Application support | Win32 launch implementation and a concrete app profile with executable, arguments, working directory and window matching | Test launch, wait, handoff, Esc and shortcut restoration, with the app version |
| Animation and audio | Original MP4, sound, character artwork or a theme with provenance and usage terms | Include a preview and measured screen bounds over time, check audio and final-frame alignment |
| Transitions and themes | Configurable keyframes, improved screen alignment, fades or window geometry | Pass the build, preserve old configs and show before/after results |
| Testing and fixes | Cold-start, multi-monitor, mixed-DPI, ultrawide or portrait reports, or a focused fix | Include environment, steps, expected/actual results and a retest |
| Documentation | Translations, tutorials, FAQ, config examples or demo recordings | Follow the instructions and verify commands and links |
| UX and design | Settings/theme-selection designs or shortcut setup and restoration improvements | Include a mockup or implementation and describe the user flow |

You can contribute without programming. Open an issue describing the part you want to work on, then share your results or submit a focused PR.
