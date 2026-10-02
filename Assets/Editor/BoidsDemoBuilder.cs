using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class BoidsDemoBuilder
{
    private const string Generated = "Assets/Demo";

    [MenuItem("Tools/Boids/Rebuild Public Demo")]
    public static void CreateDemo()
    {
        Directory.CreateDirectory(Generated);
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.35f, 0.52f, 0.55f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.035f, 0.22f, 0.27f);
        RenderSettings.fogDensity = 0.011f;

        var light = new GameObject("Sunlight").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.8f;
        light.color = new Color(0.78f, 0.94f, 1f);
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(52f, -25f, 0f);

        var sand = Material("Seabed", new Color(0.42f, 0.50f, 0.45f), 0f, 0.15f);
        var stone = Material("Rock", new Color(0.20f, 0.31f, 0.33f), 0f, 0.2f);
        var body = Material("FishBody", new Color(0.56f, 0.78f, 0.83f), 0.3f, 0.5f);
        var fins = Material("FishFins", new Color(0.89f, 0.65f, 0.23f), 0.05f, 0.35f);
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Seabed";
        ground.transform.position = new Vector3(0f, -3f, 0f);
        ground.transform.localScale = new Vector3(12f, 1f, 12f);
        ground.GetComponent<Renderer>().sharedMaterial = sand;
        ground.layer = 6;

        UnityEngine.Random.InitState(84);
        for (int i = 0; i < 14; i++)
        {
            float angle = i * Mathf.PI * 2f / 14;
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Obstacle Rock " + (i + 1);
            rock.layer = 6;
            rock.transform.position = new Vector3(Mathf.Sin(angle) * 27f, -1.8f, Mathf.Cos(angle) * 27f);
            rock.transform.localScale = new Vector3(4f + i % 3, 5f + i % 4, 5f);
            rock.GetComponent<Renderer>().sharedMaterial = stone;
        }
        var pillar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pillar.name = "Avoidance Pillar";
        pillar.layer = 6;
        pillar.transform.position = new Vector3(18f, 5f, 0f);
        pillar.transform.localScale = new Vector3(4f, 8f, 4f);
        pillar.GetComponent<Renderer>().sharedMaterial = stone;

        var fish = new GameObject("BoidAgent");
        fish.AddComponent<BoidAgent>();
        var visual = new GameObject("Visual");
        visual.transform.SetParent(fish.transform, false);
        visual.AddComponent<MeshFilter>().sharedMesh = FishMesh();
        visual.AddComponent<MeshRenderer>().sharedMaterials = new[] { body, fins };
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fish, Generated + "/BoidAgent.prefab");
        UnityEngine.Object.DestroyImmediate(fish);

        var volume = new GameObject("SwimVolume").AddComponent<BoxCollider>();
        volume.isTrigger = true;
        volume.size = new Vector3(64f, 28f, 64f);
        volume.transform.position = new Vector3(0f, 11f, 0f);
        var vortex = new GameObject("VortexCenter").transform;
        vortex.position = new Vector3(0f, 11f, 0f);
        var manager = new GameObject("BoidManager").AddComponent<BoidManager>();
        manager.transform.position = vortex.position;
        var settings = new SerializedObject(manager);
        settings.FindProperty("boidPrefab").objectReferenceValue = prefab.GetComponent<BoidAgent>();
        settings.FindProperty("swimVolume").objectReferenceValue = volume;
        settings.FindProperty("vortexCenter").objectReferenceValue = vortex;
        settings.FindProperty("spawnArea").vector3Value = new Vector3(16f, 4f, 16f);
        settings.FindProperty("boidCount").intValue = 240;
        settings.FindProperty("obstacleMask").intValue = 1 << 6;
        Set(settings, "startSpeed", 6f);
        Set(settings, "separationRadius", 1.8f);
        Set(settings, "separationWeight", 3f);
        Set(settings, "alignmentWeight", 0.45f);
        Set(settings, "cohesionWeight", 0.08f);
        Set(settings, "boundsWeight", 2f);
        Set(settings, "collisionCheckRadius", 1f);
        Set(settings, "collisionAvoidDistance", 7f);
        Set(settings, "obstacleAvoidanceWeight", 10f);
        Set(settings, "wanderWeight", 0.3f);
        Set(settings, "vortexRadius", 20f);
        Set(settings, "vortexWeight", 0.1f);
        Set(settings, "vortexShapeWeight", 1f);
        Set(settings, "targetWeight", 0f);
        settings.ApplyModifiedPropertiesWithoutUndo();

        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(28f, 22f, -40f);
        camera.transform.LookAt(new Vector3(0f, 9f, 0f));
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = RenderSettings.fogColor;
        camera.fieldOfView = 56f;
        camera.farClipPlane = 200f;
        camera.gameObject.AddComponent<AudioListener>();
        camera.gameObject.AddComponent<CameraController>();
        var additional = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        additional.renderPostProcessing = true;
        additional.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, Generated + "/UnderwaterProfile.asset");
        var color = profile.Add<ColorAdjustments>(true);
        color.contrast.Override(8f);
        color.saturation.Override(-10f);
        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.15f);
        foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
        var post = new GameObject("Underwater Volume").AddComponent<Volume>();
        post.isGlobal = true;
        post.sharedProfile = profile;
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, Generated + "/BoidsDemo.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Generated + "/BoidsDemo.unity", true) };
        Debug.Log("BOIDS_DEMO_CREATED: original runtime scripts, generated visuals, 240 agents");
    }

    public static void PrepareAndBuild()
    {
        CreateDemo();
        Build(Generated + "/BoidsDemo.unity");
    }

    public static void Build(string scene)
    {
        string output = Environment.GetEnvironmentVariable("BOIDS_BUILD_PATH");
        if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("BOIDS_BUILD_PATH is required");
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { scene },
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new InvalidOperationException("Boids player build failed");
        Debug.Log("BOIDS_PLAYER_BUILT");
    }

    private static void Set(SerializedObject settings, string name, float value)
    {
        settings.FindProperty(name).floatValue = value;
    }

    private static Material Material(string name, Color color, float metallic, float smoothness)
    {
        var result = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, enableInstancing = true };
        result.SetColor("_BaseColor", color);
        result.SetFloat("_Metallic", metallic);
        result.SetFloat("_Smoothness", smoothness);
        AssetDatabase.CreateAsset(result, Generated + "/" + name + ".mat");
        return result;
    }

    private static Mesh FishMesh()
    {
        const int rings = 9;
        const int segments = 8;
        var vertices = new List<Vector3>();
        var body = new List<int>();
        var fins = new List<int>();
        for (int ring = 0; ring < rings; ring++)
        {
            float t = ring / (float)(rings - 1);
            float radius = Mathf.Sin(t * Mathf.PI) * 0.26f + 0.015f;
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = segment * 2f * Mathf.PI / segments;
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius * 0.55f, Mathf.Sin(angle) * radius, Mathf.Lerp(-0.45f, 0.65f, t)));
                if (ring == 0) continue;
                int a = (ring - 1) * segments + segment;
                int b = (ring - 1) * segments + (segment + 1) % segments;
                int c = ring * segments + segment;
                int d = ring * segments + (segment + 1) % segments;
                body.AddRange(new[] { a, c, b, b, c, d });
            }
        }
        Triangle(vertices, fins, new Vector3(0f, 0f, -0.4f), new Vector3(0f, 0.3f, -0.85f), new Vector3(0f, -0.3f, -0.85f));
        Triangle(vertices, fins, new Vector3(0f, 0.2f, -0.2f), new Vector3(0f, 0.43f, -0.27f), new Vector3(0f, 0.18f, 0.3f));
        Triangle(vertices, fins, new Vector3(-0.1f, -0.02f, 0.05f), new Vector3(-0.37f, -0.1f, -0.22f), new Vector3(-0.1f, -0.06f, -0.2f));
        Triangle(vertices, fins, new Vector3(0.1f, -0.02f, 0.05f), new Vector3(0.37f, -0.1f, -0.22f), new Vector3(0.1f, -0.06f, -0.2f));
        var mesh = new Mesh { name = "Generated Fish", subMeshCount = 2 };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(body, 0);
        mesh.SetTriangles(fins, 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, Generated + "/Fish.asset");
        return mesh;
    }

    private static void Triangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
    {
        int start = vertices.Count;
        vertices.AddRange(new[] { a, b, c, a, c, b });
        triangles.AddRange(new[] { start, start + 1, start + 2, start + 3, start + 4, start + 5 });
    }
}
