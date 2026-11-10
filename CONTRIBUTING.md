# 一起贡献 · Contributing

[中文首页](README.md) · [English README](README.en.md) · [扩展设计 / Extension design](docs/EXTENDING.md)

欢迎首次参与开源的朋友。代码、测试、翻译、动画制作和问题复现都可以成为贡献。可以直接修复小问题，大功能请先开 Issue 说明使用场景、方案和验证方式。中英文交流都欢迎。

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
