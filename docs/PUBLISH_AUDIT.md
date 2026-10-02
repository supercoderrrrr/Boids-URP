# Publication Audit / 发布审查

## English

- Source workspace: the Boids learning project also contains an FFT ocean folder and FFT Renderer Feature
- Public content: the six original runtime scripts, cleaned core comments, self-contained fullscreen distortion graph/material, sanitized Unity settings, generated public demo and publication utilities
- Excluded: FFT code, FFT scene, FFT Renderer Feature, licensed art/plugins, imported textures of unverified provenance, embedded skyboxes, editor caches and workstation configuration
- Removed from publication settings: Unity cloud project ID, organization ID and project name
- SCM additions: the scene-referenced `SampleSceneProfile.asset` and its meta file were Private and are preserved in CS6
- Dependency follow-up: Unity's scene dependency graph identified another 96 required Private items; CS7 preserves them in the existing SCM repository only
- Post-check-in verification: none of the dependency paths selected by that audit remains Private
- SCM cleanup: CS6 changes the three core scripts' comments only; executable token comparison passed
- SCM metadata: preserve Boids CS0-CS4 and the actual publication check-in; exclude FFT CS5; omit owner email, server/organization identifiers and absolute workspace paths
- Git identity: use the account's GitHub no-reply address for the new repository
- History: initial Git commit is a current snapshot; no fabricated historical Git commits or dates

The local source workspace has other pending FFT and third-party changes. A scoped Boids check-in does not claim to commit those unrelated changes. Raw Plastic client/workspace files and authentication configuration are never included in the public repository.

## 简体中文

- 原始 Boids 学习工程同时包含 FFT 海洋目录和 Renderer Feature
- 公开内容为原有六个运行脚本、核心英文注释、独立扰动图与材质、脱敏工程设置，以及生成的演示和发布工具
- 排除 FFT、第三方美术与插件、来源未确认的贴图、内嵌天空包、本地缓存和工作站配置
- 发布设置移除 Unity 云项目、组织和项目名称标识
- `SampleSceneProfile.asset` 及其 `.meta` 原为 Private，CS6 已补充保存
- 进一步按 Unity 场景依赖表查到 96 项必要 Private 文件，CS7 仅将它们补充到原 SCM 仓库
- 提交后复核：本次依赖表选出的必要路径已没有 Private 项
- CS6 对三个核心脚本只整理注释，可执行 token 对照通过
- SCM 元数据保留 Boids CS0-CS4 与真实整理提交，排除 FFT CS5，去掉邮箱、云服务器和组织标识及本机绝对路径
- 新 Git 仓库使用 GitHub no-reply 邮箱
- 首次 Git 提交是当前版本快照，不补造旧的 Git 提交与日期

本地共享工程仍有 FFT 和第三方的其他待提交修改。本次限定范围的提交不宣称保存了那些无关改动。Plastic 客户端、工作区和认证原始文件不进入公开仓库。
