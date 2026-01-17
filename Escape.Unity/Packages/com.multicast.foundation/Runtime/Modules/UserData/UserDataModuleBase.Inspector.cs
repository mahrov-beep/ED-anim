#if UNITY_EDITOR

namespace Multicast.Modules.UserData {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Multicast.UserData;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    public partial class UserDataModuleBase<TUserData> {
        [ShowInInspector, PropertyOrder(50)]
        [PropertySpace(20)]
        [InlineButton(nameof(RevealUserDataInFinder), "Reveal In Finder"), EnableGUI]
        private static string UserDataFolder => UserDataStatics.UserDataFolder;

        [ShowInInspector, PropertyOrder(100)]
        [PropertySpace(20), Title("User Data Debugger")]
        [EnableGUI, HideLabel, HideInEditorMode, Optional, HideReferenceObjectPicker]
        private UdRoot<TUserData> UserDataInspector => this.userData;

        [ShowInInspector, PropertyOrder(120)]
        [HideInPlayMode]
        private static List<SavedUserDataInfo> SavedUserData { get; set; }

        [Button]
        [DisableInEditorMode]
        private void SaveUserDataAsJson() {
            var json = UdRoot.SerializeToJson(this.userData);
            var path = UserDataStatics.UserDataFilePath + ".debug.json";
            File.WriteAllText(path, json);
            EditorUtility.RevealInFinder(path);
        }

        [OnInspectorInit]
        private void RefreshSavedUserDataList() {
            var customSaveFilePrefix = UserDataStatics.UserDataFilePath + '_';

            if (Directory.Exists(UserDataFolder)) {
                SavedUserData = Directory.EnumerateFiles(UserDataFolder)
                    .Select(path => path.Replace('\\', '/'))
                    .Where(path => path.StartsWith(customSaveFilePrefix))
                    .Select(path => new SavedUserDataInfo {
                        path        = path,
                        displayName = path.Substring(customSaveFilePrefix.Length),
                    })
                    .ToList();
            }
            else {
                SavedUserData = new List<SavedUserDataInfo>();
            }
        }

        [Button, PropertyOrder(100)]
        [HideInPlayMode, PropertySpace]
        private void SaveNamedUserData(string displayName) {
            File.Copy(UserDataStatics.UserDataFilePath, UserDataStatics.UserDataFilePath + "_" + displayName, true);

            this.RefreshSavedUserDataList();
        }

        private static void RevealUserDataInFinder() {
            if (!Directory.Exists(UserDataFolder)) {
                Directory.CreateDirectory(UserDataFolder);
            }

            EditorUtility.RevealInFinder(UserDataFolder);
        }

        [Serializable, InlineProperty, HideReferenceObjectPicker]
        private class SavedUserDataInfo {
            [HideInInspector]
            public string path;

            [ShowInInspector, HideLabel, InlineButton(nameof(Apply))]
            public string displayName;

            public void Apply() {
                File.Copy(this.path, UserDataStatics.UserDataFilePath, true);
            }
        }
    }
}

#endif