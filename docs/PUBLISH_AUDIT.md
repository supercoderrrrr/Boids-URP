# Publication Audit / 发布审查

## English

The 2026-10-03 update publishes the new schooling runtime, URP presentation shaders, authored arch/cliff meshes, Blender source, reproducible public scene and current showcase media. `Assets/BoidsUnderwaterScene/Scenes/Boids.unity` is the only distributed scene. The older local scene is named `test` and remains outside this repository.

Licensed fish and environment packages, their baked derivatives, imported textures of unverified provenance, FFT files, workstation configuration and editor caches are excluded. The public builder creates replacement fish, corals, outcrops and textures without copying those package files. Unity cloud project and organization identifiers remain empty in publication settings.

The first Git commit imports the earlier publication snapshot. The latest update is a new Git commit with the actual publication date. Real Plastic SCM metadata remains preserved without owner emails, server names or absolute workspace paths. The previous CS6 comment-only audit is retained as historical evidence; later steering and camera fixes deliberately change executable behavior.

Publication checks cover excluded dependencies, local Markdown links, text credential/path patterns, English comment style, scene count and file size. Unity checks and runtime evidence are recorded in [VALIDATION.md](VALIDATION.md). Showcase and public-scene results are identified separately because they use different art assets.

## 简体中文

2026-10-03 更新包含新版集群代码、URP 展示着色器、制作的拱门和岩壁、Blender 源文件、可重建的公开场景与新展示素材。仓库只分发 `Boids.unity`，本地旧 `test` 场景保留在原工程中。

有许可限制的鱼和环境包、烘焙衍生网格、来源未确认的贴图、FFT 文件、本机配置和编辑器缓存均不公开。公开构建工具生成替代鱼、珊瑚、岩石与贴图，发布设置中的 Unity 云项目和组织标识保持为空。

首次 Git 提交仍是早期发布快照，最新改进通过当前日期的真实 Git 提交记录。Plastic SCM 元数据保留，账户邮箱、服务器和本机路径去除。CS6 纯注释审查属于历史证据，后来的转向与相机修复确实修改了行为。

发布检查覆盖资源依赖、Markdown 链接、凭据和本机路径模式、英文注释、场景数量与文件大小。Unity 及运行验证见 [VALIDATION.md](VALIDATION.md)，展示版与公开版的验证分开记录。
