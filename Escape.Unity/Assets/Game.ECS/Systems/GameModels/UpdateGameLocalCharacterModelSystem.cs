namespace Game.ECS.Systems.GameModels {
    using Components.Unit;
    using Domain.Game;
    using Domain.GameInventory;
    using Multicast;
    using Photon.Deterministic;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class UpdateGameLocalCharacterModelSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private GameLocalCharacterModel localCharacterModel;
        [Inject] private GameInventoryModel      gameInventoryModel;
        [Inject] private Stash<UnitComponent> unitStash;

        public override void OnAwake() {}    
        
        public unsafe override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {                
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {               
                return;
            }

            if (!f.TryGet(localRef, out Unit unit)) {                
                return;
            }

            if (!f.TryGetPointer(localRef, out Unit* unitPtr)) {
                return;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {                
                return;
            }

            if (f.TryGet(localRef, out Health health)) {
                this.localCharacterModel.Health    = health.CurrentValue.AsFloat;
                this.localCharacterModel.MaxHealth = health.MaxValue.AsFloat;
            }

            this.localCharacterModel.IsAiming           = unit.Aiming;
            this.localCharacterModel.IsHealing          = false;
            this.localCharacterModel.ShouldHideWeaponsWhileHealing = false;
            this.localCharacterModel.HealingProgress    = 0f;
            this.localCharacterModel.IsDead             = CharacterFsm.CurrentStateIs<CharacterStateDead>(f, localRef);
            this.localCharacterModel.IsKnocked          = CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, localRef);

            bool isKnocked = this.localCharacterModel.IsKnocked;
            bool isKnifeStateActive = CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, localRef);

            this.localCharacterModel.IsBeingRevived     = false;
            this.localCharacterModel.IsRevivingTeammate = false;
            this.localCharacterModel.CanReviveTeammate  = false;
            this.localCharacterModel.ReviveTarget       = null;
            this.localCharacterModel.KnockHealth        = 0f;
            this.localCharacterModel.ReviveProgress     = 0f;
            this.localCharacterModel.RevivePromptProgress = 0f;
            this.localCharacterModel.KnockTimeRemaining = 0f;
            this.localCharacterModel.KnockTimeTotal     = 0f;
            this.localCharacterModel.CanKnifeAttack     = false;
            this.localCharacterModel.IsKnifeAttacking   = isKnifeStateActive;
            this.localCharacterModel.KnifeCooldownProgress = 0f;
            this.localCharacterModel.KnifeTarget        = null;
            
            var missingHealth = this.localCharacterModel.MaxHealth - this.localCharacterModel.Health;
            this.localCharacterModel.BestMedKit = this.localCharacterModel.IsHealing
                ? null
                : this.SelectBestMedkit(missingHealth);

            if (isKnocked) {
                if (f.TryGetPointer(localRef, out CharacterStateKnocked* knocked)) {
                    this.localCharacterModel.IsBeingRevived     = knocked->IsBeingRevived;
                    this.localCharacterModel.KnockHealth        = knocked->KnockHealth.AsFloat;
                    this.localCharacterModel.KnockTimeRemaining = knocked->KnockTimer.AsFloat;
                    this.localCharacterModel.KnockTimeTotal     = knocked->KnockDuration.AsFloat;
                    this.localCharacterModel.ReviveProgress     = knocked->ReviveProgress.AsFloat;
                }
            }

            if (isKnifeStateActive && f.TryGetPointer(localRef, out CharacterStateKnifeAttack* knifeStatePtr)) {
                var knifeConfigDuration = KnifeAttackHelper.ResolveSettings(f, unitPtr).Duration.AsFloat;
                if (knifeConfigDuration > 0f) {
                    var timeLeft = Mathf.Clamp(knifeStatePtr->StateTimer.RemainingSeconds(f).AsFloat, 0f, knifeConfigDuration);
                    this.localCharacterModel.KnifeCooldownProgress = Mathf.Clamp01(timeLeft / Mathf.Max(knifeConfigDuration, float.Epsilon));
                }
                else {
                    this.localCharacterModel.KnifeCooldownProgress = 0f;
                }
            }

            if (CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, localRef) &&
                f.TryGetPointer(localRef, out CharacterStateHealing* healing)) {
                this.localCharacterModel.IsHealing = true;

                var duration  = healing->Duration.AsFloat;
                var remaining = healing->Timer.AsFloat;
                var hideDelay = 0f;

                if (healing->ItemEntity != EntityRef.None &&
                    f.TryGet(healing->ItemEntity, out Item healingItem)) {
                    if (f.FindAsset(healingItem.Asset) is HealBoxItemAsset healAsset) {
                        hideDelay = healAsset.hideWeaponsDelaySeconds.AsFloat;
                    }
                }

                if (duration <= 0f) {
                    this.localCharacterModel.HealingProgress = 1f;
                }
                else {
                    var normalized = 1f - Mathf.Clamp01(Mathf.Max(0f, remaining) / duration);
                    this.localCharacterModel.HealingProgress = normalized;
                }

                var elapsed = duration - Mathf.Max(0f, remaining);
                this.localCharacterModel.ShouldHideWeaponsWhileHealing = elapsed >= hideDelay;
            }

            if (CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, localRef) &&
                f.TryGetPointer(localRef, out CharacterStateReviving* reviving)) {
                this.localCharacterModel.IsRevivingTeammate = true;
                this.localCharacterModel.ReviveTarget       = reviving->Target;

                if (f.TryGetPointer(reviving->Target, out CharacterStateKnocked* targetKnocked)) {
                    this.localCharacterModel.ReviveProgress       = targetKnocked->ReviveProgress.AsFloat;
                    this.localCharacterModel.RevivePromptProgress = targetKnocked->ReviveProgress.AsFloat;
                }
            }

            if (!this.localCharacterModel.IsDead && !this.localCharacterModel.IsKnocked) {

                var knockedFilter = f.Filter<CharacterStateKnocked>();
                EntityRef reviveTarget = EntityRef.None;
                FP        closestDistanceSqr = FP.MaxValue;
                float     revivePrompt = 0f;

                while (knockedFilter.NextUnsafe(out EntityRef knockedEntity, out CharacterStateKnocked* otherKnocked)) {
                    if (otherKnocked->CandidateRescuer != localRef) {
                        continue;
                    }

                    if (reviveTarget == EntityRef.None || otherKnocked->CandidateDistanceSqr < closestDistanceSqr) {
                        reviveTarget        = knockedEntity;
                        closestDistanceSqr  = otherKnocked->CandidateDistanceSqr;
                        revivePrompt        = otherKnocked->ReviveProgress.AsFloat;
                    }
                }

                if (reviveTarget != EntityRef.None) {
                    this.localCharacterModel.CanReviveTeammate = true;
                    if (!this.localCharacterModel.IsRevivingTeammate) {
                        this.localCharacterModel.ReviveTarget = reviveTarget;
                        this.localCharacterModel.RevivePromptProgress = revivePrompt;
                    }
                }
           
                bool canUseKnifeByState = !isKnifeStateActive &&
                                          !CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, localRef) &&
                                          !CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, localRef) &&
                                          !CharacterFsm.CurrentStateIs<CharacterStateRoll>(f, localRef);

                if (canUseKnifeByState &&
                    f.TryGetPointer(localRef, out Team* localTeam) &&
                    f.TryGetPointer(localRef, out Transform3D* localTransform)) {

                    f.TryGetPointer(localRef, out UnitAim* unitAim);

                    FP knifeDistanceOverride = KnifeSettings.Default.Distance;
                    FP knifeAngleOverride    = KnifeSettings.Default.AttackAngleDegrees;
                    if (unit.Asset.IsValid && f.FindAsset(unit.Asset) is UnitAsset ua) {
                        var settings = ua.GetKnifeSettings();
                        if (settings.Distance > FP._0) {
                            knifeDistanceOverride = settings.Distance;
                        }
                        if (settings.AttackAngleDegrees > FP._0) {
                            knifeAngleOverride = settings.AttackAngleDegrees;
                        }
                    }

                    if (KnifeHelper.TryFindTarget(f, localRef, localTeam, localTransform, unitAim, knifeDistanceOverride, knifeAngleOverride, out var knifeTarget, out _, out _)) {
                        this.localCharacterModel.KnifeTarget     = knifeTarget;
                        this.localCharacterModel.CanKnifeAttack  = canUseKnifeByState;                       
                    } 
                } 
            }

            this.localCharacterModel.RebirthTicket = loadout.TryFindRebirthTicketInTrash(f, out var ticket) ? ticket : null;
            this.localCharacterModel.Stats         = unit.CurrentStats;

            ref var unitComponent = ref unitStash.Get(localPlayerSystem.LocalEntity);
            this.localCharacterModel.PositionView = unitComponent.PositionView;
        }

        private GameInventoryTrashItemModel SelectBestMedkit(float missingHealth) {
            const float healAmountTolerance = 0.01f;

            if (missingHealth <= healAmountTolerance) {
                return null;
            }

            GameInventoryTrashItemModel best = null;

            float bestHealAmount = 0f;
            bool  bestFills      = false;

            foreach (var model in this.gameInventoryModel.EnumerateTrashItems()) {
                if (!model.CanBeUsed.Value) {
                    continue;
                }

                if (model.ItemAsset is not HealBoxItemAsset healBoxAsset) {
                    continue;
                }

                var healAmount = this.CalculateHealAmount(healBoxAsset);
                if (healAmount <= 0f) {
                    continue;
                }

                var fillsMissingHealth = healAmount >= missingHealth - healAmountTolerance;

                if (fillsMissingHealth) {
                    if (!bestFills || healAmount < bestHealAmount - healAmountTolerance ||
                        (bestFills && Mathf.Approximately(healAmount, bestHealAmount) && this.PreferLowerRarity(model, best))) {
                        best           = model;
                        bestHealAmount = healAmount;
                        bestFills      = true;
                    }

                    continue;
                }

                if (bestFills) {
                    continue;
                }

                if (best == null || healAmount > bestHealAmount + healAmountTolerance ||
                    (Mathf.Approximately(healAmount, bestHealAmount) && this.PreferLowerRarity(model, best))) {
                    best           = model;
                    bestHealAmount = healAmount;
                }
            }

            return best;
        }

        private bool PreferLowerRarity(GameInventoryTrashItemModel candidate, GameInventoryTrashItemModel current) {
            if (current == null) {
                return true;
            }

            return candidate.ItemAsset.rarity < current.ItemAsset.rarity;
        }

        private float CalculateHealAmount(HealBoxItemAsset healBoxAsset) {
            var healValue = healBoxAsset.heal.Value.AsFloat;

            if (healBoxAsset.heal.ValueIsPercent) {
                return healValue * 0.01f * Mathf.Max(0f, this.localCharacterModel.MaxHealth);
            }

            return healValue;
        }
    }
}
