using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace BoidsUnderwaterScene.Editor
{
    public static class BoidsUnderwaterSceneBuilder
    {
        public const string Root = "Assets/BoidsUnderwaterScene";
        private const string Generated = Root + "/Generated";
        private const string Art = "Assets/LeartesStudios/UnderwaterShip/Art";
        private const string Fish = "Assets/Fish/3D Props/3D Props Fish/3D Props Fish";
        private static readonly Dictionary<Material, Material> converted = new Dictionary<Material, Material>();
        public const string ScenePath = Root + "/Scenes/Boids.unity";
        private static Collider seabedCollider;

        [MenuItem("Tools/Boids/Open Boids")]
        public static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Tools/Boids/Rebuild Boids")]
        public static void BuildScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Generated); Directory.CreateDirectory(Root + "/Scenes"); AssetDatabase.Refresh();
            MigrateAssetNames();
            converted.Clear();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = null; RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.29f,.43f,.46f);
            RenderSettings.ambientEquatorColor = new Color(.095f,.17f,.19f);
            RenderSettings.ambientGroundColor = new Color(.035f,.055f,.058f);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.color = new Color(.80f,.94f,.87f); sun.intensity = 2.3f;
            sun.shadows = LightShadows.Soft; sun.shadowBias = .045f; sun.shadowNormalBias = .18f;
            sun.transform.rotation = Quaternion.Euler(54,-34,0); RenderSettings.sun = sun;

            var pipeline = MakePipeline();
            var environment = new GameObject("Underwater Environment").AddComponent<UnderwaterEnvironment>();
            Set(environment,"pipeline",pipeline); Set(environment,"sun",sun);
            QualitySettings.renderPipeline = pipeline;
            Shader.SetGlobalFloat("_UnderwaterSurfaceHeight",28f);
            Material rock = Living("Eroded Limestone", "T_Rock_B.PNG", "T_Rock_N.png", new Color(.60f,.65f,.62f), true);
            rock.SetFloat("_Algae",.38f); rock.SetFloat("_Tile",.18f); rock.SetFloat("_Smoothness",.045f); rock.SetFloat("_NormalStrength",.48f);
            Material sand = Living("Rippled Sand", "T_Sand_B.PNG", "T_Sand_N.png", new Color(.87f,.91f,.82f), true);
            sand.SetFloat("_Algae",0); sand.SetFloat("_Tile",.36f); sand.SetFloat("_NormalStrength",.3f); sand.SetFloat("_Desaturation",.88f); sand.SetFloat("_SandRipples",1);
            var terrain = MeshObject("Sand channel", Terrain(), sand, Vector3.zero, Vector3.one);
            terrain.layer = 6; terrain.AddComponent<MeshCollider>().sharedMesh = terrain.GetComponent<MeshFilter>().sharedMesh;
            seabedCollider = terrain.GetComponent<Collider>();

            var architecture = new GameObject("Underwater Rocks").transform;
            Model("RockArch", new Vector3(3,-.9f,29), new Vector3(27,15,7), 8, rock, architecture);
            Model("StratifiedCliff1",new Vector3(-18,-3,5),new Vector3(16,29,16),-15,rock,architecture);
            Model("StratifiedCliff2",new Vector3(22,-3,15),new Vector3(13,32,21),26,rock,architecture);
            Model("StratifiedCliff3",new Vector3(-28,-4,35),new Vector3(20,30,24),44,rock,architecture);
            Model("StratifiedCliff1",new Vector3(31,-4,55),new Vector3(17,31,24),-26,rock,architecture);
            Model("StratifiedCliff2",new Vector3(-29,-3,75),new Vector3(21,25,24),68,rock,architecture);
            Model("StratifiedCliff3",new Vector3(39,-4,91),new Vector3(28,31,30),-12,rock,architecture);
            var reefs = new GameObject("Rock outcrops and living corals").transform;
            UnityEngine.Random.InitState(621);
            for (int i=0;i<68;i++)
            {
                float z = UnityEngine.Random.Range(-23f,110f);
                float side = i%2 == 0 ? -1 : 1;
                float x = side * UnityEngine.Random.Range(12f,33f) + Mathf.Sin(z*.05f)*3;
                float size = UnityEngine.Random.Range(1.3f,5.4f);
                var obj = ArtObject("SM_Rock0"+(i%5+1));
                obj.name = "Rock outcrop " + i; obj.transform.SetParent(reefs);
                Fit(obj,new Vector3(size*1.5f,size*.75f,size),new Vector3(x,Height(x,z)-.2f,z));
                obj.transform.Rotate(0,UnityEngine.Random.Range(0,360),0);
                foreach(var r in obj.GetComponentsInChildren<Renderer>()) r.sharedMaterial=rock;
                foreach(var c in obj.GetComponentsInChildren<Collider>()) c.gameObject.layer=6;
            }
            string[] species={"Finger","Table","Heliopora","Porites","Pocillopora","Tube","Elkhorn"};
            for (int i=0;i<105;i++)
            {
                float z=UnityEngine.Random.Range(-18f,96f);
                float x=(i%2==0?-1:1)*UnityEngine.Random.Range(10f,29f)+Mathf.Sin(z*.05f)*3;
                var obj=ArtObject("SM_Coral_"+species[i%species.Length]);
                obj.name="BackgroundCoral"+i;obj.transform.SetParent(reefs);
                float size=UnityEngine.Random.Range(.65f,2.6f);
                Fit(obj,new Vector3(size,size*.72f,size),new Vector3(x,Height(x,z),z));
                obj.transform.Rotate(0,UnityEngine.Random.Range(0,360),0,Space.World);
                ConvertMaterials(obj);
                foreach(var c in obj.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                GroundCoral(obj,seabedCollider);
            }
            MakeForegroundRocks(reefs,rock);
            MakeKelp();
            Material water = Save(new Material(Shader.Find("BoidsUnderwaterScene/WaterSurface")), "WaterSurface.mat");
            water.SetTexture("_NormalMap",ArtTexture("T_Water_N.png"));
            water.SetFloat("_WaveHeight",.28f);
            var surface=MeshObject("WaterSurface",Grid("WaterGrid",420,96,false),water,new Vector3(0,28,30),Vector3.one);
            surface.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            surface.GetComponent<Renderer>().receiveShadows=false;

            Mesh fishMesh=BakeFish(out Material fishMaterial);
            School("Arch passage school",fishMesh,fishMaterial,300,.72f,3.25f,2.8f,0f,.30f,
                new[]{new Vector3(-5,7,-9),new Vector3(2,7,13),new Vector3(3,7,31),new Vector3(5,9,53),new Vector3(17,13,72),new Vector3(8,20,49),new Vector3(-6,19,29),new Vector3(-8,12,5)});
            var distant = School("DistantFlowingRibbon",fishMesh,fishMaterial,420,.43f,5.2f,2.1f,.32f,.25f,
                new[]{new Vector3(-11,19,46),new Vector3(-3,19,58),new Vector3(12,18,72),new Vector3(19,18,93),new Vector3(5,20,115),new Vector3(-14,21,105),new Vector3(-16,20,80)});
            Set(distant,"lookAhead",6.5f);Set(distant,"maxAcceleration",7.5f);
            var distantSettings=new SerializedObject(distant);distantSettings.FindProperty("reprojectToCorridor").boolValue=true;distantSettings.ApplyModifiedPropertiesWithoutUndo();
            School("Foreground reef residents",fishMesh,fishMaterial,95,.9f,2.3f,2.4f,.15f,.35f,
                new[]{new Vector3(-4,6,-10),new Vector3(5,6,-4),new Vector3(8,6.5f,9),new Vector3(6,7,23),new Vector3(-2,7,25),new Vector3(-6,7,12),new Vector3(-6,6.5f,-1)});
            var vortex = School("VortexSchool",fishMesh,fishMaterial,960,.95f,2.6f,1.8f,0,1,
                new[]{Vector3.zero,Vector3.right,Vector3.forward,Vector3.left});
            vortex.enabled=false;
            ConfigureVortexSchool(vortex);vortex.enabled=true;
            Particles("MarineSnow",new Vector3(0,12,15),new Vector3(54,24,82),false);
            Particles("RightBubbleColumn",new Vector3(7,1,2),Vector3.one,true);
            Particles("LeftBubbleColumn",new Vector3(-9,1,19),Vector3.one,true);
            Particles("DistantBubbleColumn",new Vector3(12,1,52),Vector3.one,true);
            Particles("AmbientBubbles",new Vector3(0,9,24),new Vector3(46,15,85),true,true);
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag="MainCamera";camera.transform.position=new Vector3(0,5,-25);
            camera.transform.LookAt(new Vector3(2,9,33));camera.fieldOfView=61;camera.nearClipPlane=.15f;camera.farClipPlane=240;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.027f,.17f,.20f);camera.allowHDR=true;
            camera.gameObject.AddComponent<AudioListener>();camera.gameObject.AddComponent<UnderwaterCameraController>();
            var extra=camera.GetUniversalAdditionalCameraData();extra.renderPostProcessing=true;
            extra.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;extra.antialiasingQuality=AntialiasingQuality.High;
            MakeVolume();
            foreach(var root in scene.GetRootGameObjects())
                foreach(var child in root.GetComponentsInChildren<Transform>(true))child.name=PascalName(child.name);
            ConfigureSchoolPresentation();
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
            Debug.Log("BOIDS_SCENE_CREATED agents=1775 Unity="+Application.unityVersion);
        }

        private static string PascalName(string name)
        {
            var words=Regex.Split(name,"[^a-zA-Z0-9]+");
            string result="";
            foreach(string word in words)if(word.Length>0)result+=char.ToUpperInvariant(word[0])+word.Substring(1);
            return result;
        }

        private static void MigrateAssetNames()
        {
            string oldScene=Root+"/Scenes/BoidsTest.unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(oldScene)!=null && AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
            {
                string error=AssetDatabase.MoveAsset(oldScene,ScenePath);
                if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
            }
            foreach(string path in Directory.GetFiles(Generated))
            {
                if(path.EndsWith(".meta"))continue;
                string clean=PascalName(Path.GetFileNameWithoutExtension(path));
                if(clean==Path.GetFileNameWithoutExtension(path))continue;
                string error=AssetDatabase.RenameAsset(path.Replace('\\','/'),clean);
                if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
            }
        }

        private static UniversalRenderPipelineAsset MakePipeline()
        {
            var sourceData=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/URP-HighFidelity-Renderer.asset");
            var renderer=Object.Instantiate(sourceData);
            renderer.rendererFeatures.Clear();
            renderer=Save(renderer,"UnderwaterRenderer.asset");
            foreach(var feature in renderer.rendererFeatures) if(feature!=null) Object.DestroyImmediate(feature,true);
            renderer.rendererFeatures.Clear();
            var underwater=ScriptableObject.CreateInstance<UnderwaterRendererFeature>();underwater.name="ShadowedUnderwater";
            underwater.Configure(Shader.Find("BoidsUnderwaterScene/Underwater"));
            renderer.rendererFeatures.Add(underwater);AssetDatabase.AddObjectToAsset(underwater,renderer);
            foreach(var sourceFeature in sourceData.rendererFeatures)
            {
                if(sourceFeature==null||sourceFeature.GetType().Name!="ScreenSpaceAmbientOcclusion")continue;
                var ao=Object.Instantiate(sourceFeature);ao.name="ContactOcclusion";ao.SetActive(true);
                var settings=new SerializedObject(ao);var fields=settings.FindProperty("m_Settings");
                fields.FindPropertyRelative("Source").enumValueIndex=0;
                fields.FindPropertyRelative("Downsample").boolValue=true;
                fields.FindPropertyRelative("AfterOpaque").boolValue=true;
                fields.FindPropertyRelative("Intensity").floatValue=.85f;
                fields.FindPropertyRelative("Radius").floatValue=.65f;
                fields.FindPropertyRelative("Samples").enumValueIndex=1;
                settings.ApplyModifiedPropertiesWithoutUndo();renderer.rendererFeatures.Add(ao);AssetDatabase.AddObjectToAsset(ao,renderer);
            }
            var map=new SerializedObject(renderer);var mapArray=map.FindProperty("m_RendererFeatureMap");
            var sourceRenderer=new SerializedObject(AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/URP-HighFidelity-Renderer.asset"));
            map.FindProperty("postProcessData").objectReferenceValue=sourceRenderer.FindProperty("postProcessData").objectReferenceValue;
            mapArray.arraySize=renderer.rendererFeatures.Count;
            for(int i=0;i<renderer.rendererFeatures.Count;i++){AssetDatabase.TryGetGUIDAndLocalFileIdentifier(renderer.rendererFeatures[i],out string _,out long id);mapArray.GetArrayElementAtIndex(i).longValue=id;}
            map.ApplyModifiedPropertiesWithoutUndo();
            var original=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/URP-HighFidelity.asset");
            var pipeline=Save(Object.Instantiate(original),"UnderwaterPipeline.asset");
            var so=new SerializedObject(pipeline);var list=so.FindProperty("m_RendererDataList");list.arraySize=1;list.GetArrayElementAtIndex(0).objectReferenceValue=renderer;
            so.FindProperty("m_DefaultRendererIndex").intValue=0;
            so.FindProperty("m_MSAA").intValue=1;
            so.FindProperty("m_AdditionalLightShadowsSupported").boolValue=false;
            so.FindProperty("m_MainLightShadowmapResolution").intValue=2048;
            so.FindProperty("m_ShadowDistance").floatValue=100;
            so.FindProperty("m_ShadowCascadeCount").intValue=4;
            so.FindProperty("m_RequireDepthTexture").boolValue=true;
            so.FindProperty("m_RequireOpaqueTexture").boolValue=false;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(renderer);return pipeline;
        }

        private static T Save<T>(T value,string file) where T:Object
        {
            file=PascalName(Path.GetFileNameWithoutExtension(file))+Path.GetExtension(file);
            string path=Generated+"/"+file;T existing=AssetDatabase.LoadAssetAtPath<T>(path);
            value.name=Path.GetFileNameWithoutExtension(file);
            if(existing!=null)
            {
                if(existing is UniversalRendererData||existing is VolumeProfile)
                    foreach(var nested in AssetDatabase.LoadAllAssetsAtPath(path))if(nested!=existing&&(nested is ScriptableRendererFeature||nested is VolumeComponent))Object.DestroyImmediate(nested,true);
                EditorUtility.CopySerialized(value,existing);Object.DestroyImmediate(value);EditorUtility.SetDirty(existing);return existing;
            }
            AssetDatabase.CreateAsset(value,path);return value;
        }
        private static Material Living(string name,string color,string normal,Color tint,bool triplanar)
        {
            var mat=Save(new Material(Shader.Find("BoidsUnderwaterScene/EnvironmentLit")),name+".mat");
            mat.enableInstancing=true;mat.SetColor("_BaseColor",tint);mat.SetFloat("_Triplanar",triplanar?1:0);
            mat.SetTexture("_BaseMap",ArtTexture(color));
            mat.SetTexture("_BumpMap",ArtTexture(normal));return mat;
        }
        private static Texture2D ArtTexture(string name)
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+name);
            return texture!=null?texture:BoidsSceneAssets.Texture(name);
        }
        private static GameObject ArtObject(string name)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Prefabs/"+name+".prefab");
            return prefab!=null?(GameObject)PrefabUtility.InstantiatePrefab(prefab):BoidsSceneAssets.Object(name);
        }
        private static void Model(string name,Vector3 position,Vector3 size,float yaw,Material material,Transform parent)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+name+".fbx");
            if(source==null)throw new InvalidOperationException("Missing Blender mesh "+name);
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(source);obj.transform.SetParent(parent);Fit(obj,size,position);obj.transform.rotation=Quaternion.Euler(0,yaw,0)*obj.transform.rotation;
            foreach(var filter in obj.GetComponentsInChildren<MeshFilter>())
            {
                filter.gameObject.layer=6;filter.GetComponent<Renderer>().sharedMaterial=material;
                filter.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
            }
        }
        private static void Fit(GameObject obj,Vector3 size,Vector3 bottom)
        {
            obj.transform.position=Vector3.zero;
            Vector3 originalScale=obj.transform.localScale;
            var renderers=obj.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;
            foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            Vector3 worldScale=new Vector3(size.x/Mathf.Max(bounds.size.x,.001f),size.y/Mathf.Max(bounds.size.y,.001f),size.z/Mathf.Max(bounds.size.z,.001f));
            Vector3 right=obj.transform.right,up=obj.transform.up,forward=obj.transform.forward;
            obj.transform.localScale=Vector3.Scale(originalScale,new Vector3(Vector3.Scale(right,worldScale).magnitude,Vector3.Scale(up,worldScale).magnitude,Vector3.Scale(forward,worldScale).magnitude));
            bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            obj.transform.position=bottom-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        }
        private static void ConvertMaterials(GameObject obj)
        {
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    Material source=materials[i];if(source==null)continue;
                    if(!converted.TryGetValue(source,out var target))
                    {
                        target=Save(new Material(Shader.Find("BoidsUnderwaterScene/EnvironmentLit")),"Coral_"+source.name+".mat");target.enableInstancing=true;
                        target.SetFloat("_Triplanar",0);target.SetFloat("_Algae",0);target.SetColor("_BaseColor",new Color(.86f,.84f,.78f));
                        Texture texture=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):null;
                        if(texture==null&&source.HasProperty("_MainTex"))texture=source.GetTexture("_MainTex");
                        target.SetTexture("_BaseMap",texture);converted[source]=target;
                    }
                    materials[i]=target;
                }
                renderer.sharedMaterials=materials;
            }
        }
        private static float Height(float x,float z)
        {
            return Mathf.Sin(x*.065f+z*.035f)*.48f+Mathf.Sin(z*.105f-x*.034f)*.33f
                +Mathf.PerlinNoise(x*.035f+60,z*.035f+60)*.7f+Mathf.SmoothStep(0,1,Mathf.InverseLerp(8,24,Mathf.Abs(x)))*1.25f;
        }
        private static Mesh Terrain()=>Grid("Seabed",250,150,true);
        private static Mesh Grid(string name,float width,int resolution,bool seabed)
        {
            int side=resolution+1;var vertices=new Vector3[side*side];var uv=new Vector2[vertices.Length];var triangles=new int[resolution*resolution*6];
            for(int z=0;z<side;z++)for(int x=0;x<side;x++)
            {
                float px=(x/(float)resolution-.5f)*width,pz=(z/(float)resolution-.5f)*width+(seabed?40:0);int i=z*side+x;
                float y=seabed?Height(px,pz)+Mathf.Sin(px*2.3f+Mathf.Sin(pz*.15f))*.035f:0;
                vertices[i]=new Vector3(px,y,pz);uv[i]=new Vector2(px,pz);
                if(x==resolution||z==resolution)continue;int k=(z*resolution+x)*6;
                triangles[k]=i;triangles[k+1]=i+side;triangles[k+2]=i+1;triangles[k+3]=i+1;triangles[k+4]=i+side;triangles[k+5]=i+side+1;
            }
            var mesh=new Mesh{name=name,vertices=vertices,uv=uv,triangles=triangles};mesh.RecalculateNormals();mesh.RecalculateBounds();return Save(mesh,name+".asset");
        }
        private static GameObject MeshObject(string name,Mesh mesh,Material material,Vector3 position,Vector3 scale)
        {
            var obj=new GameObject(name);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;
            obj.transform.position=position;obj.transform.localScale=scale;return obj;
        }
        private static Mesh BakeFish(out Material material)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Fish+"/Prefabs/BluefinTuna.prefab");
            if(prefab==null)
            {
                material=Save(new Material(Shader.Find("BoidsUnderwaterScene/EnvironmentLit")),"SchoolFish.mat");material.enableInstancing=true;
                material.SetFloat("_Triplanar",0);material.SetFloat("_Fish",1);material.SetFloat("_Algae",0);material.SetFloat("_Smoothness",.7f);
                return Save(BoidsSceneAssets.Fish(),"BakedSchoolFish.asset");
            }
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var skin=instance.GetComponentInChildren<SkinnedMeshRenderer>();
            var mesh=new Mesh();skin.BakeMesh(mesh);var vertices=mesh.vertices;var normals=mesh.normals;
            Matrix4x4 matrix=instance.transform.worldToLocalMatrix*skin.transform.localToWorldMatrix;
            Bounds bounds=new Bounds(matrix.MultiplyPoint3x4(vertices[0]),Vector3.zero);
            for(int i=0;i<vertices.Length;i++){vertices[i]=matrix.MultiplyPoint3x4(vertices[i]);normals[i]=matrix.MultiplyVector(normals[i]).normalized;bounds.Encapsulate(vertices[i]);}
            Quaternion rotation=bounds.size.x>bounds.size.z?Quaternion.Euler(0,-90,0):Quaternion.identity;
            float size=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
            for(int i=0;i<vertices.Length;i++){vertices[i]=rotation*(vertices[i]-bounds.center)/size;normals[i]=rotation*normals[i];}
            mesh.vertices=vertices;mesh.normals=normals;mesh.RecalculateBounds();mesh.bounds=new Bounds(Vector3.zero,Vector3.one*2f);
            material=Save(new Material(Shader.Find("BoidsUnderwaterScene/EnvironmentLit")),"SchoolFish.mat");material.enableInstancing=true;
            var original=skin.sharedMaterial;Texture texture=original.HasProperty("_BaseMap")?original.GetTexture("_BaseMap"):original.mainTexture;
            if(texture==null&&original.HasProperty("_MainTex"))texture=original.GetTexture("_MainTex");
            material.SetTexture("_BaseMap",texture);material.SetFloat("_Triplanar",0);material.SetFloat("_Fish",1);material.SetFloat("_Algae",0);material.SetFloat("_Smoothness",.7f);
            material.SetColor("_BaseColor",new Color(1.05f,1.09f,1.11f));
            Debug.Log("BOIDS_FISH sourceBounds="+bounds+" vertices="+vertices.Length+" texture="+(texture==null?"NONE":texture.name));
            Object.DestroyImmediate(instance);return Save(mesh,"BakedSchoolFish.asset");
        }
        private static BoidsSchool School(string name,Mesh mesh,Material material,int count,float size,float speed,float width,float progress,float spread,Vector3[] points)
        {
            var school=new GameObject(name).AddComponent<BoidsSchool>();school.enabled=false;
            Set(school,"fishMesh",mesh);Set(school,"fishMaterial",material);Set(school,"count",count);Set(school,"fishLength",size);
            Set(school,"cruiseSpeed",speed);Set(school,"corridorRadius",width);Set(school,"initialProgress",progress);Set(school,"schoolSpread",spread);
            Set(school,"seed",count+19);Set(school,"separationRadius",size*1.2f);Set(school,"perceptionRadius",size*4);
            var so=new SerializedObject(school);var route=so.FindProperty("waypoints");route.arraySize=points.Length;
            for(int i=0;i<points.Length;i++)route.GetArrayElementAtIndex(i).vector3Value=points[i];so.ApplyModifiedPropertiesWithoutUndo();school.enabled=true;
            return school;
        }
        private static void MakeVolume()
        {
            var profile=Save(ScriptableObject.CreateInstance<VolumeProfile>(),"UnderwaterColorGrade.asset");
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
            var grade=profile.Add<ColorAdjustments>(true);grade.postExposure.Override(.25f);grade.contrast.Override(14);grade.saturation.Override(-7);
            var bloom=profile.Add<Bloom>(true);bloom.threshold.Override(1.15f);bloom.intensity.Override(.24f);bloom.scatter.Override(.65f);
            var vignette=profile.Add<Vignette>(true);vignette.intensity.Override(.17f);vignette.smoothness.Override(.4f);
            foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);
            var volume=new GameObject("Underwater color grade").AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
        }
        private static void Particles(string name,Vector3 position,Vector3 size,bool bubbles,bool ambient=false)
        {
            if(bubbles&&!ambient)position.y=Height(position.x,position.z)+.18f;
            var obj=new GameObject(name);obj.transform.position=position;var ps=obj.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.duration=30;main.loop=true;main.prewarm=true;main.maxParticles=bubbles?(ambient?420:1050):1400;
            main.startLifetime=bubbles?(ambient?new ParticleSystem.MinMaxCurve(15,22):new ParticleSystem.MinMaxCurve(12,17)):new ParticleSystem.MinMaxCurve(22,35);
            main.startSpeed=0;main.startSize=bubbles?(ambient?new ParticleSystem.MinMaxCurve(.05f,.13f):new ParticleSystem.MinMaxCurve(.12f,.30f)):new ParticleSystem.MinMaxCurve(.025f,.07f);
            main.simulationSpace=ParticleSystemSimulationSpace.World;main.startColor=Color.white;
            var emission=ps.emission;emission.rateOverTime=bubbles?(ambient?16:48):40;
            var shape=ps.shape;shape.shapeType=bubbles&&!ambient?ParticleSystemShapeType.Cone:ParticleSystemShapeType.Box;
            if(bubbles&&!ambient){shape.radius=.65f;shape.angle=0;shape.rotation=new Vector3(-90,0,0);}else shape.scale=size;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;
            velocity.x=new ParticleSystem.MinMaxCurve(bubbles?-.035f:-.04f,bubbles?.035f:.04f);velocity.y=bubbles?(ambient?new ParticleSystem.MinMaxCurve(.2f,.45f):new ParticleSystem.MinMaxCurve(1.35f,1.5f)):new ParticleSystem.MinMaxCurve(-.025f,.025f);velocity.z=new ParticleSystem.MinMaxCurve(-.03f,.03f);
            var noise=ps.noise;noise.enabled=true;noise.strength=bubbles?.055f:.045f;noise.frequency=.22f;noise.scrollSpeed=.06f;
            if(bubbles&&!ambient){var growth=ps.sizeOverLifetime;growth.enabled=true;growth.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.8f,1,1.18f));}
            var lifetime=ps.colorOverLifetime;lifetime.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.8f,.12f),new GradientAlphaKey(.8f,.75f),new GradientAlphaKey(0,1)});lifetime.color=gradient;
            var material=AssetDatabase.LoadAssetAtPath<Material>(Generated+(bubbles?"/Bubbles.mat":"/MarineSnow.mat"));
            if(material==null)material=Save(new Material(Shader.Find("BoidsUnderwaterScene/UnderwaterParticles")),bubbles?"Bubbles.mat":"MarineSnow.mat");
            material.SetFloat("_Ring",bubbles?1:0);material.SetColor("_BaseColor",bubbles?new Color(.86f,.96f,1f,.86f):new Color(.67f,.86f,.85f,.3f));EditorUtility.SetDirty(material);
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;ps.Play();
        }
        private static void MakeKelp()
        {
            var verts=new List<Vector3>();var tris=new List<int>();var uv=new List<Vector2>();
            for(int blade=0;blade<7;blade++)
            {
                float heading=blade*2.399963f;Vector3 across=new Vector3(Mathf.Cos(heading),0,Mathf.Sin(heading));
                float height=2.7f+blade*.36f;
                for(int j=0;j<=16;j++)
                {
                    float t=j/16f;Vector3 c=across*(.2f+Mathf.Sin(t*2.8f+blade)*t*.6f)+Vector3.up*t*height;
                    float width=Mathf.Sin(t*Mathf.PI)*(.09f+blade*.008f);verts.Add(c-across*width);verts.Add(c+across*width);uv.Add(new Vector2(0,t));uv.Add(new Vector2(1,t));
                    if(j==16)continue;int k=blade*34+j*2;tris.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});
                }
            }
            var mesh=new Mesh{vertices=verts.ToArray(),triangles=tris.ToArray(),uv=uv.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.bounds=new Bounds(new Vector3(0,2.5f,0),new Vector3(5,7,5));mesh=Save(mesh,"KelpFronds.asset");
            var mat=Save(new Material(Shader.Find("BoidsUnderwaterScene/EnvironmentLit")),"Kelp.mat");mat.SetFloat("_Triplanar",0);mat.SetFloat("_Sway",1);mat.SetFloat("_Cull",0);mat.SetColor("_BaseColor",new Color(.09f,.23f,.12f));mat.enableInstancing=true;
            var root=new GameObject("Swaying kelp beds").transform;
            for(int i=0;i<110;i++)
            {
                float z=UnityEngine.Random.Range(-22f,94f);float x=(i%2==0?-1:1)*UnityEngine.Random.Range(9f,30f);
                var obj=MeshObject("Kelp "+i,mesh,mat,new Vector3(x,Height(x,z),z),Vector3.one*UnityEngine.Random.Range(.5f,1.25f));obj.transform.SetParent(root);obj.transform.Rotate(0,UnityEngine.Random.Range(0,360),0);
                var renderer=obj.GetComponent<Renderer>();renderer.shadowCastingMode=ShadowCastingMode.Off;
                var lod=obj.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.025f,new[]{renderer})});lod.RecalculateBounds();
            }
        }
        private static void MakeForegroundRocks(Transform parent,Material rock)
        {
            Vector3[] bases={new Vector3(-11,0,-10),new Vector3(11,0,-7),new Vector3(-13,0,10),new Vector3(17,0,27)};
            string[] coralNames={"Table","Finger","Elkhorn","Heliopora","Pocillopora","Tube"};
            var planted=new List<Vector3>();
            var radii=new List<float>();
            for(int bank=0;bank<bases.Length;bank++)
            {
                for(int k=0;k<6;k++)
                {
                    float x=bases[bank].x+(float)Math.Cos(k*2.4)*2.8f,z=bases[bank].z+(float)Math.Sin(k*2.4)*4f;
                    var obj=ArtObject("SM_Rock0"+(k%5+1));
                    obj.name="Coral bank foundation";obj.transform.SetParent(parent);
                    Fit(obj,new Vector3(4.5f+k*.3f,2.3f+k*.2f,4.3f),new Vector3(x,Height(x,z)-.5f,z));
                    foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterial=rock;
                    foreach(var c in obj.GetComponentsInChildren<Collider>())c.gameObject.layer=6;
                }
                Physics.SyncTransforms();
                for(int k=0;k<12;k++)
                {
                    bool onRock=k<3;
                    var obj=ArtObject("SM_Coral_"+coralNames[k%6]);
                    obj.name="ForegroundCoral"+(bank*12+k);obj.transform.SetParent(parent);
                    float size=UnityEngine.Random.Range(.7f,onRock?1.8f:1.55f);
                    Fit(obj,new Vector3(size,k%6==0?size*.28f:size*.85f,size),Vector3.zero);
                    obj.transform.Rotate(0,UnityEngine.Random.Range(0,360),0,Space.World);ConvertMaterials(obj);
                    foreach(var c in obj.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
                    bool placed=false;
                    for(int attempt=0;attempt<140;attempt++)
                    {
                        float angle=UnityEngine.Random.Range(0,Mathf.PI*2);
                        float radius=UnityEngine.Random.Range(onRock?.8f:4.5f,onRock?5.5f:11f);
                        Vector3 location=bases[bank]+new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);
                        if(Mathf.Abs(location.x)>25||Mathf.Abs(location.x)<3.5f)continue;
                        bool close=false;
                        for(int n=0;n<planted.Count;n++)
                            if(Vector2.Distance(new Vector2(location.x,location.z),new Vector2(planted[n].x,planted[n].z))<size*.6f+radii[n]+.8f){close=true;break;}
                        if(close)continue;
                        Bounds bounds=CoralBounds(obj);
                        obj.transform.position+=new Vector3(location.x-bounds.center.x,Height(location.x,location.z)-bounds.min.y,location.z-bounds.center.z);
                        if(!GroundCoral(obj,onRock?null:seabedCollider))continue;
                        bounds=CoralBounds(obj);
                        if(!onRock)
                        {
                            bool blocked=false;
                            var hits=Physics.OverlapBox(bounds.center+Vector3.up*.05f,new Vector3(bounds.extents.x+.2f,Mathf.Max(.05f,bounds.extents.y-.05f),bounds.extents.z+.2f),Quaternion.identity,1<<6,QueryTriggerInteraction.Ignore);
                            foreach(var hit in hits)if(hit!=seabedCollider){blocked=true;break;}
                            if(blocked)continue;
                        }
                        else if(bounds.min.y<Height(location.x,location.z)+.4f)continue;
                        planted.Add(location);radii.Add(size*.6f);placed=true;break;
                    }
                    if(!placed)Object.DestroyImmediate(obj);
                }
                Physics.SyncTransforms();
            }
        }

        private static Bounds CoralBounds(GameObject obj)
        {
            var renderers=obj.GetComponentsInChildren<Renderer>();
            Bounds bounds=renderers[0].bounds;
            foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        public static List<Vector3> CoralBasePoints(GameObject obj)
        {
            var points=new List<Vector3>();
            float lowest=float.PositiveInfinity;
            var lod=obj.GetComponentInChildren<LODGroup>();
            var visible=new HashSet<Renderer>();
            if(lod!=null && lod.GetLODs().Length>0)foreach(var renderer in lod.GetLODs()[0].renderers)visible.Add(renderer);
            foreach(var filter in obj.GetComponentsInChildren<MeshFilter>())
            {
                if(filter.sharedMesh==null||(visible.Count>0&&!visible.Contains(filter.GetComponent<Renderer>())))continue;
                foreach(var vertex in filter.sharedMesh.vertices)
                {
                    Vector3 point=filter.transform.TransformPoint(vertex);
                    points.Add(point);lowest=Mathf.Min(lowest,point.y);
                }
            }
            float bottomBand=Mathf.Max(.035f,CoralBounds(obj).size.y*.055f);
            points.RemoveAll(p=>p.y>lowest+bottomBand);
            return points;
        }

        private static bool GroundCoral(GameObject obj,Collider support)
        {
            var points=CoralBasePoints(obj);
            float correction=float.NegativeInfinity,minSurface=float.PositiveInfinity,maxSurface=float.NegativeInfinity;
            foreach(var point in points)
            {
                var ray=new Ray(new Vector3(point.x,45,point.z),Vector3.down);
                RaycastHit hit;
                bool found=support!=null?support.Raycast(ray,out hit,80):Physics.Raycast(ray,out hit,80,1<<6,QueryTriggerInteraction.Ignore);
                if(!found||hit.normal.y<.68f||hit.point.y>Height(point.x,point.z)+4.2f)return false;
                correction=Mathf.Max(correction,hit.point.y-point.y-.025f);
                minSurface=Mathf.Min(minSurface,hit.point.y);maxSurface=Mathf.Max(maxSurface,hit.point.y);
            }
            if(points.Count==0||(support==null&&maxSurface-minSurface>.32f))return false;
            obj.transform.position+=Vector3.up*correction;
            return true;
        }
        private static void Set(Object target,string property,Object value){var so=new SerializedObject(target);so.FindProperty(property).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void Set(Object target,string property,float value){var so=new SerializedObject(target);so.FindProperty(property).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void Set(Object target,string property,int value){var so=new SerializedObject(target);so.FindProperty(property).intValue=value;so.ApplyModifiedPropertiesWithoutUndo();}

        private static void ConfigureSchoolPresentation()
        {
            var template=AssetDatabase.LoadAssetAtPath<Material>(Generated+"/SchoolFish.mat");
            foreach(var school in Object.FindObjectsOfType<BoidsSchool>())
            {
                string file;
                Color color;
                switch(school.name)
                {
                    case "VortexSchool": file="VortexFish.mat";color=new Color(1.55f,1.03f,.24f);break;
                    case "DistantFlowingRibbon": file="DistantFish.mat";color=new Color(.24f,.88f,1.5f);break;
                    case "ArchPassageSchool": file="ArchFish.mat";color=new Color(1.08f,1.15f,1.23f);break;
                    case "ForegroundReefResidents": file="ForegroundFish.mat";color=new Color(1.6f,.57f,.35f);break;
                    default: continue;
                }
                var material=Save(new Material(template),file);
                material.name=System.IO.Path.GetFileNameWithoutExtension(file);
                material.SetColor("_BaseColor",color);material.SetFloat("_Desaturation",.82f);material.enableInstancing=true;
                EditorUtility.SetDirty(material);Set(school,"fishMaterial",material);
                if(school.IsVortex)ConfigureVortexSchool(school);
            }
            var camera=Camera.main;
            if(camera!=null)
            {
                camera.transform.position=new Vector3(0,5,-8);
                camera.transform.LookAt(new Vector3(2,9,33));
            }
        }

        private static void ConfigureVortexSchool(BoidsSchool school)
        {
            var settings=new SerializedObject(school);
            settings.FindProperty("vortex").boolValue=true;
            settings.FindProperty("vortexCenter").vector3Value=new Vector3(0,14.5f,62);
            settings.FindProperty("vortexHeight").floatValue=21;
            settings.FindProperty("vortexRadius").floatValue=7;
            settings.FindProperty("vortexVerticalFrequency").floatValue=.045f;
            settings.FindProperty("fishLength").floatValue=.95f;
            settings.FindProperty("cruiseSpeed").floatValue=2.6f;
            settings.FindProperty("corridorRadius").floatValue=1.8f;
            settings.FindProperty("separationRadius").floatValue=1.05f;
            settings.FindProperty("separationWeight").floatValue=2.6f;
            settings.FindProperty("perceptionRadius").floatValue=2.4f;
            settings.FindProperty("alignmentWeight").floatValue=.35f;
            settings.FindProperty("cohesionWeight").floatValue=.025f;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void BuildSchoolReview()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            ConfigureSchoolPresentation();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
            BoidsSceneValidation.Validate();
            string output=Environment.GetEnvironmentVariable("BOIDS_BUILD_PATH");
            if(string.IsNullOrEmpty(output))throw new InvalidOperationException("BOIDS_BUILD_PATH missing");
            var report=BuildPipeline.BuildPlayer(new[]{ScenePath},output,BuildTarget.StandaloneWindows64,BuildOptions.Development);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("School review build failed");
            Debug.Log("BOIDS_BUILD_SUCCEEDED");
        }
        public static void BuildPublicRelease()
        {
            BuildScene();BoidsSceneValidation.Validate();
            BoidsPublicationValidation.Validate();
            EditorSceneManager.OpenScene(ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            var dependencies=AssetDatabase.GetDependencies(ScenePath,true);
            foreach(string dependency in dependencies)
                if(dependency.StartsWith(Art)||dependency.StartsWith(Fish)||dependency.StartsWith("Assets/FFT-Ocean"))
                    throw new InvalidOperationException("Excluded scene dependency "+dependency);
            string output=Environment.GetEnvironmentVariable("BOIDS_BUILD_PATH");
            if(string.IsNullOrEmpty(output))throw new InvalidOperationException("BOIDS_BUILD_PATH missing");
            var report=BuildPipeline.BuildPlayer(new[]{ScenePath},output,BuildTarget.StandaloneWindows64,BuildOptions.Development);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Public Boids build failed");
            Debug.Log("BOIDS_PUBLIC_BUILD_SUCCEEDED");
        }
        public static void BuildPlayer()
        {
            BuildScene();BoidsSceneValidation.Validate();
            string output=Environment.GetEnvironmentVariable("BOIDS_BUILD_PATH");if(string.IsNullOrEmpty(output))throw new InvalidOperationException("BOIDS_BUILD_PATH missing");
            var report=BuildPipeline.BuildPlayer(new[]{ScenePath},output,BuildTarget.StandaloneWindows64,BuildOptions.Development);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Boids build failed");
            Debug.Log("BOIDS_BUILD_SUCCEEDED");
        }
    }
}
