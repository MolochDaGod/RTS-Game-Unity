using UnityEditor;
using UnityEngine;
using System.IO;

public class WebGLBuilder
{
    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        BuildWebGLProject();
    }

    public static void BuildWebGLProject()
    {
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build");
        
        // Ensure build directory exists
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        // Configure build settings
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenePaths();
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        // Set WebGL player settings
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.runInBackground = true;

        Debug.Log("Starting WebGL build...");
        Debug.Log("Build path: " + buildPath);
        Debug.Log("Scenes: " + string.Join(", ", buildPlayerOptions.scenes));

        // Build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + report.summary.totalSize + " bytes");
            Debug.Log("Build location: " + buildPath);
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }

    private static string[] GetScenePaths()
    {
        // Get all enabled scenes from build settings
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        // If no scenes in build settings, try to find some
        if (scenes.Length == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            scenes = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                scenes[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }
        }

        return scenes;
    }
}
