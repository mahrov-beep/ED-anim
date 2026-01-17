namespace Game.UI.Modules.Core {
    using Game.Shared;
    using Multicast;
    using Multicast.Install;
    using Multicast.Modules.GameDef;
    using UnityEngine;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class GameDefModule : GameDefBaseModule<GameDef> {
        protected override GameDef CreateDef(IEnumerableCache<Multicast.TextAsset> cache) => GameDef.FromCache(cache);
    }
}