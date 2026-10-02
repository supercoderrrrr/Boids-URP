# Capture Notes / 录制说明

`original-underwater` records the existing SampleScene with 400 agents and its licensed presentation assets. The isolated capture copy removes the unrelated FFT ocean Renderer Feature; no flocking-rule parameters are changed. The saved original camera view is used with a small lateral camera sway. The original local project is not modified by the capture.

`public-demo` records the included generated scene with 240 agents, the same runtime flocking scripts and a generated fish mesh. Its activity box and presentation differ from the original scene, as documented in the main README.

Both clips run 180 warm-up steps and then 300 simulated steps at 30 Hz. Every second simulation step is captured, producing 150 frames at 1280 × 720 and a 10-second MP4 encoded at 15 fps. GIFs are shorter, reduced-resolution previews. No runtime performance benchmark is inferred from these controlled recordings.

The `*-capture.json` files store the frame count, simulation/output rates, agent count and whether errors were captured. Screenshots are direct Unity camera output from frame 75, without generated replacement artwork or color correction outside Unity.

To repeat a capture, make a Development Build and launch the executable with `-boidsCapture <output-directory>`. The included `BoidsPortfolioCapture` script is activated only by that argument. It temporarily disables manual camera/obstacle input in the running capture and does not save scene changes.

原场景录像使用已有场景和 400 个个体，在隔离副本中移除无关 FFT Renderer Feature，保留原集群参数和相机视角；公开演示录像使用仓库内的生成场景和 240 个个体。两者均先预热，再按 30 Hz 模拟、每两步采集一帧，MP4 为 15 fps，GIF 为缩小预览。截图直接来自 Unity 第 75 帧，不使用生成画面替代真实运行结果，录制帧率不代表游戏性能。
