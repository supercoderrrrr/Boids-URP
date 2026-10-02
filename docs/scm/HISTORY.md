# Plastic SCM History / Plastic SCM 历史

Actual changeset IDs, timestamps, original comments and repository-relative paths exported from Plastic SCM. Personal email, cloud identifiers and local workspace paths are omitted. CS5 concerns FFT ocean work and is excluded from this Boids archive. CS0 is repository initialization and has no comment or changed items.

以下编号、时间、原始提交说明和仓库相对路径直接导出自 Plastic SCM。账户邮箱、云组织标识和本机路径已移除。CS5 属于 FFT 海洋工作，未纳入本 Boids 归档。CS0 是仓库初始化，没有提交说明和变更文件。

## CS0 | 2026-08-02T14:25:52+08:00 | /main

(No check-in comment / 无提交说明)

Changed items: 0

<details><summary>Changed paths / 文件清单</summary>


</details>

## CS1 | 2026-08-03T18:05:03+08:00 | /main

实现 Unity Boids 鱼群集群模拟基础版本

- 搭建 BoidAgent / BoidManager 结构，支持批量生成和管理鱼群个体。
- 实现 Boids 核心三规则：分离、对齐、聚合，通过局部邻域检测驱动群体行为。
- 使用速度、期望速度、转向力的 Steering Behavior 模型控制个体运动，并限制最大速度和最大转向力。
- 实现基于 BoxCollider 的鱼缸游动范围约束，使 Boid 在长方体空间内活动。
- 实现基于 Physics.SphereCast 的前向障碍物检测与 LayerMask 障碍过滤。
- 使用黄金螺旋 / Fibonacci Sphere 生成球面候选方向，用于平滑选择避障方向。
- 加入 Wander 自由游动力，使鱼群具备更自然的随机游动表现。
- 加入 Vortex Field 混合旋涡力，用切向力和径向力辅助维持鱼群动态形状。
- 将速度、感知半径、规则权重、鱼缸边界、避障、自由游动和旋涡力参数暴露到 Inspector，方便后续调参与作品集展示。

Changed items: 37

<details><summary>Changed paths / 文件清单</summary>

- `Added` `/Assets`
- `Added` `/ProjectSettings`
- `Added` `/Assets/Material`
- `Added` `/Assets/Material/Mat_Boid.mat`
- `Added` `/Assets/Material/Mat_Boid.mat.meta`
- `Added` `/Assets/Material/Mat_Box.mat`
- `Added` `/Assets/Material/Mat_Box.mat.meta`
- `Added` `/Assets/Material/Mat_Obstacle.mat`
- `Added` `/Assets/Material/Mat_Obstacle.mat.meta`
- `Added` `/Assets/Material.meta`
- `Added` `/Assets/Prefabs`
- `Added` `/Assets/Prefabs/BoidAgent.prefab`
- `Added` `/Assets/Prefabs/BoidAgent.prefab.meta`
- `Added` `/Assets/Prefabs.meta`
- `Added` `/Assets/Scenes`
- `Added` `/Assets/Scenes/SampleScene.unity`
- `Added` `/Assets/Scenes/SampleScene.unity.meta`
- `Added` `/Assets/Scripts`
- `Added` `/Assets/Scripts/BoidAgent.cs`
- `Added` `/Assets/Scripts/BoidAgent.cs.meta`
- `Added` `/Assets/Scripts/BoidHelper.cs`
- `Added` `/Assets/Scripts/BoidHelper.cs.meta`
- `Added` `/Assets/Scripts/BoidManager.cs`
- `Added` `/Assets/Scripts/BoidManager.cs.meta`
- `Added` `/Assets/Scripts/ObstacleController.cs`
- `Added` `/Assets/Scripts/ObstacleController.cs.meta`
- `Added` `/Assets/Scripts.meta`
- `Added` `/Assets/Settings`
- `Added` `/Assets/Settings/URP-HighFidelity-Renderer.asset`
- `Added` `/Assets/Settings/URP-HighFidelity-Renderer.asset.meta`
- `Added` `/Assets/Settings/URP-HighFidelity.asset`
- `Added` `/Assets/Settings/URP-HighFidelity.asset.meta`
- `Added` `/Assets/UniversalRenderPipelineGlobalSettings.asset`
- `Added` `/Assets/UniversalRenderPipelineGlobalSettings.asset.meta`
- `Added` `/ProjectSettings/EditorSettings.asset`
- `Added` `/ProjectSettings/ProjectSettings.asset`
- `Added` `/ProjectSettings/TagManager.asset`

</details>

## CS2 | 2026-08-04T14:59:10+08:00 | /main

- 优化Boid算法

