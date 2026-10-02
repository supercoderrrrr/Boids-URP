# Implementation Notes / 实现说明

## English

`BoidManager` owns configuration and the spawned-agent list. `Start` instantiates the prefab around the manager position, initializes each velocity and stores each agent. `spawnArea` is a half-extent: sampling ranges from `-spawnArea` to `+spawnArea` along each axis.

`BoidAgent.Update` reads neighboring positions and velocities, computes behavioral steering, integrates velocity and moves the transform. State is updated in place, so later agents in the frame can observe earlier agents' updated state.

For an offset `r = myPosition - neighborPosition`, the separation accumulator adds `r / |r|²`. Its magnitude is proportional to `1 / |r|`; closer agents contribute more. The accumulator is then converted into a desired heading. Alignment averages normalized neighbor velocities. Cohesion uses the average neighbor position minus the agent position.

Each desired direction is converted to steering by:

```text
desiredVelocity = normalize(direction) * maxSpeed
steering = clampMagnitude(desiredVelocity - velocity, maxSteerForce)
acceleration = sum(weighted behavior steering)
velocity += acceleration * deltaTime
velocity = normalize(velocity) * clamp(speed, minSpeed, maxSpeed)
position += velocity * deltaTime
```

`maxSteerForce` limits an individual behavior, not the final weighted sum. Changing weights also changes effective turning acceleration. This does not explicitly constrain angular turn rate.

`CalculateFlockingForces` shares one all-agent loop and precomputes squared radii. This reduces repeated work but leaves quadratic neighbor complexity. Three old, unused single-rule methods remain in the learning implementation; `Update` calls only the combined method.

Bounds steering looks ahead along the current movement direction, converts the predicted point with `InverseTransformPoint`, subtracts the collider center and compares against its local half-size. The current implementation normalizes the accumulated wall direction before steering, so it does not preserve a continuous wall-force falloff. Use an identity-scale box for the published preset: wall margins are compared in local units while look-ahead uses world units. It provides soft confinement and can overshoot.

Avoidance first casts along the current heading. On a hit, it casts along the helper's candidate directions and accepts the first unobstructed candidate. Directions are generated once on a Fibonacci sphere, with index zero aligned along local +Z. A collider and the correct obstacle layer are required. There is no overlap resolution for an agent starting inside an obstacle, and the casts use Unity's default trigger interaction setting.

Wander picks a normalized random direction at staggered times, scaling its vertical component before normalization. Vortex shaping blends `cross(up, radialDirection)` with the normalized radial correction toward a target radius. It ignores vertical distance and has a sharp radial sign change around that radius. Target steering is an optional attraction to a transform.

`UnderwaterFogController` contains an optional local-box fog switch. It is retained for study; the public demonstration instead saves a permanent scene fog and a global Volume. `CameraController` uses legacy Unity input and cursor lock. `ObstacleController` is an older manual-testing utility and is not attached in the public scene because it shares movement keys with the camera.

The public scene builder changes presentation and configuration, not the runtime flocking algorithm. `BoidsPortfolioCapture` is dormant unless the `-boidsCapture` argument is present, and is compiled only for the Editor or development builds.

## 简体中文

`BoidManager` 保存参数与个体列表，在 `Start` 中生成并初始化个体。`spawnArea` 表示半尺寸，每轴生成范围为其负值到正值。

`BoidAgent.Update` 计算局部规则、叠加转向、积分速度并更新 Transform。个体直接读取彼此状态，因此同一帧中后更新的个体可能读到前面个体的新状态，尚未使用统一快照。

分离使用 `r / |r|²`，贡献大小随距离倒数变化；对齐平均邻居单位朝向；聚合朝局部平均位置。方向经过“期望速度减当前速度”转为转向力，限制单项最大值后按权重叠加，最后限制速度大小并移动。

合并遍历减少重复计算，但复杂度仍为 O(N²)。原脚本保留了三个未调用的旧单规则方法，实际更新只使用合并方法。

边界计算在局部空间检查预测位置。最终归一化边界方向会丢失连续的距离强度，因此当前并非平滑边界力衰减。墙面距离按局部单位比较，前瞻距离使用世界单位，公开配置的盒子缩放保持为 1。转向约束不能保证绝不越界。

避障先向前 SphereCast，遇到障碍后选取第一个通畅的 Fibonacci 球面候选方向。障碍需要 Collider 和匹配的 Layer。当前没有“已在障碍内部”的重叠恢复机制，Trigger 行为沿用 Unity 全局设置。

Wander 交错随机换向，旋涡混合水平切向与径向修正，不处理竖直形状，目标吸引为可选行为。当前旋涡在目标半径两侧会发生径向符号切换。

公开场景保存常驻雾与 Global Volume，保留的局部雾脚本用于学习；相机使用旧 Input 接口。旧的障碍手动控制脚本与相机使用相同按键，所以没有挂到公开演示中。场景构建工具只调整展示和参数，录制工具只在 Editor 或开发构建中通过命令行显式启用。
