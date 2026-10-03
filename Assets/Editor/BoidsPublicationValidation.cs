using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class BoidsPublicationValidation
{
    public static void Validate()
    {
        var scene = EditorSceneManager.OpenScene(CinematicReef.Editor.ReefSceneBuilder.ScenePath);
        var errors = new List<string>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                    errors.Add("Missing script on " + transform.name);
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                foreach (var material in renderer.sharedMaterials)
                    if (material == null || material.shader == null || ShaderUtil.ShaderHasError(material.shader))
                        errors.Add("Missing or invalid material on " + renderer.name);
        }
        var schools = UnityEngine.Object.FindObjectsOfType<CinematicReef.ReefSchool>();
        if (schools.Length != 4) errors.Add("Expected four independent schools");
        foreach (var school in schools)
        {
            var configuration = new SerializedObject(school);
            if (configuration.FindProperty("fishMesh").objectReferenceValue == null) errors.Add("Missing fish mesh");
            if (configuration.FindProperty("fishMaterial").objectReferenceValue == null) errors.Add("Missing fish material");
        }
        if (Camera.main == null) errors.Add("Missing main camera");
        foreach (string path in AssetDatabase.GetAllAssetPaths())
            if (path.StartsWith("Assets/FFT-Ocean") || path.StartsWith("Assets/Fish/") || path.StartsWith("Assets/LeartesStudios/") || path.StartsWith("Assets/WaterCausticsModules/"))
                errors.Add("Excluded source asset present: " + path);
        foreach (string path in AssetDatabase.FindAssets("t:UniversalRendererData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(AssetDatabase.GUIDToAssetPath(path));
            foreach (var feature in data.rendererFeatures)
                if (feature == null || feature.GetType().Name.StartsWith("FFTOcean") || feature.GetType().Name.StartsWith("WaterCaustics"))
                    errors.Add("Unexpected renderer dependency");
        }
        string report = "{\"checks\":\"scene scripts, materials, schools, camera and excluded dependencies\",\"passed\":" + (errors.Count == 0 ? "true" : "false") + ",\"issueCount\":" + errors.Count + "}";
        File.WriteAllText("docs/publication-validation.json", report);
        if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
        Debug.Log("BOIDS_PUBLICATION_VALIDATION_PASSED");
    }
}