Changed items: 6

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Material/Mat_Obstacle.mat`
- `Changed` `/Assets/Prefabs/BoidAgent.prefab`
- `Changed` `/Assets/Scenes/SampleScene.unity`
- `Changed` `/Assets/Scripts/BoidAgent.cs`
- `Changed` `/Assets/Scripts/BoidManager.cs`
- `Changed` `/ProjectSettings/ProjectSettings.asset`

</details>

## CS3 | 2026-08-04T20:03:12+08:00 | /main

-一些普通的环境搭建
-加了一些简单的后处理效果
-实现水下的fog效果
-用shader做了简单的洋流扰动

Changed items: 3

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Scenes/SampleScene.unity`
- `Changed` `/Assets/Settings/URP-HighFidelity-Renderer.asset`
- `Changed` `/Assets/Settings/URP-HighFidelity.asset`

</details>

## CS4 | 2026-08-05T11:24:15+08:00 | /main

-实现焦散效果

Changed items: 2

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Scenes/SampleScene.unity`
- `Changed` `/Assets/Settings/URP-HighFidelity-Renderer.asset`

</details>

## CS6 | 2026-10-02T15:26:33+08:00 | /main

chore(boids): archive the flocking release and complete tracked scene dependencies

- Preserve the CPU Boids implementation with one-pass separation, alignment and cohesion accumulation using squared-distance filtering
- Retain bounded steering and speed integration, predictive BoxCollider bounds, LayerMask-filtered SphereCast avoidance and cached Fibonacci-sphere directions
- Retain staggered wander, optional target steering and horizontal vortex/radial shape controls
- Add the previously Private SampleSceneProfile asset and its meta file referenced by the scene Global Volume
- Replace legacy teaching comments and commented-out experiments with concise English comments without trailing periods
- Verify that comment cleanup preserves all executable tokens in BoidAgent, BoidManager and BoidHelper
- Prepare a separate Boids-only GitHub publication with a redistributable generated demo, bilingual documentation and sanitized Plastic SCM history
- Keep FFT ocean changes and unrelated asset additions outside this scoped check-in


Changed items: 5

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Scripts/BoidHelper.cs`
- `Changed` `/Assets/Scripts/BoidAgent.cs`
- `Changed` `/Assets/Scripts/BoidManager.cs`
- `Added` `/Assets/Settings/SampleSceneProfile.asset`
- `Added` `/Assets/Settings/SampleSceneProfile.asset.meta`

</details>

## CS7 | 2026-10-02T15:38:38+08:00 | /main

chore(boids): preserve required private scene dependencies before GitHub publication

- Audit SampleScene with Unity AssetDatabase.GetDependencies and compare every required asset/meta path with Plastic SCM Private status
- Track the 96 missing scene dependency items, including fish, coral, rock geometry, materials, animation data and corresponding meta files
- Preserve Scenes and Settings folder metadata so their Unity GUIDs remain stable
- Keep these imported third-party source assets in the existing SCM repository only; exclude them from the public GitHub release
- Verify the isolated source scene builds and records 400 agents without captured runtime errors
- Provide a separate public demo with generated geometry and the unchanged Boids runtime algorithm
- Archive actual SCM changeset IDs, dates, original comments and relative paths without account or cloud identifiers
- Retain local FFT changes and unrelated imported assets outside this Boids dependency check-in


Changed items: 104

<details><summary>Changed paths / 文件清单</summary>

- `Added` `/Assets/Fish`
- `Added` `/Assets/Scenes.meta`
- `Added` `/Assets/Settings.meta`
- `Added` `/Assets/Fish/3D Props`
- `Added` `/Assets/Fish/3D Props/3D Props Fish`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Ani`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Ani/fish.controller`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Ani/fish.controller.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Ani/fish@idle.FBX`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Ani/fish@idle.FBX.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/BlueTang.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/BlueTang.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralAB.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralAB.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralE.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralE.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralF.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralF.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralG.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralG.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralH.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralH.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralNB.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralNB.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralNC.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralNC.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralND.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/CoralND.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockA.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockA.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockB.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockB.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockC.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockC.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockE.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockE.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockG.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockG.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockK.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/RockK.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedA.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedA.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedB.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedB.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedC.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedC.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedD.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/SeaweedD.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/ShellB.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/ShellB.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/ShellD.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/ShellD.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/StarfishB.fbx`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/FBX/StarfishB.fbx.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/BlueTang.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/BlueTang.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralAB.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralAB.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralE.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralE.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralF.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralF.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralG.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralG.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralH.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralH.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralNB.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralNB.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralNC.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralNC.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralND.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/CoralND.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockA.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockA.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockB.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockB.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockC.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockC.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockE.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockE.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockG.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockG.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockK.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/RockK.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedA.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedA.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedB.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedB.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedC.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedC.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedD.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/SeaweedD.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/ShellB.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/ShellB.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/ShellD.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/ShellD.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/StarfishB.prefab`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Prefabs/StarfishB.prefab.meta`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Texture`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Texture/Color.png`
- `Added` `/Assets/Fish/3D Props/3D Props Fish/3D Props Fish/Texture/Color.png.meta`

</details>
