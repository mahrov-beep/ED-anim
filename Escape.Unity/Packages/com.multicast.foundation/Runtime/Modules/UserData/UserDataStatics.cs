namespace Multicast.Modules.UserData {
    using System.IO;
    using UnityEngine;

    public static class UserDataStatics {
        public static string UserDataFolder => $"{Application.persistentDataPath}/ud_data_v2";

        public static string UserDataFilePath       => $"{UserDataFolder}/ud_data.dat";
        public static string UserDataBackupFilePath => $"{UserDataFolder}/ud_data.bak";
        public static string UserDataTempFilePath   => $"{UserDataFolder}/ud_data.tmp";

        public static void CreateUserDataDirectory() {
            if (!Directory.Exists(UserDataStatics.UserDataFolder)) {
                Directory.CreateDirectory(UserDataStatics.UserDataFolder);
            }
        }

        public static void DeleteAllData() {
            File.Delete(UserDataStatics.UserDataFilePath);
            File.Delete(UserDataStatics.UserDataTempFilePath);
            File.Delete(UserDataStatics.UserDataBackupFilePath);
        }
    }
}