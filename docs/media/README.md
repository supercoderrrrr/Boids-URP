# Boids Capture Notes / Boids 录制说明

## English

All screenshots and showcase recordings in this folder come from the latest underwater composition, now named `Boids`. Previous-scene media was removed from the current repository version.

- `boids-opening`: the first six seconds, including the populated vortex from startup
- `boids-schooling`: an eight-second recording from the arch-front view
- `boids-vortex`: a six-second close-up of the slower, enlarged vortex school
- `boids-*.png`: direct Unity camera captures of the opening, arch, vortex, overview and surface
- `showcase-editor.json` and `showcase-runtime.json`: validation from the licensed showcase copy

Showcase source frames were captured on 2026-10-02 at 1600 × 900. Simulation runs at 30 Hz, and every second step is recorded for MP4 playback at 15 fps. The GIF is a reduced 960-pixel-wide preview. Playback rates are controlled and do not establish runtime FPS. No external image synthesis or color grading was applied to these camera captures.

The showcase contains licensed art, excluded from the public project. The public `Boids` scene shares its 1,775-agent runtime and routes but uses generated replacement assets. Its separately captured evidence is recorded in `public-boids-runtime.json` and `public-boids.png`.

To capture the distributed scene, make a Windows Development Build and launch it with `-reefCapture <output-directory>`. `ReefCapture` temporarily disables manual camera input, records the opening and close-up views, measures simulation work, and exits. It does not save scene changes.

## 简体中文

本目录的展示录像与截图全部来自新版海底场景，场景现已命名为 `Boids`，当前仓库已移除旧场景素材。开场录像为 6 秒，拱门前巡游录像为 8 秒，漩涡近景为 6 秒。

展示素材于 2026-10-02 直接从 Unity 相机采集，分辨率为 1600 × 900，以 30 Hz 模拟、每两步记录一帧，MP4 播放为 15 fps，GIF 为缩小预览。没有额外生成画面或在 Unity 之外重新调色，录制帧率不等同于实测运行 FPS。

展示版包含未公开的许可资源。公开 `Boids` 使用生成替代资源，并复用相同的 1775 条鱼与路线，单独运行的验证保存在 `public-boids-runtime.json` 与 `public-boids.png`。开发构建可用 `-reefCapture <输出目录>` 重复录制。
