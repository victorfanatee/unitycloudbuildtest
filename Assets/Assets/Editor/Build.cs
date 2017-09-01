using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class Build : MonoBehaviour
{
    public static void OnPostprocessBuild(string path)
    {
        string buildPath = Path.Combine(path, PlayerSettings.productName);

        UnityEngine.Debug.Log("[BUILD] Running: PostProcessBuild");
        UnityEngine.Debug.Log("[BUILD] Path: " + buildPath);

        /* create gradle files */ 
        var CreateGradleWrapper = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "gradle",
                WorkingDirectory = buildPath,
                Arguments = "wrapper --gradle-version 2.14.1 -q",
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };

        UnityEngine.Debug.Log("[BUILD] Running command: " + CreateGradleWrapper.StartInfo.FileName + " " + CreateGradleWrapper.StartInfo.Arguments);
        CreateGradleWrapper.Start();

        CreateGradleWrapper.WaitForExit();

        /* add multidex support */
        string buildScriptPath = Path.Combine(buildPath, "build.gradle");

        string pattern = @"(android\s+{.*defaultConfig\s+{.*?\n)((\s+)}.*})";
        string substitution = "$1$3$3multiDexEnabled true\n$2";
        string input = File.ReadAllText(buildScriptPath, Encoding.UTF8);

        RegexOptions options = RegexOptions.Singleline;

        Regex regex = new Regex(pattern, options);
        string result = regex.Replace(input, substitution);

        result = result.Replace("defaultConfig", "  dexOptions {javaMaxHeapSize \"4g\"}\r\ndefaultConfig");

        File.WriteAllText(buildScriptPath, result);

        /* run gradle wrapper */
        var RunGradleWrapper = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(buildPath, "gradlew.bat"),
                Arguments = "assembleDebug --stacktrace",
                WorkingDirectory = buildPath,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };

        UnityEngine.Debug.Log("[BUILD] Running command: " + RunGradleWrapper.StartInfo.FileName + " " + RunGradleWrapper.StartInfo.Arguments);
        RunGradleWrapper.Start();

        RunGradleWrapper.WaitForExit();
    }
}
