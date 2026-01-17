namespace Scripts.Editor {
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using Newtonsoft.Json;

    public class CursorIntegration {
        private static string IntegrationDir => Path.Combine(Application.dataPath, "../../.cursor-integration");
        
        [InitializeOnLoadMethod]
        static void Initialize() {
            if (!Directory.Exists(IntegrationDir)) {
                Directory.CreateDirectory(IntegrationDir);
            }
        }

        [MenuItem("Tools/Export Error Logs (No Stack Trace)")]
        public static void ExportErrorLogsNoStackMenu() {
            ExportErrorLogs(includeStackTrace: false, includeFileLogs: false);
        }

        [MenuItem("Tools/Export Error Logs (With Stack Trace)")]
        public static void ExportErrorLogsWithStackMenu() {
            ExportErrorLogs(includeStackTrace: true, includeFileLogs: false);
        }

        [MenuItem("Tools/Export Error Logs (Full with File Logs)")]
        public static void ExportAllLogsMenu() {
            ExportErrorLogs(includeStackTrace: true, includeFileLogs: true);
        }

        private static void ExportErrorLogs(bool includeStackTrace = true, bool includeFileLogs = false) {
            var logs = ParseUnityLogFile();
            var errors = logs.Where(l => l.type == "Error" || l.type == "Exception").ToList();
            var warnings = logs.Where(l => l.type == "Warning").ToList();

            var data = new {
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                errorCount = errors.Count,
                warningCount = warnings.Count,
                includeStackTrace = includeStackTrace,
                errors = errors.Select(e => new {
                    type = e.type,
                    message = e.message,
                    stackTrace = includeStackTrace ? e.stackTrace : null,
                    timestamp = e.timestamp
                }).ToArray(),
                warnings = warnings.Select(w => new {
                    type = w.type,
                    message = w.message,
                    stackTrace = includeStackTrace ? w.stackTrace : null,
                    timestamp = w.timestamp
                }).ToArray(),
                projectInfo = new {
                    projectName = Application.productName,
                    unityVersion = Application.unityVersion,
                    isPlaying = Application.isPlaying,
                    activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                },
                fileLogs = includeFileLogs ? GetFileLogs() : null
            };
            
            var jsonPath = Path.Combine(IntegrationDir, "unity_error_logs.json");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(jsonPath, json);

            var yamlPath = Path.Combine(IntegrationDir, "unity_error_logs.yaml");
            var yaml = ConvertToYaml(data);
            File.WriteAllText(yamlPath, yaml);
        }

        private static List<LogEntry> ParseUnityLogFile() {
            var logFilePath = Path.Combine(Application.dataPath, "../Logs/Editor.log");
            if (!File.Exists(logFilePath)) {
                logFilePath = Path.Combine(Application.dataPath, "../debug.log");
            }
            
            if (!File.Exists(logFilePath)) {
                return new List<LogEntry>();
            }

            var logs = new List<LogEntry>();
            var lines = File.ReadAllLines(logFilePath);
            
            LogEntry currentLog = null;
            var stackTraceLines = new List<string>();
            
            for (int i = 0; i < lines.Length; i++) {
                var line = lines[i];
                
                var errorMatch = Regex.Match(line, @"(Error|Exception|Warning):\s+(.+)$");
                if (errorMatch.Success) {
                    if (currentLog != null) {
                        currentLog.stackTrace = string.Join("\n", stackTraceLines);
                        logs.Add(currentLog);
                        stackTraceLines.Clear();
                    }
                    
                    currentLog = new LogEntry {
                        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        type = errorMatch.Groups[1].Value,
                        message = errorMatch.Groups[2].Value,
                        stackTrace = ""
                    };
                    continue;
                }
                
                if (currentLog != null && (line.StartsWith("  at ") || line.StartsWith("   at ") || line.Contains("(at "))) {
                    stackTraceLines.Add(line.Trim());
                }
                
                if (currentLog != null && line.Trim() == "") {
                    currentLog.stackTrace = string.Join("\n", stackTraceLines);
                    logs.Add(currentLog);
                    currentLog = null;
                    stackTraceLines.Clear();
                }
            }
            
            if (currentLog != null) {
                currentLog.stackTrace = string.Join("\n", stackTraceLines);
                logs.Add(currentLog);
            }
            
            return logs.TakeLast(200).ToList();
        }

        private static object GetFileLogs() {
            var logsPath = Path.Combine(Application.dataPath, "../Logs");
            if (!Directory.Exists(logsPath)) return new object[0];
            
            return Directory.GetFiles(logsPath, "*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .Take(3)
                .Select(file => new {
                    name = Path.GetFileName(file),
                    lastWrite = File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm:ss"),
                    sizeMB = new FileInfo(file).Length / (1024.0 * 1024.0),
                    preview = GetFilePreview(file, 50)
                })
                .ToArray();
        }

        private static string GetFilePreview(string filePath, int lines) {
            try {
                var allLines = File.ReadAllLines(filePath);
                var recentLines = allLines.Skip(System.Math.Max(0, allLines.Length - lines)).ToArray();
                return string.Join("\n", recentLines);
            } catch {
                return "Failed to read file";
            }
        }

        private static string ConvertToYaml(object data) {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return ToYamlString(dict, 0);
        }

        private static string ToYamlString(object obj, int indent) {
            var indentStr = new string(' ', indent * 2);
            
            if (obj is Dictionary<string, object> dict) {
                var result = "";
                foreach (var kvp in dict) {
                    result += $"{indentStr}{kvp.Key}:\n";
                    result += ToYamlString(kvp.Value, indent + 1);
                }
                return result;
            } else if (obj is Newtonsoft.Json.Linq.JArray arr) {
                var result = "";
                foreach (var item in arr) {
                    result += $"{indentStr}- ";
                    var itemStr = ToYamlString(item, indent + 1).TrimStart();
                    result += itemStr;
                }
                return result;
            } else if (obj is Newtonsoft.Json.Linq.JObject jobj) {
                var result = "\n";
                foreach (var prop in jobj.Properties()) {
                    result += $"{indentStr}{prop.Name}: ";
                    var value = ToYamlString(prop.Value, indent + 1).TrimStart();
                    result += value;
                }
                return result;
            } else {
                return $"{obj}\n";
            }
        }

        private class LogEntry {
            public string message;
            public string stackTrace;
            public string type;
            public string timestamp;
        }
    }
}
