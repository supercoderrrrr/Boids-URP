#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class BoidsPortfolioCapture : MonoBehaviour
{
    private const int Width = 1280;
    private const int Height = 720;
    private string output;
    private Camera view;
    private RenderTexture target;
    private Texture2D readback;
    private readonly StringBuilder errors = new StringBuilder();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-boidsCapture");
        if (index < 0 || index + 1 >= args.Length) return;
        var capture = new GameObject("Boids Portfolio Capture").AddComponent<BoidsPortfolioCapture>();
        capture.output = Path.GetFullPath(args[index + 1]);
    }

    private void OnEnable() => Application.logMessageReceived += Log;
    private void OnDisable() => Application.logMessageReceived -= Log;

    private void Log(string message, string trace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
            errors.AppendLine(type + ": " + message);
    }

    private IEnumerator Start()
    {
        Directory.CreateDirectory(output);
        Time.captureFramerate = 30;
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;
        view = Camera.main;
        var manager = FindObjectOfType<BoidManager>();
        if (view == null || manager == null)
        {
            File.WriteAllText(Path.Combine(output, "failure.txt"), "Missing camera or manager");
            Application.Quit(2);
            yield break;
        }
        var controls = view.GetComponent<CameraController>();
        if (controls != null) controls.enabled = false;
        foreach (var obstacle in FindObjectsOfType<ObstacleController>()) obstacle.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
        target.Create();
        readback = new Texture2D(Width, Height, TextureFormat.RGB24, false);

        // Capture uses the actual agent updates with a fixed simulation timestep
        for (int i = 0; i < 180; i++) yield return null;
        Vector3 position = view.transform.position;
        Quaternion rotation = view.transform.rotation;
        for (int frame = 0; frame < 150; frame++)
        {
            view.transform.position = position + view.transform.right * Mathf.Sin(frame / 150f * Mathf.PI) * 1.5f;
            view.transform.rotation = rotation;
            for (int step = 0; step < 2; step++) yield return null;
            Capture(Path.Combine(output, "frame-" + frame.ToString("0000") + ".png"));
        }
        File.WriteAllText(Path.Combine(output, "capture-result.json"),
            "{\"frames\":150,\"simulationFps\":30,\"outputFps\":15,\"agents\":" + manager.Boids.Count + ",\"runtimeErrors\":" + (errors.Length == 0 ? "false" : "true") + "}");
        File.WriteAllText(Path.Combine(output, "runtime-errors.txt"), errors.ToString());
        Application.Quit(errors.Length == 0 ? 0 : 2);
    }

    private void Capture(string path)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture previousTarget = view.targetTexture;
        view.targetTexture = target;
        view.Render();
        RenderTexture.active = target;
        readback.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false);
        readback.Apply(false, false);
        File.WriteAllBytes(path, readback.EncodeToPNG());
        view.targetTexture = previousTarget;
        RenderTexture.active = previous;
    }

    private void OnDestroy()
    {
        Time.captureFramerate = 0;
        if (target != null) { target.Release(); Destroy(target); }
        if (readback != null) Destroy(readback);
    }
}
#endif
