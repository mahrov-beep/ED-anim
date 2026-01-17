namespace Multicast.Modules.UserData {
    using System.IO;
    using System.Linq;
    using Collections;
    using Cysharp.Threading.Tasks;
    using Routes;
    using UI.Widgets;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    internal class UserDataUI {
        public static bool ShowUserDataSelectorOnce {
            get => PlayerPrefs.GetInt("Multicast_ShowUserDataSelectorOnce", 1) != 0;
            set => PlayerPrefs.SetInt("Multicast_ShowUserDataSelectorOnce", value ? 1 : 0);
        }

        public async UniTask ShowUserDataSelectorUI() {
            var lastSelectedPath = Atom.Value(default(string));

            var cache = new AddressableCache<TextAsset>();

            await cache.Preload("UserDataPresets");

            var paths = cache.EnumerateCachedPaths().ToList();

            await App.Current.GetNavigator(AppNavigatorType.System).Push(new SlideDownRoute(
                new RouteSettings("user_data_selector", RouteModalType.Popup),
                (context, _, _) => new DebugListWidget("User Data") {
                    Items = {
                        BuildNoDataItem(),
                        paths.Select(BuildItem),
                    },
                }
            )).PopTask;

            Widget BuildNoDataItem() {
                lastSelectedPath.Get();

                return new DebugListItemWidget {
                    PrimaryText   = "Clean install (no data)",
                    SecondaryText = !File.Exists(UserDataStatics.UserDataFilePath) ? "+" : "",
                    OnClick = () => {
                        UserDataStatics.CreateUserDataDirectory();
                        UserDataStatics.DeleteAllData();
                        lastSelectedPath.Value = string.Empty;
                    },
                };
            }

            Widget BuildItem(string path) {
                lastSelectedPath.Get();
                
                return new DebugListItemWidget {
                    PrimaryText   = path,
                    SecondaryText = lastSelectedPath.Value == path ? "+" : "",

                    OnClick = () => {
                        UserDataStatics.CreateUserDataDirectory();
                        UserDataStatics.DeleteAllData();

                        using (var fileStream = new FileStream(UserDataStatics.UserDataFilePath, FileMode.Create, FileAccess.Write)) {
                            fileStream.Write(cache.Get(path).bytes);
                        }

                        lastSelectedPath.Value = path;
                    },
                };
            }
        }
    }
}