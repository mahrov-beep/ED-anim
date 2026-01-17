namespace Game.Domain.Game {
    using GameInventory;
    using JetBrains.Annotations;
    using Multicast;
    using Quantum;
    using UniMob;
    using UnityEngine;

    public class GameLocalCharacterModel : Model {
        public GameLocalCharacterModel(Lifetime lifetime) : base(lifetime) {
        }

        [Atom] public bool IsDead { get; set; }
        [Atom] public bool IsKnocked { get; set; }
        [Atom] public bool IsBeingRevived { get; set; }

        [Atom] public EntityRef? RebirthTicket { get; set; }
        [Atom] public EntityRef? ReviveTarget { get; set; }

        [Atom] public UnitStats Stats { get; set; }

        [Atom] public float Health    { get; set; }
        [Atom] public float MaxHealth { get; set; }

        [Atom] public float KnockHealth { get; set; }
        [Atom] public float KnockTimeRemaining { get; set; }
        [Atom] public float KnockTimeTotal     { get; set; }
        [Atom] public float ReviveProgress     { get; set; }
        [Atom] public float RevivePromptProgress { get; set; }
        [Atom] public bool  CanReviveTeammate    { get; set; }
        [Atom] public bool  IsRevivingTeammate   { get; set; }

        [Atom] public bool  CanKnifeAttack    { get; set; }
        [Atom] public bool  IsKnifeAttacking  { get; set; }
        [Atom] public float KnifeCooldownProgress { get; set; }
        [Atom] public EntityRef? KnifeTarget  { get; set; }
        
        [Atom] public Vector3 PositionView { get; set; }

        [Atom] public bool IsAiming { get; set; }
        [Atom] public bool IsHealing { get; set; }
        [Atom] public bool ShouldHideWeaponsWhileHealing { get; set; }
        [Atom] public float HealingProgress { get; set; }

        [Atom, CanBeNull] public GameInventoryTrashItemModel BestMedKit { get; set; }
    }
}