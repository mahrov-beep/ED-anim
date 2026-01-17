namespace Multicast.Modules.UserData {
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using Multicast.Analytics;
    using Multicast.UserData;
    using UnityEngine;
    using UnityEngine.Pool;
    using Debug = UnityEngine.Debug;

    internal class UserDataService<TUserData> : IUserDataService where TUserData : UdObject {
        private readonly Stopwatch stopwatch = new Stopwatch();

        private readonly IAnalytics analytics;

        private readonly Func<UdArgs, TUserData> factory;

        private UdRoot<TUserData> userData;

        public UdRoot<TUserData> UserData => this.userData;

        UdRoot IUserDataService.Root => this.userData;

        public UserDataService(IAnalytics analytics, Func<UdArgs, TUserData> factory) {
            this.analytics = analytics;
            this.factory   = factory;
        }

        public async UniTask LoadOrCreateUserData() {
            await this.CreateUserDataDirectoryWithRetry();

            var sources = new (string name, string path)[] {
                ("Main", UserDataStatics.UserDataFilePath),
                ("Temp", UserDataStatics.UserDataTempFilePath),
                ("Backup", UserDataStatics.UserDataBackupFilePath),
            };

            foreach (var (name, path) in sources) {
                if (!this.TryLoadUserData(path, out this.userData, out var ex)) {
                    if (ex != null) {
                        this.analytics.Send(new FailedToLoadUserDataAnalyticsEvent(name, ex.Message));
                    }

                    continue;
                }

                this.SaveUserData();
                this.CreateBackupForUserData();
                break;
            }

            this.userData ??= UdRoot.Create(this.factory);
        }

        public void SaveUserData() {
            if (this.userData.TryGetActiveTransaction(out var transactionId)) {
                Debug.LogError($"UserData cannot be saved because transaction is opened: {transactionId}");
                return;
            }

            try {
                UserDataStatics.CreateUserDataDirectory();

                using (UdRoot.ArrayBufferWriterPool.Get(out var stream)) {
                    this.stopwatch.Restart();

                    UdRoot.Serialize(this.userData, stream);

                    this.stopwatch.Stop();

                    if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                        Debug.Log($"Serialize UserData in {this.stopwatch.ElapsedMilliseconds} ms");
                    }

                    this.stopwatch.Restart();

                    using (var fileStream = new FileStream(UserDataStatics.UserDataTempFilePath, FileMode.Create, FileAccess.Write)) {
                        fileStream.Write(stream.WrittenSpan);
                    }

                    File.Delete(UserDataStatics.UserDataFilePath);
                    File.Move(UserDataStatics.UserDataTempFilePath, UserDataStatics.UserDataFilePath);

                    this.stopwatch.Stop();

                    if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                        Debug.Log($"Save UserData in {this.stopwatch.ElapsedMilliseconds} ms");
                    }
                }
            }
            catch (IOException ex) {
                this.analytics.Send(new FailedToSaveUserDataAnalyticsEvent(ex.Message));
            }
        }

        private bool TryLoadUserData(string path, out UdRoot<TUserData> root, out Exception exception) {
            try {
                if (!File.Exists(path)) {
                    root      = null;
                    exception = null;
                    return false;
                }

                this.stopwatch.Restart();

                var bytes = File.ReadAllBytes(path);

                this.stopwatch.Stop();

                if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                    Debug.Log($"Load UserData in {this.stopwatch.ElapsedMilliseconds} ms");
                }

                this.stopwatch.Restart();

                root      = UdRoot.FromMemory(this.factory, bytes.AsMemory());
                exception = null;

                this.stopwatch.Stop();

                if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                    Debug.Log($"Deserialize UserData in {this.stopwatch.ElapsedMilliseconds} ms");
                }

                return true;
            }
            catch (Exception ex) {
                root      = null;
                exception = ex;
                return false;
            }
        }

        private void CreateBackupForUserData() {
            try {
                if (!File.Exists(UserDataStatics.UserDataFilePath)) {
                    Debug.LogError("Failed to create UserData backup: file not exists");
                    return;
                }

                File.Delete(UserDataStatics.UserDataBackupFilePath);
                File.Copy(UserDataStatics.UserDataFilePath, UserDataStatics.UserDataBackupFilePath, true);
            }
            catch (IOException) {
                Debug.LogError("Failed to create UserData backup: IOException");
            }
        }

        private async Task CreateUserDataDirectoryWithRetry() {
            var created = false;
            var tries   = 0;
            do {
                ++tries;

                try {
                    UserDataStatics.CreateUserDataDirectory();
                    created = true;
                }
                catch (IOException) {
                    if (tries > 5) {
                        throw;
                    }

                    Debug.LogError("Failed to create UserData folder");
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                }
            } while (!created);
        }
    }
}