using System;
using Game.Shared;
using Game.Shared.Defs;
using Game.UI;
using Multicast;
using Multicast.Collections;
using Multicast.DirtyDataEditor;
using Multicast.FeatureToggles;
using Quantum;
using Sirenix.OdinInspector;

public class GameDefValidationAsset : DirtyDataValidationAsset {
    public override object Parse(IEnumerableCache<TextAsset> cache) {
        var oldDebug = MulticastLog.DebugLogCallback;
        try {
            MulticastLog.DebugLogCallback = null;

            return GameDef.FromCache(cache);
        }
        finally {
            MulticastLog.DebugLogCallback = oldDebug;
        }
    }

    public override void Validate(object def, SelfValidationResult result) {
        base.Validate(def, result);

        var gameDef = (GameDef)def;

        ValidateFields<FeatureToggleDef, FeatureToggleName>(result, typeof(SharedConstants.Game.FeatureToggles), gameDef.FeatureToggles, it => it.Name);
        
        ValidateFields(result, typeof(SharedConstants.Game.Currencies), gameDef.Currencies);
        ValidateFields(result, typeof(SharedConstants.Game.Threshers), gameDef.Threshers);
        ValidateFields(result, typeof(SharedConstants.Game.Gunsmiths), gameDef.Gunsmiths);
        ValidateFields(result, typeof(SharedConstants.Game.GunsmithLoadouts), gameDef.GunsmithLoadouts);
        ValidateFields(result, typeof(SharedConstants.Game.Features), gameDef.Features);
        ValidateFields(result, typeof(SharedConstants.Game.GameModes), gameDef.GameModes);
        ValidateFields(result, typeof(SharedConstants.Game.Quests), gameDef.Quests);
        ValidateFields(result, typeof(SharedConstants.Game.QuestCounterTasks), gameDef.QuestCounterTasks);
        ValidateFields(result, typeof(SharedConstants.Game.Items), gameDef.Items);
        ValidateFields(result, typeof(SharedConstants.Game.Tutorials), gameDef.Tutorials);
        ValidateFields(result, typeof(SharedConstants.Game.CoinFarms), gameDef.CoinFarms);

        ValidateQuantumAssets<ItemDef, ItemAsset>(result, gameDef.Items, it => ItemAssetCreationData.GetItemAssetPath(it.key), static (result, def, asset) => {
            if (def.Type != asset.ItemType && def.Type != ItemTypes.Invalid) {
                result.AddError($"ItemType mismatch for def and asset in '{def.key}': dde={def.Type}, asset={asset.ItemType}");
            }

            if (def.Weight.RawValue <= 0) {
                result.AddError("Item weight must be greater than 0");
            }
        });
        ValidateQuantumAssets<GameModeDef, GameModeAsset>(result, gameDef.GameModes, it => "QuantumUser/Resources/Configs/GameModes/" + it.key, static (result, def, asset) => {
            if (def.gameRule != asset.rule) {
                result.AddError($"GameMode rule mismatch for def and asset in '{def.key}'");
            }
        });

        foreach (var entry in QuantumUnityDB.Global.Entries) {
            if (entry.Path.StartsWith("QuantumUser/Resources/Configs/Items/") &&
                QuantumUnityDB.Global.HasRequiredAsset<ItemAsset>(entry.Path, out _)) {
                var itemAsset = LoadQuantumAsset<ItemAsset>(entry.Path);

                if (!gameDef.Items.TryGet(itemAsset.ItemKey, out _)) {
                    result.AddError($"No item configured in DDE for QuantumAsset: itemKey={itemAsset.ItemKey}");
                }
            }

            if (entry.Path.StartsWith("QuantumUser/Resources/Configs/GameModes/") &&
                QuantumUnityDB.Global.HasRequiredAsset<GameModeAsset>(entry.Path, out _)) {
                var gameModeAsset = LoadQuantumAsset<GameModeAsset>(entry.Path);

                if (gameModeAsset.rule is GameRules.MainMenuStorage or GameRules.MainMenuGameResults) {
                    continue;
                }

                if (!gameDef.GameModes.TryGet(gameModeAsset.gameModeKey, out _)) {
                    result.AddError($"No gameMode configured in DDE for QuantumAsset: gameModeKey={gameModeAsset.gameModeKey}");
                }
            }
        }
    }

    private static void ValidateQuantumAssets<TDef, TAsset>(SelfValidationResult result, LookupCollection<TDef> defs,
        Func<TDef, string> selector,
        Action<SelfValidationResult, TDef, TAsset> validation = null)
        where TDef : Def
        where TAsset : AssetObject {
        foreach (var (key, def) in defs) {
            var path = selector(def) ?? string.Empty;
            if (!QuantumUnityDB.Global.HasRequiredAsset<TAsset>(path, out var errorMessage)) {
                result.AddError($"DDE: QuantumAsset '{path}' for '{typeof(TDef).Name}/{key}' not exist: {errorMessage}");
                continue;
            }

            if (validation != null) {
                var asset = LoadQuantumAsset<TAsset>(path);
                validation.Invoke(result, def, asset);
            }
        }
    }

    private static TAsset LoadQuantumAsset<TAsset>(string path) where TAsset : AssetObject {
        return
#if UNITY_EDITOR
            UnityEngine.Application.isEditor
                ? QuantumUnityDB.Global.GetAssetEditorInstance(new AssetRef<TAsset>(QuantumUnityDB.Global.GetAssetGuid(path)))
                :
#endif
                QuantumUnityDB.Global.GetRequiredAsset<TAsset>(path);
    }
}