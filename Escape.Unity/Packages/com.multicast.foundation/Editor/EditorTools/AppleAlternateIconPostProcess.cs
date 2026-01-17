using System.IO;
using System.Linq;
using Multicast.Tools.AppleAlternateIconGen;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;

namespace Multicast.EditorTools {

    public class AppleAlternateIconPostProcess {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            if (target != BuildTarget.iOS) {
                return;
            }

            var settingsList = AssetDatabase.FindAssets("t: " + typeof(AppleAlternateIcons).FullName)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AppleAlternateIcons>)
                .ToList();

            if (settingsList.Count == 0) {
                return;
            }

            var settings = settingsList[0];

            UpdatePbxProject(pathToBuiltProject, settings);
            CreateAppIconSet(pathToBuiltProject, settings);
        }

        private static void UpdatePbxProject(string pathToBuiltProject, AppleAlternateIcons settings) {
            var pbxProjectPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj", "project.pbxproj");
            var pbxProject     = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            var targetGuid = pbxProject.GetUnityMainTargetGuid();
            {
                pbxProject.SetBuildProperty(targetGuid, "ASSETCATALOG_COMPILER_INCLUDE_ALL_APPICON_ASSETS", "YES");
            }

            pbxProject.WriteToFile(pbxProjectPath);
        }

        private static void CreateAppIconSet(string pathToBuiltProject, AppleAlternateIcons settings) {
            var paths = settings.GetIncludedAlternateIconPaths();

            Debug.Log($"[AppleAlternateIconPostProcess] Create {paths.Length} app icons");

            foreach (var alternateIconPath in paths) {
                Debug.Log($"[AppleAlternateIconPostProcess] Add {alternateIconPath}");

                var iconName         = Path.GetFileNameWithoutExtension(alternateIconPath) ?? "";
                var iconSetDirectory = Path.Combine(pathToBuiltProject, "Unity-iPhone", "Images.xcassets", $"{iconName}.appiconset");

                if (!Directory.Exists(iconSetDirectory)) {
                    Directory.CreateDirectory(iconSetDirectory);
                }

                foreach (var filePath in Directory.GetFiles(alternateIconPath, "*.*", SearchOption.TopDirectoryOnly)) {
                    File.Copy(filePath, filePath.Replace(alternateIconPath, iconSetDirectory), true);
                }
            }
        }
    }
}
#endif