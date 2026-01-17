using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class DockerHelper {
    private static string composeCmd;

    public enum ServerStatus {
        Unknown,
        Stopped,
        Starting,
        Running,
        Healthy,
        Unhealthy
    }

    public class ServerInfo {
        public ServerStatus Status           { get; set; } = ServerStatus.Unknown;
        public string       Version          { get; set; } = "";
        public bool         IsChecking       { get; set; } = false;
        public string       CurrentOperation { get; set; } = "";
    }

    private static          ServerInfo              cachedServerInfo = new ServerInfo();
    private static          CancellationTokenSource statusCheckCts;
    private static readonly object                  LockObj            = new object();
    private static readonly System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient {
        Timeout = TimeSpan.FromSeconds(2)
    };

    private static string cachedLocalIP;

    [MenuItem("Server/Docker Compose/Create Container (up -d)", false, 90)]
    private static void MenuUp() => ComposeUp();

    [MenuItem("Server/Docker Compose/Pause", false, 91)]
    private static void MenuPause() => ComposePause();

    [MenuItem("Server/Docker Compose/Stop", false, 92)]
    private static void MenuStop() => ComposeStop();

    [MenuItem("Server/Docker Compose/Remove container (down -v)", false, 93)]
    private static void MenuRemoveContainer() => ComposeRemoveContainer();

    [MenuItem("Server/Docker Compose/Reload (down -v && up -d --build)", false, 94)]
    private static void MenuReload() => ComposeReload();

    [MenuItem("Server/Docker Compose/SetConnectionString", false, 97)]
    private static void MenuSetConnectionString() => SetConnectionString();

    public static void SetConnectionString() {
        var root = GetRoot();
        if (ResolveComposePath(root) == null) {
            Debug.LogError("compose.yml(.yaml) не найден");
            return;
        }

        var (cs, soServices, _) = Exec($"{GetComposeCmd()} {GetComposeFile(root)} config --services", root, 8000);
        var services = (cs == 0 ? soServices.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>())
            .Select(s => s.Trim()).Where(s => s.Length > 0)
            .OrderByDescending(s =>
                s.Contains("postgres", StringComparison.OrdinalIgnoreCase) ? 3 :
                s.Equals("db", StringComparison.OrdinalIgnoreCase) ? 2 :
                s.Contains("db", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ToArray();

        string host = "localhost";
        string port = "5432";

        foreach (var svc in services) {
            var (cp, soPort, _) = Exec($"{GetComposeCmd()} {GetComposeFile(root)} port {svc} 5432", root, 5000);
            var line = soPort?.Trim();
            if (cp == 0 && !string.IsNullOrEmpty(line)) {
                var m = Regex.Match(line, @"(?<host>[\w\.\[\]:]+):(?<port>\d+)");
                if (m.Success) {
                    host = m.Groups["host"].Value == "0.0.0.0" ? "localhost" : m.Groups["host"].Value;
                    port = m.Groups["port"].Value;
                    break;
                }
            }
        }

        string db  = "postgres";
        string usr = "postgres";
        string pwd = "postgres";

        var conn = $"Host={host};Port={port};Database={db};Username={usr};Password={pwd};Pooling=true;";

        ServerMenuSettings.PostgresConnection = conn;

        Debug.Log($"Set PostgresConnection: {conn}");
    }

    public static void ComposeUp() {
        ComposeUpAsync();
    }

    private static async void ComposeUpAsync() {
        try {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Starting containers...";
            }

            var root     = GetRoot();
            var services = string.Join(" ", GetComposeServices());
            var cmd = string.IsNullOrEmpty(services)
                ? $"{GetComposeCmd()} {GetComposeFile(root)} up -d --build --wait"
                : $"{GetComposeCmd()} {GetComposeFile(root)} up -d --build --wait {services}";

            var envVars = new System.Collections.Generic.Dictionary<string, string> {
                ["DISABLE_LOADOUT_LOST"] = ServerMenuSettings.DisableLoadoutLost.ToString().ToLower()
            };

            var result = await Task.Run(() => Exec(cmd, root, 90000, envVars));

            var (code, so, se) = result;

            if (code == 0) {
                Debug.Log($"✓ Docker Compose: up -d --build --wait ok @ {root}{(string.IsNullOrEmpty(services) ? "" : $" [{services}]")}");
            }

            if (!string.IsNullOrEmpty(so)) {
                Debug.Log(so);
            }

            if (!string.IsNullOrEmpty(se)) {
                Debug.Log($"\n{se}");
            }

            RefreshServerStatus();
        }
        catch (Exception ex) {
            Debug.LogError($"Docker ComposeUp failed: {ex.Message}");
        }
        finally {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "";
            }
        }
    }

    public static void ComposePause() {
        ComposePauseAsync();
    }

    private static async void ComposePauseAsync() {
        try {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Pausing containers...";
            }

            var root     = GetRoot();
            var services = string.Join(" ", GetComposeServices());
            var cmd = string.IsNullOrEmpty(services)
                ? $"{GetComposeCmd()} {GetComposeFile(root)} pause"
                : $"{GetComposeCmd()} {GetComposeFile(root)} pause {services}";

            var result = await Task.Run(() => Exec(cmd, root));

            var (code, so, se) = result;

            if (code == 0) {
                Debug.Log($"✓ Docker Compose: pause ok @ {root}{(string.IsNullOrEmpty(services) ? "" : $" [{services}]")}");
            }

            if (!string.IsNullOrEmpty(so)) {
                Debug.Log(so);
            }

            if (!string.IsNullOrEmpty(se)) {
                Debug.Log($"\n{se}");
            }

            RefreshServerStatus();
        }
        catch (Exception ex) {
            Debug.LogError($"Docker ComposePause failed: {ex.Message}");
        }
        finally {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "";
            }
        }
    }

    public static void ComposeStop() {
        ComposeStopAsync();
    }

    private static async void ComposeStopAsync() {
        try {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Stopping containers...";
            }

            var root     = GetRoot();
            var services = string.Join(" ", GetComposeServices());
            var cmd = string.IsNullOrEmpty(services)
                ? $"{GetComposeCmd()} {GetComposeFile(root)} stop"
                : $"{GetComposeCmd()} {GetComposeFile(root)} stop {services}";

            var result = await Task.Run(() => Exec(cmd, root));

            var (code, so, se) = result;

            if (code == 0) {
                Debug.Log($"✓ Docker Compose: stop ok @ {root}{(string.IsNullOrEmpty(services) ? "" : $" [{services}]")}");
            }

            if (!string.IsNullOrEmpty(so)) {
                Debug.Log(so);
            }

            if (!string.IsNullOrEmpty(se)) {
                Debug.Log($"\n{se}");
            }

            RefreshServerStatus();
        }
        catch (Exception ex) {
            Debug.LogError($"Docker ComposeStop failed: {ex.Message}");
        }
        finally {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "";
            }
        }
    }

    public static void ComposeRemoveContainer() {
        ComposeRemoveContainerAsync();
    }

    private static async void ComposeRemoveContainerAsync() {
        try {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Removing containers...";
            }

            var root     = GetRoot();
            var services = string.Join(" ", GetComposeServices());
            var cmd = string.IsNullOrEmpty(services)
                ? $"{GetComposeCmd()} {GetComposeFile(root)} down -v"
                : $"{GetComposeCmd()} {GetComposeFile(root)} down -v {services}";

            var result = await Task.Run(() => Exec(cmd, root));

            var (code, so, se) = result;

            if (code == 0) {
                Debug.Log($"✓ Docker Compose: down -v ok @ {root}{(string.IsNullOrEmpty(services) ? "" : $" [{services}]")}");
            }

            if (!string.IsNullOrEmpty(so)) {
                Debug.Log(so);
            }

            if (!string.IsNullOrEmpty(se)) {
                Debug.Log($"\n{se}");
            }

            RefreshServerStatus();
        }
        catch (Exception ex) {
            Debug.LogError($"Docker ComposeRemoveContainer failed: {ex.Message}");
        }
        finally {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "";
            }
        }
    }

    public static void ComposeReload() {
        ComposeReloadAsync();
    }

    private static async void ComposeReloadAsync() {
        try {
            var root     = GetRoot();
            var services = string.Join(" ", GetComposeServices());

            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Stopping containers...";
            }

            var downCmd = string.IsNullOrEmpty(services)
                ? $"{GetComposeCmd()} {GetComposeFile(root)} down -v --remove-orphans"
                : $"{GetComposeCmd()} {GetComposeFile(root)} down -v --remove-orphans {services}";

            var downResult = await Task.Run(() => Exec(downCmd, root, 20000));

            var (downCode, downSo, downSe) = downResult;

            if (downCode == 0) {
                Debug.Log($"✓ Docker Compose: reload step 1/2 down -v --remove-orphans ok @ {root}{(string.IsNullOrEmpty(services) ? "" : $" [{services}]")}");
            }

            if (!string.IsNullOrEmpty(downSo)) {
                Debug.Log(downSo);
            }

            if (!string.IsNullOrEmpty(downSe)) {
                Debug.Log($"\n{downSe}");
            }

            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Building and starting containers...";
            }

            var upCmd = string.IsNullOrEmpty(services)
                ? $"{GetComposeCmd()} {GetComposeFile(root)} up -d --build --force-recreate --wait"
                : $"{GetComposeCmd()} {GetComposeFile(root)} up -d --build --force-recreate --wait {services}";

            var envVars = new System.Collections.Generic.Dictionary<string, string> {
                ["DISABLE_LOADOUT_LOST"] = ServerMenuSettings.DisableLoadoutLost.ToString().ToLower()
            };

            var upResult = await Task.Run(() => Exec(upCmd, root, 90000, envVars));

            var (upCode, upSo, upSe) = upResult;

            if (upCode == 0) {
                Debug.Log($"✓ Docker Compose: reload step 2/2 up -d --build --force-recreate --wait ok @ {root}{(string.IsNullOrEmpty(services) ? "" : $" [{services}]")}");
            }

            if (!string.IsNullOrEmpty(upSo)) {
                Debug.Log(upSo);
            }

            if (!string.IsNullOrEmpty(upSe)) {
                Debug.Log($"\n{upSe}");
            }

            RefreshServerStatus();
        }
        catch (Exception ex) {
            Debug.LogError($"Docker ComposeReload failed: {ex.Message}");
        }
        finally {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "";
            }
        }
    }

    public static void ComposeRebuildServer() {
        ComposeRebuildServerAsync();
    }

    private static async void ComposeRebuildServerAsync() {
        try {
            var root = GetRoot();

            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Stopping server...";
            }

            var stopCmd    = $"{GetComposeCmd()} {GetComposeFile(root)} rm -f -s escape-server";
            var stopResult = await Task.Run(() => Exec(stopCmd, root, 20000));

            var (stopCode, stopSo, stopSe) = stopResult;

            if (stopCode == 0) {
                Debug.Log($"✓ Docker Compose: rebuild step 1/5 rm escape-server ok @ {root}");
            }

            if (!string.IsNullOrEmpty(stopSo)) {
                Debug.Log(stopSo);
            }

            if (!string.IsNullOrEmpty(stopSe)) {
                Debug.Log($"\n{stopSe}");
            }

            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Removing old image...";
            }

            var rmiCmd = "docker rmi escape2-escape-server:latest";
            await Task.Run(() => Exec(rmiCmd, root, 10000));
            Debug.Log("✓ Docker: rebuild step 2/5 rmi escape2-escape-server:latest");

            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Clearing build cache...";
            }

            var pruneCmd = "docker builder prune -f";
            var pruneResult = await Task.Run(() => Exec(pruneCmd, root, 60000));

            if (pruneResult.Item1 == 0) {
                Debug.Log("✓ Docker: rebuild step 3/5 build cache cleared");
            }

            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Building server without cache...";
            }

            var buildCmd    = $"{GetComposeCmd()} {GetComposeFile(root)} build --no-cache escape-server";
            var buildEnv = new System.Collections.Generic.Dictionary<string, string> {
                ["DISABLE_LOADOUT_LOST"] = ServerMenuSettings.DisableLoadoutLost.ToString().ToLower()
            };
            var buildResult = await Task.Run(() => Exec(buildCmd, root, 90000, buildEnv));

            if (buildResult.Item1 == 0) {
                Debug.Log("✓ Docker Compose: rebuild step 4/5 build --no-cache escape-server ok");
            }

            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "Starting server...";
            }

            var upCmd    = $"{GetComposeCmd()} {GetComposeFile(root)} up -d --no-deps --wait escape-server";
            var upResult = await Task.Run(() => Exec(upCmd, root, 60000, buildEnv));

            var (code, so, se) = upResult;

            if (code == 0) {
                Debug.Log($"✓ Docker Compose: rebuild step 5/5 up -d escape-server ok @ {root}");
            }

            if (!string.IsNullOrEmpty(so)) {
                Debug.Log(so);
            }

            if (!string.IsNullOrEmpty(se)) {
                Debug.Log($"\n{se}");
            }

            RefreshServerStatus();
        }
        catch (Exception ex) {
            Debug.LogError($"Docker ComposeRebuildServer failed: {ex.Message}");
        }
        finally {
            lock (LockObj) {
                cachedServerInfo.CurrentOperation = "";
            }
        }
    }

    /// <summary>
    /// Быстрый неблокирующий метод получения информации о сервере из кеша.
    /// Автоматически запускает проверку в фоне если она не идёт.
    /// </summary>
    public static ServerInfo GetServerInfo() {
        lock (LockObj) {
            if (!cachedServerInfo.IsChecking) {
                cachedServerInfo.IsChecking = true;
                StartBackgroundStatusCheck();
            }

            return cachedServerInfo;
        }
    }

    /// <summary>
    /// Принудительно обновить статус сервера (для кнопок Start/Stop)
    /// </summary>
    public static void RefreshServerStatus() {
        lock (LockObj) {
            statusCheckCts?.Cancel();
            cachedServerInfo.IsChecking = true;
            StartBackgroundStatusCheck();
        }
    }

    private static void StartBackgroundStatusCheck() {
        statusCheckCts?.Cancel();
        statusCheckCts = new CancellationTokenSource();
        var token = statusCheckCts.Token;

        Task.Run(async () => {
            try {
                var info = await CheckServerStatusAsync(token);
                lock (LockObj) {
                    cachedServerInfo.Status     = info.Status;
                    cachedServerInfo.Version    = info.Version;
                    cachedServerInfo.IsChecking = false;
                }

                // Обновляем UI в главном потоке
                EditorApplication.delayCall += () => EditorApplication.QueuePlayerLoopUpdate();
            }
            catch (OperationCanceledException) {
                // Нормальная отмена - ничего не делаем
            }
            catch (Exception ex) {
                Debug.LogWarning($"Server status check failed: {ex.Message}");
                lock (LockObj) {
                    cachedServerInfo.Status     = ServerStatus.Unknown;
                    cachedServerInfo.IsChecking = false;
                }
            }
        }, token);
    }

    private static async Task<ServerInfo> CheckServerStatusAsync(CancellationToken ct) {
        var info = new ServerInfo();

        try {
            var healthResponse = await HttpClient.GetAsync("http://localhost:5024/", ct);

            if (healthResponse.IsSuccessStatusCode) {
                var content = await healthResponse.Content.ReadAsStringAsync();
                if (content.Contains("TDM")) {
                    info.Status = ServerStatus.Healthy;

                    try {
                        var versionText = await HttpClient.GetStringAsync("http://localhost:5024/version/");
                        info.Version = versionText?.Trim() ?? "";
                    }
                    catch {
                        info.Version = "";
                    }

                    return info;
                }
            }

            info.Status = ServerStatus.Unhealthy;
            return info;
        }
        catch (System.Net.Http.HttpRequestException) {
            return await Task.Run(() => CheckDockerStatus(), ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested) {
            return await Task.Run(() => CheckDockerStatus(), ct);
        }
    }

    private static ServerInfo CheckDockerStatus() {
        var info         = new ServerInfo();
        var dockerStatus = GetServiceStatus("escape-server");

        info.Status = dockerStatus switch {
            "healthy" => ServerStatus.Healthy,
            "unhealthy" => ServerStatus.Unhealthy,
            "running" => ServerStatus.Running,
            _ => ServerStatus.Stopped
        };

        return info;
    }

    public static string GetServiceStatus(string serviceName = "escape-server") {
        var root = GetRoot();
        var cmd  = $"{GetComposeCmd()} {GetComposeFile(root)} ps {serviceName} --format json";
        var (code, so, _) = Exec(cmd, root, 5000);

        if (code != 0 || string.IsNullOrEmpty(so)) {
            return "stopped";
        }

        if (so.Contains("\"Health\":\"healthy\"")) {
            return "healthy";
        }

        if (so.Contains("\"Health\":\"unhealthy\"")) {
            return "unhealthy";
        }

        if (so.Contains("\"State\":\"running\"")) {
            return "running";
        }

        return "stopped";
    }

    public static bool IsDozzleRunning() {
        try {
            var localIp = GetLocalIPAddress();
            var request = System.Net.WebRequest.Create($"http://{localIp}:14088");
            request.Timeout = 300;
            request.Method = "HEAD";
            using var response = request.GetResponse();
            return true;
        }
        catch {
            return false;
        }
    }

    public static string GetLocalIPAddress() {
        if (!string.IsNullOrEmpty(cachedLocalIP)) {
            return cachedLocalIP;
        }

        try {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    cachedLocalIP = ip.ToString();
                    return cachedLocalIP;
                }
            }
        }
        catch {
            // ignored
        }
        cachedLocalIP = "127.0.0.1";
        return cachedLocalIP;
    }

    public static void StartDozzle() {
        StartDozzleAsync();
    }

    private static async void StartDozzleAsync() {
        try {
            var root = GetRoot();
            var cmd = $"{GetComposeCmd()} -f docker-compose.dozzle.yml up -d";
            var result = await Task.Run(() => Exec(cmd, root, 10000));

            var (code, so, se) = result;

            if (code == 0) {
                Debug.Log("✓ Dozzle started");
            } else if (!string.IsNullOrEmpty(se)) {
                Debug.LogError($"Failed to start Dozzle: {se}");
            }
        }
        catch (Exception ex) {
            Debug.LogError($"Failed to start Dozzle: {ex.Message}");
        }
    }

    private static string[] GetComposeServices() {
        var root        = GetRoot();
        var composePath = ResolveComposePath(root);

        if (composePath == null) {
            Debug.LogError("docker-compose.yml not found at repo root");
            return Array.Empty<string>();
        }

        var (code, so, se) = Exec($"{GetComposeCmd()} {GetComposeFile(root)} config --services", root, waitMs: 15000);
        if (code == 0) {
            var names = so.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToArray();
            return names;
        }

        Debug.LogWarning(string.IsNullOrEmpty(se) ? "Failed to list compose services; running command without explicit services." : se);
        return Array.Empty<string>();
    }

    private static string GetRoot() {
        var start = new DirectoryInfo(Application.dataPath);
        var dir   = start;
        for (int i = 0; i < 6 && dir != null; i++) {
            var composeLocalhost = Path.Combine(dir.FullName, "docker-compose.localhost.yml");
            var composeYml       = Path.Combine(dir.FullName, "docker-compose.yml");
            var composeYaml      = Path.Combine(dir.FullName, "docker-compose.yaml");

            if (File.Exists(composeLocalhost) || File.Exists(composeYml) || File.Exists(composeYaml)) {
                return dir.FullName;
            }

            var composeNewYml  = Path.Combine(dir.FullName, "compose.yml");
            var composeNewYaml = Path.Combine(dir.FullName, "compose.yaml");

            if (File.Exists(composeNewYml) || File.Exists(composeNewYaml)) {
                return dir.FullName;
            }

            var git = Path.Combine(dir.FullName, ".git");

            if (Directory.Exists(git)) {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return new DirectoryInfo(Application.dataPath).Parent?.Parent?.Parent?.FullName ?? Application.dataPath;
    }

    private static string ResolveComposePath(string root) {
        var candidates = new[] {
            Path.Combine(root, "docker-compose.localhost.yml"),
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string GetComposeCmd() {
        if (!string.IsNullOrEmpty(composeCmd)) {
            return composeCmd;
        }

        if (CheckCmd("docker compose version")) {
            composeCmd = "docker compose";
            return composeCmd;
        }

        if (CheckCmd("docker-compose version")) {
            composeCmd = "docker-compose";
            return composeCmd;
        }

        Debug.LogError("Neither `docker compose` nor `docker-compose` is available in PATH");

        composeCmd = "docker compose";

        return composeCmd;
    }

    private static string GetComposeFile(string root) {
        var composePath = ResolveComposePath(root);
        if (composePath == null) {
            return "";
        }

        var fileName = Path.GetFileName(composePath);
        return $"-f {fileName}";
    }

    private static bool CheckCmd(string command) {
        var (code, _, _) = Exec(command, GetRoot(), waitMs: 4000);
        return code == 0;
    }

    private static (int exitCode, string stdout, string stderr) Exec(string command, string workingDirectory, int waitMs = 600000, System.Collections.Generic.Dictionary<string, string> envVars = null) {
        try {
#if UNITY_EDITOR_WIN
            var shellCmd = "cmd.exe";
            var shellArgs = "/c " + command;
#else
            var shellCmd  = "/bin/bash";
            var shellArgs = "-c \"" + command.Replace("\"", "\\\"") + "\"";
#endif
            var psi = new ProcessStartInfo(shellCmd, shellArgs) {
                WorkingDirectory       = workingDirectory,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };

#if UNITY_EDITOR_LINUX
            var home = System.Environment.GetEnvironmentVariable("HOME");
            var path = $"/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin";
            if (!string.IsNullOrEmpty(home)) {
                path                             = $"{home}/.local/bin:{path}";
                psi.EnvironmentVariables["PATH"] = path;
            }
#endif

            if (envVars != null) {
                foreach (var kvp in envVars) {
                    psi.EnvironmentVariables[kvp.Key] = kvp.Value;
                }
            }

            using var p = new Process { StartInfo = psi };
            p.Start();

            if (!p.WaitForExit(waitMs)) {
                try {
                    p.Kill();
                }
                catch {
                    // ignored
                }

                return (-1, "", "Process timed out");
            }

            var so = p.StandardOutput.ReadToEnd();
            var se = p.StandardError.ReadToEnd();

            return (p.ExitCode, so, se);
        }
        catch (Exception e) {
            Debug.LogException(e);

            return (-1, "", e.Message);
        }
    }
}