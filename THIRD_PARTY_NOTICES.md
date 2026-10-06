# References and Third-Party Notices

## Boids Reference

The learning implementation follows the steering and collision-direction concepts demonstrated in [Sebastian Lague's Boids project](https://github.com/SebLague/Boids). The Fibonacci-sphere helper closely follows the reference helper's construction. The reference is licensed under MIT; the notice is retained below for the adapted portions.

```text
MIT License

Copyright (c) 2019 Sebastian Lague

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

Further algorithm references: [Craig Reynolds](https://www.red3d.com/cwr/boids/) and the [sphere sampling discussion](https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere/44164075#44164075).

## Assets in the Showcase

| Local Resource | Credit | Public Distribution |
| --- | --- | --- |
| 3D Props Fish | Layer Lab | Rendered media only; fish source models, textures and animations omitted |
| Underwater Ship environment | Leartes Studios | Rendered media only; environment source assets omitted |
| Water Caustics Effect for URP v2 | Masataka Hakozaki / HacoApp | Rendered media only; plugin source, shaders and textures omitted |
| Bubble VFX Collection | Imported third-party package; creator not verified from the available package text | Source package omitted |
| Skyboxes package | Staggart Creations | Embedded package and its source art omitted |
| Imported water/noise/caustic textures | Source provenance not verified in the local files | Omitted |

The public `Boids` scene uses `BoidsSceneAssets` to generate fish, outcrops, branching corals and textures instead of redistributing those resources. The authored arch and stratified cliff meshes and their Blender source are included. A mesh baked from the licensed fish model is still a derivative of that model and is excluded along with its textures.

The latest scene uses the included URP shaders for surface animation, underwater absorption, scattering, shadowed light shafts and procedural caustics. The earlier scene integrated a third-party caustics plugin, whose source remains excluded.

Unity Asset Store content is subject to its applicable asset license. Public source publication and rendered portfolio media are different uses; see [Unity's Asset Store EULA](https://unity.com/legal/as-terms). Obtaining a package does not by itself establish permission to redistribute its raw files.

Unity packages resolve through Package Manager and retain their own licenses. This notice does not grant a new blanket license to all repository content.

## 简体中文

Boids 转向与避障方向设计参考了 Sebastian Lague 的教程与项目，球面采样辅助类沿用了其实现思路，因此保留上方 MIT 许可。局部集群规则的原始参考为 Craig Reynolds。

展示场景的鱼与部分环境资源属于第三方内容，源文件不随公开仓库上传，从鱼模型烘焙的网格也不分发。公开 `Boids` 用 `BoidsSceneAssets` 生成替代鱼、岩石、珊瑚与贴图，同时包含制作的拱门、分层岩壁和 Blender 源文件。

新版水面动画、吸收、散射、带阴影光束与程序焦散使用仓库内的 URP 着色器。早期场景使用的第三方焦散插件仍未公开。

水、噪声与焦散贴图的本地来源无法确认，因此也未公开。Unity 包保留各自许可。本说明不向所有仓库内容额外授予统一的新许可。
