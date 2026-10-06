# Schooling Implementation / 集群实现

## English

`BoidsSchool` owns the positions, previous positions, velocities, next velocities, route progress and rendering matrices for one school. There is no shared neighbor list between schools. Four instances provide the arch passage, distant ribbon, foreground residents and vortex.

### Neighbor Queries and Integration

`BoidsSpatialGrid` maps `floor(position / cellSize)` to the head of an index chain. A reusable `next` array stores the remaining indices in each cell. Cell size is the larger of perception and separation radii, so a 3 × 3 × 3 query covers the required neighborhood. Negative coordinates use floor rather than truncation.

Each simulation step rebuilds the grid from the current positions. All agents read these positions and velocities, accumulate steering into `nextVelocities`, and only then integrate their state. Separation adds an inverse-distance contribution, alignment compares the mean neighboring velocity with the current velocity, and cohesion pulls toward the local average position. Final acceleration and speed are bounded.

Candidate count depends on occupancy. Sparse distributions avoid most all-pairs checks; placing every agent in one cell can still produce quadratic work. This grid uses hash lookup and linked indices, not a sorted GPU grid.

### Shared Travel Intent

Closed Catmull-Rom waypoints are sampled into a polyline with cumulative arc lengths. A binary search finds the segment at a requested travel distance. Each fish has its own corridor offset and speed factor. A look-ahead point supplies desired velocity, while local rules and obstacle avoidance remain active.

The distant ribbon periodically projects each agent onto the nearest segment to correct route-progress drift. These checks are staggered over 15 simulation ticks. Only progress changes; the agent position is not snapped to the route. Progress-guidance error includes along-route phase mismatch and differs from nearest-centerline distance.

The vortex instead combines tangential velocity with radial feedback around a height-dependent radius. Its vertical target follows a slow sine cycle with per-agent phases. Stratified spawn heights and golden-angle azimuths fill the full column before the first update, with circulating velocity initialized immediately. This is an artistic velocity field rather than a fluid simulation.

### Obstacle Probes and Rendering

Each agent refreshes its cached avoidance response once every five ticks. A forward `SphereCast` checks the obstacle layer; on a hit, up to 12 candidate casts search a forward cone. The response is combined with the other forces. These probes do not resolve body overlaps or guarantee containment.

Simulation uses a 1/30-second step, with bounded catch-up work. Rendering interpolates between previous and current positions and smooths orientation. Each school submits a single `Graphics.DrawMeshInstanced` call, capped at 1,023 agents; the four authored schools need four fish draws per camera. Vertex deformation animates the tails without per-fish GameObjects or Animators. These draw counts do not include environment objects or particles.

The original `BoidAgent`, `BoidManager` and `BoidHelper` remain as earlier learning-stage code. The `Boids` scene uses `BoidsSchool`. Legacy regression checks cover the zero-direction steering guard and signed initial camera pitch.

The public builder substitutes generated geometry and textures when licensed packages are absent. Showcase assets and public assets therefore differ, while the schooling runtime and authored routes are shared. Capture code activates only in the Editor or a Development Build with `-boidsCapture`.

## 简体中文

`BoidsSchool` 独立管理一组鱼的位置、上一帧位置、速度、下一步速度、路线进度和绘制矩阵。四组分别负责拱门通道、远景巡游、前景活动与漩涡，各组不会共享邻居或转向力。

空间网格把 `floor(position / cellSize)` 映射到单元格链表头，复用 `next` 数组记录同格个体。单元格尺寸取感知与分离半径的较大值，因此检查当前格与周围 26 格即可覆盖邻居范围。负坐标使用向下取整。

每一步先重建网格，所有鱼读取同一批位置和速度计算下一步速度，再统一积分。分离、对齐、聚合与引导和避障叠加，最终限制加速度和速度。候选数量取决于分布密度；所有鱼挤在同格时仍可能接近平方复杂度。这是 CPU 哈希网格与索引链表，没有使用 GPU 排序。

闭合 Catmull-Rom 路线按弧长采样，通过二分查询确定前视点，并为每条鱼保留横向偏移和速度差异。远景鱼群错开执行最近线段投影以修正进度漂移，只改进度，不把鱼的位置直接吸附到路线。沿路线进度误差与最近中心线距离是两种不同指标。

漩涡使用切向速度、径向反馈和缓慢的正弦纵向目标。分层高度与黄金角分布让开场鱼群覆盖完整区域，并提前赋予环绕速度。

每条鱼每五个模拟步更新一次缓存避障响应，前向 SphereCast 命中后最多检查 12 个前方候选方向。它属于预测转向，不能保证鱼体绝不交叠或穿过障碍。

模拟以 30 Hz 更新并限制追帧次数，绘制对位置插值、对朝向平滑。每组通过一次实例化绘制提交最多 1023 条鱼，当前四组每个相机需要四次鱼群绘制；环境与粒子另计。摆尾由顶点着色器完成，没有逐鱼 GameObject 或 Animator。

原来的 Agent/Manager 脚本保留为学习阶段代码，正式 `Boids` 场景使用 `BoidsSchool`。公开构建工具会生成替代鱼与环境资源，展示与公开版的资源不同，但使用相同集群逻辑和路线。录制工具通过 `-boidsCapture` 显式启用。
