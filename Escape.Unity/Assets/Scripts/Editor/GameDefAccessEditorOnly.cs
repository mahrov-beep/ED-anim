using Game.Domain;
using Game.Shared;
using Multicast;
using UnityEditor;

internal class GameDefAccessEditorOnly : AssetPostprocessor {
    private static GameDef def;

    [InitializeOnLoadMethod]
    private static void Setup() {
        CoreConstants.GameDefAccessEditorOnly = () => {
            if (def != null) {
                return def;
            }

            return def = GameDef.FromCache(EditorAddressablesCache<UnityEngine.TextAsset>.Instance.Select(it => new Multicast.TextAsset(it.text)));
        };
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
        foreach (var str in importedAssets) {
            if (str.EndsWith("Game Def Validation Asset.asset")) {
                def = null;
            }
        }
    }
}