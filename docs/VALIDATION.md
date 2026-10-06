# Boids Validation / Boids 验证

## English

The public update was validated on **2026-10-03** with Unity 2022.3.62f2, URP 14.0.12, Windows x64 and Direct3D 11. Showcase recordings retain their 2026-10-02 capture results. The two configurations share schooling code and routes but use different art assets.

| Check | Result |
| --- | --- |
| Public C# compilation and Development Build | Passed |
| Distributed scenes | Only `Assets/BoidsUnderwaterScene/Scenes/Boids.unity` |
| Scene scripts, materials, school meshes and camera | Passed |
| Excluded package and FFT dependencies | No excluded scene dependencies |
| Spatial-grid equivalence | Exact match against brute-force radius queries for 257 test points, including negative coordinates and cell boundaries |
| Unified-school isolation | All three traveling schools matched their isolated positions for 60 steps with a concurrently simulated vortex |
| Vortex startup | 960 agents present across all 96 sampled angle/height sectors, already circulating |
| Route collision sampling | 1,347 samples, zero blocked samples |
| Coral contact checks | 48 foreground and 105 background colonies; zero unsupported or high placements detected |
| Legacy regressions | Zero-direction steering and signed initial camera pitch checks passed |
| Public runtime | 1,775 agents, 900 sampled simulation steps, no recorded Error or Exception messages |
| Media | Updated camera stills, GIF preview and H.264 recordings; public and showcase output visually reviewed |

Reports: [editor validation](boids-editor-validation.json), [public runtime](media/public-boids-runtime.json), [showcase editor](media/showcase-editor.json), [showcase runtime](media/showcase-runtime.json), [publication checks](publication-validation.json) and [text audit](release-audit.json).

### Measurements

The licensed showcase recorded 87,711 candidate comparisons per sampled simulation step. Independent all-pairs loops within those same four schools would perform 1,195,250 directed comparisons. This is approximately a **93% reduction in candidates**, not a 93% reduction in frame time.

On an NVIDIA GeForce RTX 4060 Laptop GPU, showcase school simulation averaged 8.42 ms per sampled step. Its 1600 × 900 synchronous render measurement had a 9.28 ms median and 29.84 ms 95th percentile. That measurement explicitly calls `Camera.Render` and reads one pixel to synchronize GPU completion; PNG encoding is excluded. It is not a normal game-frame FPS benchmark. The public replacement-assets run has its own values in the linked runtime report, and is not an identical-view performance comparison.

### Limits

The grid can approach quadratic work when agents share a dense neighborhood. Route sampling and a one-time 0.12-unit overlap sample do not establish collision-free fish bodies; separation and predictive sphere casts remain steering heuristics. Cross-school body collisions are not simulated.

The water uses approximate transmission, absorption and scattering. It does not reflect the complete reef or refract an above-water scene. Full input interaction, allocation profiling, scaling across agent counts, alternate graphics APIs and other platforms were not tested. The original fullscreen Shader Graph experiment is retained separately and is not the renderer used by the new scene.

Historical Plastic SCM and comment-cleanup reports are preserved as earlier evidence. The later steering and camera fixes intentionally change behavior, so the old comment-only token equivalence is not a claim that today's scripts are unchanged from that snapshot.

## 简体中文

公开版本于 **2026-10-03** 使用 Unity 2022.3.62f2、URP 14.0.12、Windows x64 和 Direct3D 11 验证。首页展示素材保留 2026-10-02 的录制结果，两版使用相同集群代码与路线，但美术资源不同。

编译和开发构建通过，仓库仅有正式 `Boids` 场景。257 点网格查询与全遍历结果一致，三组流动鱼与同时运行漩涡的隔离对照连续 60 步一致。漩涡开场已存在全部 960 条鱼，96 个高度与角度分区均有鱼。1347 个路线采样点未检出障碍相交，珊瑚底部接触检查通过。

公开运行采集了 1775 条鱼与 900 个模拟步，没有记录 Error 或 Exception。截图、GIF 和录像均来自实际 Unity 相机输出，完整结果见上方 JSON 报告。

展示版平均每步约 87711 次邻居候选比较，同样四组的全员遍历参考为 1195250 次，候选减少约 93%，不能等同于整帧性能提高 93%。展示版群集计算平均约 8.42 ms / 模拟步；显式渲染并同步 GPU 的中位耗时约 9.28 ms，不是正常游戏帧率。公开版替代资源的实测值另存于对应报告，不能直接作为同场景优化对比。

分离与预测避障没有刚体碰撞约束，密集鱼体和不同鱼群仍可能交叠；水下光学属于实时近似。键鼠完整交互、分配开销、不同鱼数下的扩展性与其他平台没有完整测试。保留的早期 SCM 和纯注释核验只描述当时版本，后续修复确实改变了脚本行为。
