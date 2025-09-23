using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace UnityApplication.Editor
{
    [InitializeOnLoad]
    public static class PluginBuilder
    {
        static PluginBuilder()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                BuildPlugin();
            }
        }

        private static void BuildPlugin()
        {
            UnityEngine.Debug.Log("Building GroveGames.ObjectPool.dll...");

            string dotnetPath = GetDotNetPath();
            if (string.IsNullOrEmpty(dotnetPath))
            {
                UnityEngine.Debug.LogError("dotnet CLI not found. Please ensure .NET SDK is installed.");
                return;
            }

            var process = new Process();
            process.StartInfo.FileName = dotnetPath;
            process.StartInfo.Arguments = "build ../../src/GroveGames.ObjectPool/GroveGames.ObjectPool.csproj -c Release -f netstandard2.1 -o ./Assets/Plugins";
            process.StartInfo.WorkingDirectory = Application.dataPath + "/..";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    UnityEngine.Debug.Log("Plugin build completed successfully");
                    AssetDatabase.Refresh();
                }
                else
                {
                    string error = process.StandardError.ReadToEnd();
                    UnityEngine.Debug.LogError($"Plugin build failed: {error}");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to start build process: {ex.Message}");
            }
            finally
            {
                process?.Dispose();
            }
        }

        private static string GetDotNetPath()
        {
            string[] commonPaths;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                commonPaths = new[]
                {
                    "dotnet.exe",
                    @"C:\Program Files\dotnet\dotnet.exe",
                    @"C:\Program Files (x86)\dotnet\dotnet.exe"
                };
            }
            else
            {
                commonPaths = new[]
                {
                    "dotnet",
                    "/usr/local/share/dotnet/dotnet",
                    "/usr/local/bin/dotnet",
                    "/usr/bin/dotnet"
                };
            }

            foreach (string path in commonPaths)
            {
                if (TryExecuteDotNet(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static bool TryExecuteDotNet(string path)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = "--version";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}