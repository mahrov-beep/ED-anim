#nullable enable
namespace Game.ECS.Systems.Unit {
    using System.Collections.Generic;
    using Components.Unit;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using Shared.UserProfile.Data;
    using Sirenix.OdinInspector;
    using UniMob;
    using UnityEngine;

    public class MainMenuCharacterVisual : LifetimeMonoBehaviour {
        [SerializeField, Required]
        private CharacterVisual characterVisual;

        [SerializeField, Required]
        private EscapeCharacterRemotePlayer escapeCharacterRemotePlayer;

        [SerializeField] private bool isRemote;

        private GameSnapshotLoadout externalLoadout;
        private MenuCharacterEquipmentApplier equipmentApplier;

        protected override void Start() {
            base.Start();

            this.equipmentApplier = new MenuCharacterEquipmentApplier(this.escapeCharacterRemotePlayer);

            InitializeWithDelayAsync().Forget();

            Atom.Reaction(this.Lifetime, () => {
                var loadout = this.externalLoadout;
                
                if (loadout == null) {
                    var sdUserProfile   = App.Get<SdUserProfile>();
                    var selectedLoadout = sdUserProfile.Loadouts.Get(sdUserProfile.Loadouts.SelectedLoadout.Value);
                    
                    loadout = selectedLoadout.LoadoutSnapshot.Value;
                }

                var slotItems = loadout?.SlotItems;
                
                if (slotItems == null) {
                    return;
                }

                foreach (var slotType in CharacterLoadoutSlotsExtension.AllValidSlots) {
                    var slotItem = slotType.ToInt() < slotItems.Length ? slotItems[slotType.ToInt()] : null;
                    var slotName = EnumNames<CharacterLoadoutSlots>.GetName(slotType);

                    this.characterVisual.SetVisual(slotName, slotItem?.ItemKey);
                }

                this.equipmentApplier?.ApplyEquipment(loadout);
                CreatePartyComponent();
            });
        }

        private async UniTaskVoid InitializeWithDelayAsync() {
            await UniTask.Yield();

            if (this.externalLoadout == null) {
                var sdUserProfile   = App.Get<SdUserProfile>();
                var selectedLoadout = sdUserProfile.Loadouts.Get(sdUserProfile.Loadouts.SelectedLoadout.Value);
                var loadout         = selectedLoadout.LoadoutSnapshot.Value;

                if (loadout?.SlotItems != null) {
                    foreach (var slotType in CharacterLoadoutSlotsExtension.AllValidSlots) {
                        var slotItem = slotType.ToInt() < loadout.SlotItems.Length ? loadout.SlotItems[slotType.ToInt()] : null;
                        var slotName = EnumNames<CharacterLoadoutSlots>.GetName(slotType);
                        
                        this.characterVisual.SetVisual(slotName, slotItem?.ItemKey);
                    }

                    this.equipmentApplier?.ApplyEquipment(loadout);
                    CreatePartyComponent();
                }
            }
        }

        private void CreatePartyComponent() {
            if (this.isRemote) {
                return;
            }
            
            var world = World.Default;
            var stash = world.GetStash<UnitPartyComponent>();

            var localId = App.ServerAccessTokenInfo.UserId;
            
            var sdUserProfile = App.Get<SdUserProfile>();

            var filter = world.Filter.With<UnitPartyComponent>().Build();
            var hasPrimary = false;
            var duplicates = new List<Entity>();

            foreach (var entity in filter) {
                ref var party = ref stash.Get(entity);

                if (party.guid != localId) {
                    continue;
                }

                if (!hasPrimary) {
                    hasPrimary = true;
                    party.transform = this.transform;
                    party.level     = sdUserProfile.Level.Value;
                    party.nickName  = sdUserProfile.NickName.Value;
                }
                else {
                    duplicates.Add(entity);
                }
            }

            foreach (var entity in duplicates) {
                stash.Remove(entity);
                world.RemoveEntity(entity);
            }

            if (hasPrimary) {
                return;
            }

            var newEntity = world.CreateEntity();

            ref var newParty = ref stash.Add(newEntity);

            newParty.guid      = localId;
            newParty.transform = this.transform;
            newParty.level     = sdUserProfile.Level.Value;
            newParty.nickName  = sdUserProfile.NickName.Value;
        }

        public void ApplyLoadout(GameSnapshotLoadout loadout) {
            this.externalLoadout = loadout;
            
            if (loadout == null || loadout.SlotItems == null) {
                return;
            }

            foreach (var slotType in CharacterLoadoutSlotsExtension.AllValidSlots) {
                var slotItem = slotType.ToInt() < loadout.SlotItems.Length ? loadout.SlotItems[slotType.ToInt()] : null;
                var slotName = EnumNames<CharacterLoadoutSlots>.GetName(slotType);
                
                this.characterVisual.SetVisual(slotName, slotItem?.ItemKey);
            }

            this.equipmentApplier?.ApplyEquipment(loadout);
        }
    }
}
