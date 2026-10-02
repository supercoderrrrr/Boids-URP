# Validation / 验证

Validated on 2026-10-02 with Unity 2022.3.62f2, URP 14.0.12, Windows x64 and Direct3D 11.

| Check | Result |
| --- | --- |
| Public Unity project import and C# compilation | Passed |
| Public Windows Development Build | Passed |
| Isolated original-scene Windows Development Build | Passed |
| Scene script/material/prefab/camera/bounds validation | Passed, zero findings |
| Renderer dependency validation | No missing features, FFT features or third-party caustics features in public project |
| Original core comment edits | Identical executable tokens before/after; public scripts match cleaned source scripts |
| Public runtime recording | 240 agents, 150 frames, no captured Error/Exception messages |
| Original-scene runtime recording | 400 agents, 150 frames, no captured Error/Exception messages |
| Rendered frames | Nonblank 1280 × 720 frames; frame differences confirmed motion; representative frames visually reviewed |
| Encoded media | H.264 MP4 and looping GIF previews produced; real camera stills included |
| SCM preservation | Actual CS6 comment/profile check-in and CS7 required-dependency check-in completed |
| Public text scan | No matched token/private-key/email/workspace-path/source-cloud-ID patterns |

Machine-readable reports: [publication-validation.json](publication-validation.json), [comment-audit.json](comment-audit.json), [original capture](media/original-underwater-capture.json) and [public capture](media/public-demo-capture.json).

The builds contain some Unity shader fallback/dependency warnings during import/stripping. They completed successfully; the representative camera frames do not show magenta error materials. This does not establish correctness for unused shaders or other graphics backends.

This is an import/build/runtime and publication check, not a full automated behavioral test suite. No isolated EditMode or PlayMode regression suite existed for the original Boids scripts. Keyboard and cursor handling were inspected but not exercised through real desktop input during these command-line captures.

No runtime FPS, memory benchmark, large-scale stress test, hard-boundary guarantee or zero-overlap guarantee is claimed. Linux, macOS, mobile, XR and alternate graphics APIs are unverified. Fullscreen distortion remains disabled and is not claimed as a validated finished effect. The local original project retains unrelated pending FFT and imported-asset changes.

## 简体中文

本次于 2026-10-02 使用 Unity 2022.3.62f2、URP 14.0.12、Windows x64 和 Direct3D 11 验证。公开工程导入、脚本编译、两套场景开发构建以及公开场景引用检查通过；原场景 400 个、公开演示 240 个个体的录制未捕获 Error 或 Exception。

实际画面已检查非空、帧间变化和代表静帧，MP4 与 GIF 已生成。核心注释整理前后可执行 token 相同，公开核心脚本与整理后的原脚本一致。CS6 和 CS7 均为真实 SCM 提交，公开文本扫描未匹配账户邮箱、凭据、本机路径或源工程云标识。

Unity 构建过程中存在部分 Shader fallback/依赖警告，构建成功且代表画面未显示粉色错误材质；这不代表所有未使用 Shader 与图形后端均已验证。项目原先没有独立的 Boids 回归测试套件，键盘与鼠标只检查了实现，没有在命令行录制中进行真实桌面输入测试。

本次不宣称实测 FPS、不穿模或绝不越界，其他平台和图形 API 尚未测试，全屏扰动仍禁用。本地原工程仍保留其他待提交的 FFT 与资源修改。
