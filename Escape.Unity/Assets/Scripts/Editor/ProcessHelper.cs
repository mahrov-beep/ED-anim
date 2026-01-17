using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class ProcessHelper {
    public static void KillProcessByName(string processName) {
        try {
#if UNITY_EDITOR_WIN
            var cleanName = processName.Replace(".exe", "");
            ExecuteCommand($"taskkill /F /IM {cleanName}.exe", timeoutMs: 5000);
#else
            ExecuteCommand($"pkill -9 {processName}", timeoutMs: 5000);
#endif
        }
        catch (Exception e) {
            Debug.LogError($"Error killing process '{processName}': {e.Message}");
        }
    }

    public static void KillProcessesByNames(params string[] processNames) {
        foreach (var name in processNames) {
            KillProcessByName(name);
        }
    }

    public static (int exitCode, string stdout, string stderr) ExecuteCommand(string command, string workingDirectory = null, int timeoutMs = 60000) {
        try {
#if UNITY_EDITOR_WIN
            var shellCmd = "cmd.exe";
            var shellArgs = "/c " + command;
#else
            var shellCmd = "/bin/bash";
            var shellArgs = "-c \"" + command.Replace("\"", "\\\"") + "\"";
#endif
            var psi = new ProcessStartInfo(shellCmd, shellArgs) {
                WorkingDirectory = workingDirectory ?? Application.dataPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            if (!process.WaitForExit(timeoutMs)) {
                try {
                    process.Kill();
                }
                catch {
                }
                return (-1, "", "Process timed out");
            }

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();

            return (process.ExitCode, stdout, stderr);
        }
        catch (Exception e) {
            Debug.LogException(e);
            return (-1, "", e.Message);
        }
    }

    public static void ExecuteCommandAsync(string command, string workingDirectory = null) {
        try {
#if UNITY_EDITOR_WIN
            var shellCmd = "cmd.exe";
            var shellArgs = "/k " + command;
#else
            var shellCmd = "/bin/bash";
            var shellArgs = "-c \"" + command.Replace("\"", "\\\"") + "\"";
#endif
            var psi = new ProcessStartInfo(shellCmd, shellArgs) {
                WorkingDirectory = workingDirectory ?? Application.dataPath,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            var process = new Process { StartInfo = psi };
            process.Start();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public static bool IsProcessRunning(string processName) {
        try {
            var processes = Process.GetProcessesByName(processName);
            var isRunning = processes.Length > 0;
            foreach (var p in processes) {
                p.Dispose();
            }
            return isRunning;
        }
        catch {
            return false;
        }
    }

    public static int GetProcessCount(string processName) {
        try {
            var processes = Process.GetProcessesByName(processName);
            var count = processes.Length;
            foreach (var p in processes) {
                p.Dispose();
            }
            return count;
        }
        catch {
            return 0;
        }
    }

    public static void KillAllDotnetProcesses() {
#if UNITY_EDITOR_WIN
        ExecuteCommand("taskkill /F /IM dotnet.exe /T", timeoutMs: 10000);
#else
        ExecuteCommand("pkill -9 dotnet", timeoutMs: 5000);
#endif
    }
}
