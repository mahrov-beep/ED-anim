namespace Game.Domain.GameModes {
    using System;
    using System.Collections.Generic;
    using global::System;
    using global::System.Linq;
    using Multicast;
    using Multicast.Collections;
    using Quantum;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using UniMob;
    using UnityEngine;
    using UserData;

    public class GameModesModel : KeyedSingleInstanceModel<GameModeDef, SdGameMode, GameModeModel> {
        private readonly SdGameModesRepo data;

        public GameModesModel(Lifetime lifetime, LookupCollection<GameModeDef> defs, SdUserProfile sdUserProfile)
            : base(lifetime, defs, sdUserProfile.GameModes.Lookup) {
            this.data = sdUserProfile.GameModes;

            this.AutoConfigureData = true;
        }

        [Atom] public List<GameModeModel> VisibleGameModes => this.Values.Where(it => it.Visible).ToList();

        public GameModeModel SelectedGameMode => this.TryGet(this.data.SelectedGameMode.Value, out var selected) && selected.Visible
            ? selected
            : this.VisibleGameModes.First();
    }

    public class GameModeModel : Model<GameModeDef, SdGameMode> {
        public GameModeModel(Lifetime lifetime, GameModeDef def, SdGameMode data) : base(lifetime, def, data) {
        }

        public bool Visible => this.Def.visible;

        [Atom] public GameModeAsset ModeQuantumAsset => QuantumUnityDB.Global.GetRequiredAsset<GameModeAsset>(
            "QuantumUser/Resources/Configs/GameModes/" + this.Def.key);

        public GameRules Rule => this.Def.gameRule;

        public int MinLoadoutQuality => this.Def.minLoadoutQuality;
        public int MinProfileLevel   => this.Def.minProfileLevel;

        public List<ERarityType> LootRarities => this.Def.lootRarities;
    }
}