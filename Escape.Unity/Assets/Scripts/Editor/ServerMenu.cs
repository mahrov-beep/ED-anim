using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class ServerMenu {
    private const string SERVER_PROJECT_PATH = "/../../Server";

    [MenuItem("Server/Settings...", false, 0)]
    private static void OpenSettings() => ServerMenuSettings.Open();

    [MenuItem("Server/Kill Server Processes", false, 2)]
    private static void KillServerProcessesMenu() => KillServerProcesses();

    [MenuItem("Server/Copy Multicast.dll to Server Project", false, 1)]
    private static void CopyMulticastDllToServerProject() {
        const string src = "Packages/com.multicast.foundation/Runtime/Multicast.dll";
        const string dst = "../server/assemblies/Multicast.dll";

        if (!File.Exists(src)) {
            Debug.LogError($"Multicast.dll not exist at path: {src}");
            return;
        }

        File.Copy(src, dst, true);
    }

    [MenuItem("Server/Run Local Server/Debug", false, 100)]
    public static void RunServerDebug() {
        KillServerProcesses();
        RunServerInternal("Debug", "Development");
    }

    [MenuItem("Server/Run Local Server/Debug (Skip Recompile)", false, 100)]
    public static void RunServerDebugSkipRecompile() {
        KillServerProcesses();
        RunServerInternal("Debug", "Development", "--no-build");
    }

    [MenuItem("Server/Run Local Server/Release", false, 101)]
    public static void RunServerRelease() => RunServerInternal("Release", "Staging");

    public static void RunServerInternal(string configuration, string environment, string extraOptions = "") {
        var arguments = new Dictionary<string, string> {
            ["--assetsPath"]         = Application.dataPath + "/Content.Addressables/",
            ["--tempDirectory"]      = Application.dataPath + "/../Temp/Multicast/ServerTempDatabase/",
            ["--postgresConnection"] = ServerMenuSettings.PostgresConnection,
            ["--urls"]               = "http://localhost:5024",
            ["--jwtIssuer"]          = "multicast/escape",
            ["--jwtAudience"]        = "multicast/escape",
            ["--jwtSigningKey"]      = Guid.Empty.ToString(),
            ["--disableLoadoutLost"] = ServerMenuSettings.DisableLoadoutLost.ToString().ToLower(),
        };

        var argsString = string.Join("", arguments.Select(it => $"{it.Key} \"{it.Value}\" "));

        RunInternal($"-c \"{configuration}\" --environment {environment} {extraOptions} -- {argsString}", SERVER_PROJECT_PATH + "/Game.ServerRunner");
    }

    [MenuItem("Server/Open Server Project", false, 60)]
    private static void OpenQuantumProject() {
        var path = Path.GetFullPath(Application.dataPath + SERVER_PROJECT_PATH + "/Escape.Server.sln");

        if (!File.Exists(path)) {
            EditorUtility.DisplayDialog("Open Server Project", "Solution file '" + path + "' not found", "Ok");
        }

        var uri = new Uri(path);
        Application.OpenURL(uri.AbsoluteUri);
    }

    private static void RunInternal(string args, string projectDirectory) {
        try {
            var cmd = "dotnet run " + args;

            EditorUtility.DisplayProgressBar("QuantumBuildTool", $"Running {projectDirectory}", 1f);
            Debug.Log($"ExecTerminal: {cmd}");
            ExecTerminal(cmd, workingDirectory: Application.dataPath + projectDirectory);
        }
        finally {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void KillServerProcesses() {
        ProcessHelper.KillAllDotnetProcesses();
        ProcessHelper.KillProcessesByNames("ServerRunner", "Game.ServerRunner");
    }

    private static void ExecTerminal(string command, string workingDirectory) {
        Process proc = null;
        try {
#if UNITY_EDITOR_WIN
            string shellCmd    = "cmd.exe";
            string shellCmdArg = "/k";
#else
            string shellCmd = "/bin/bash";
            string shellCmdArg = "-c";
#endif

            string cmdArguments  = shellCmdArg + " \"" + command + "\"";
            var    procStartInfo = new ProcessStartInfo(shellCmd, cmdArguments);
            procStartInfo.WorkingDirectory = workingDirectory;

            proc           = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
        finally {
            proc?.Close();
        }
    }
}