# Development Archive / 开发归档

[English](#english) | [简体中文](#简体中文)

## English

### Why the First Git Commit Contains the Project

The project was developed in Unity Version Control / Plastic SCM before publication on GitHub. Its first Git commit imports the current Boids publication snapshot. Earlier work is documented by real Plastic changesets, not reconstructed or backdated Git commits.

The original workspace also contains an FFT ocean implementation. This repository publishes the Boids system and its underwater presentation. The FFT-specific CS5 is excluded from the Boids archive; it remains in the shared SCM repository.

### Recorded Development

| Record | Date (UTC+08:00) | Work | Current Entry Points |
| --- | --- | --- | --- |
| CS1 | 2026-08-03 | Agent/manager structure; separation, alignment, cohesion; bounded steering and speed; box confinement; SphereCast; Fibonacci directions; wander and vortex controls | [BoidAgent](Assets/Scripts/BoidAgent.cs), [BoidManager](Assets/Scripts/BoidManager.cs), [BoidHelper](Assets/Scripts/BoidHelper.cs) |
| CS2 | 2026-08-04 | Boids optimization; current implementation accumulates three rules in one traversal and uses squared-distance filtering | [CalculateFlockingForces](Assets/Scripts/BoidAgent.cs) |
| CS3 | 2026-08-04 | Environment assembly, post-processing, underwater fog and a shader-based distortion experiment | [UnderwaterFogController](Assets/Scripts/UnderwaterFogController.cs), [fullscreen graph](Assets/Shader/SGF_UnderwaterFullscreenDistortion.shadergraph) |
| CS4 | 2026-08-05 | Caustics presentation in the original scene | Third-party caustics resources omitted from this public repository |
| CS6 | 2026-10-02 | Preserve the missing Volume profile; normalize core comments to English; verify executable tokens are unchanged | [SCM history](docs/scm/HISTORY.md), [publication audit](docs/PUBLISH_AUDIT.md) |
| CS7 | 2026-10-02 | Preserve 96 required Private scene dependency items in SCM after a Unity dependency audit; imported art stays out of public GitHub | [SCM history](docs/scm/HISTORY.md) |
| Git update | 2026-10-03 | 3D spatial grid, shared simulation snapshots, corridor guidance, independent vortex, instanced fish and refreshed showcase media | [BoidsSchool](Assets/BoidsUnderwaterScene/Scripts/BoidsSchool.cs), [validation](docs/VALIDATION.md) |

The brief historical comments are preserved verbatim in [HISTORY.md](docs/scm/HISTORY.md). The technical descriptions above also reference the current code; they should not be read as proof that every current detail existed in each historical revision. `changesets.json` contains repository-relative changed paths, comments and actual IDs. It is a metadata archive, not a full export of historical source revisions.

### GitHub Publication Work

The release copy separates Boids from FFT renderer dependencies, excludes third-party source assets and local caches, and removes Unity cloud project/organization identifiers. The original working scenes and FFT files are retained locally.

A generated fish mesh, scene builder and command-line capture utility initially provided a runnable public demo. The latest update replaces that scene with `Boids`, sharing the new schooling runtime and routes while generating replacement assets for excluded licensed art. The older local scene is named `test`; it is not distributed in the current repository.

Core comment cleanup removed obsolete commented-out experiments and retained short English explanations without trailing periods. A lexical check verified that non-comment executable tokens in the three edited original scripts were identical before and after cleanup. This is a style check, not an authorship classifier.

### Schooling Update

Neighbor queries now use a reusable 3D spatial grid, followed by exact distance filtering. Steering reads a shared snapshot before integration. A 30 Hz simulation and interpolated instanced draws separate simulation frequency from display refresh, with vertex tail animation replacing per-fish animation objects.

Three schools follow closed corridors with individual offsets and speed variation. Staggered route-progress correction keeps the faster distant ribbon coherent. The fourth school uses tangential and radial vortex guidance with full-height startup population. Regression checks compare the three traveling schools with and without the vortex to verify isolation.

The scene presentation was refined with a wider animated water surface, shadowed underwater light shafts and clearer particles. Showcase recordings and screenshots now use only the latest composition. The original scripts also gained a zero-direction steering guard, initialization safety and signed camera pitch/cursor cleanup fixes; these are behavioral changes after the earlier comment-only check-in.

### Validation and Next Steps

The publication uses Unity 2022.3.62f2 / URP 14.0.12. Specific import, player-build, runtime and media checks are recorded in [VALIDATION.md](docs/VALIDATION.md), including any unresolved limitations.

Future work: measured scaling across agent counts, denser-school collision handling and Burst/Jobs or GPU simulation. The grid, snapshots and corridor guidance described above are implemented; the remaining items are not claimed as complete.

---

## 简体中文

### 为什么首次 Git 提交包含完整项目

项目开发阶段使用 Unity Version Control / Plastic SCM，之后才发布到 GitHub。首次 Git 提交导入当前 Boids 发布快照。此前开发过程由真实 Plastic 变更集记录，没有重建或回填 Git 提交日期。

原工程也包含 FFT 海洋实现。本仓库发布 Boids 与水下展示部分。FFT 专属 CS5 未纳入 Boids 归档，仍保留在共享 SCM 仓库中。

### 已记录的开发过程

| 记录 | 时间（UTC+08:00） | 内容 | 当前代码入口 |
| --- | --- | --- | --- |
| CS1 | 2026-08-03 | Agent/Manager、分离/对齐/聚合、转向力与速度约束、Box 边界、SphereCast、Fibonacci 方向、Wander 和旋涡 | [BoidAgent](Assets/Scripts/BoidAgent.cs)、[BoidManager](Assets/Scripts/BoidManager.cs)、[BoidHelper](Assets/Scripts/BoidHelper.cs) |
| CS2 | 2026-08-04 | Boids 优化；当前代码将三规则合并到一次遍历并使用距离平方筛选 | [CalculateFlockingForces](Assets/Scripts/BoidAgent.cs) |
| CS3 | 2026-08-04 | 环境搭建、后处理、水下雾和 Shader 扰动实验 | [UnderwaterFogController](Assets/Scripts/UnderwaterFogController.cs)、[全屏扰动图](Assets/Shader/SGF_UnderwaterFullscreenDistortion.shadergraph) |
| CS4 | 2026-08-05 | 原始场景的焦散展示 | 第三方焦散资源未随本仓库分发 |
| CS6 | 2026-10-02 | 补充缺失的 Volume Profile、统一核心英文注释、验证可执行代码未改变 | [SCM 历史](docs/scm/HISTORY.md)、[发布审查](docs/PUBLISH_AUDIT.md) |
| CS7 | 2026-10-02 | 按 Unity 依赖表补充 96 项必要 Private 场景依赖，仅保存到 SCM，第三方美术不进入公开 GitHub | [SCM 历史](docs/scm/HISTORY.md) |
| Git 更新 | 2026-10-03 | 三维空间网格、统一模拟快照、通道引导、独立漩涡、实例化鱼群与新版展示素材 | [BoidsSchool](Assets/BoidsUnderwaterScene/Scripts/BoidsSchool.cs)、[验证](docs/VALIDATION.md) |

历史原始日志保存在 [HISTORY.md](docs/scm/HISTORY.md)。上表同时参考了当前代码，不意味着每一项当前细节都已经存在于对应历史版本。`changesets.json` 保存真实编号、说明和仓库相对文件路径，属于元数据归档，不是历史源码的完整导出。

### 本次公开整理

发布副本分离 Boids 与 FFT Renderer 依赖，排除第三方源资源与本地缓存，并移除 Unity 云项目和组织标识。本地原场景和 FFT 文件保留。

初次发布时添加了生成鱼网格、场景构建与命令行录制工具。最新更新用正式 `Boids` 替换旧演示，公开版本复用新集群代码与路线，并为未公开的许可资源生成替代资产。本地旧场景改名为 `test`，当前 GitHub 不再分发旧场景。

核心注释整理移除了过时的注释掉代码，保留简洁英文解释并去掉句末句号。词法对照确认三个原始脚本在整理前后具有相同的非注释可执行 token。这属于代码风格核验，不能用来判定代码作者或生成来源。

### 集群更新

邻居查询改用可复用三维空间网格，再按实际距离筛选；所有转向读取统一快照后才积分。30 Hz 模拟与实例化插值显示分离更新频率，摆尾由顶点着色器完成。

三组鱼群沿闭合通道游动并保留偏移和速度差异，错开执行的路线进度校正让较快的远景群体保持流向。第四组使用切向与径向漩涡引导，开场已覆盖完整高度；回归检查对比流动鱼群单独运行和同时运行漩涡时的位置，验证它们相互独立。

另外精修了场景，补充覆盖更广的动态水面、带阴影的水下丁达尔光与更清晰的粒子。截图与录像全部替换为新版本。原脚本也增加了零方向转向保护、初始化检查、相机初始角度与鼠标释放修复，这些是早期纯注释提交之后的行为改动。

### 验证与后续方向

发布环境为 Unity 2022.3.62f2 / URP 14.0.12，导入、构建、运行和展示素材的具体核验见 [VALIDATION.md](docs/VALIDATION.md)，未验证的限制也在其中说明。

后续方向包括不同个体数量下的实测性能、密集群体的碰撞处理，以及 Burst/Jobs 或 GPU 模拟。上述网格、快照与通道引导已经实现，其余方向尚未完成。
