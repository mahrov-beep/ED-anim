using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum {
  using Core;

  /// <summary>
  /// EarlyLagCompensationSystem creates proxies which are destroyed later in LateLagCompensationSystem.
  /// This system must run before physics system.
  ///
  /// How it works?
  /// =============
  /// As a local player, you see movement of all other player entity views being snapshot interpolated (not limited to players).
  /// But in simulation, other players are predicted.
  /// This system takes interpolation offset (in ticks) and interpolation alpha - both sent via input
  /// and creates a proxy entity (with LagCompensationProxy component) for each entity marked to be lag compensated LagCompensationTarget.
  /// These proxy entities represent state which player saw at the time of firing from a gun.
  ///
  /// Because every player has different interpolation, colliders of proxies must have unique layer per player. Check LagCompensationUtility for more details.
  /// All proxies for PlayerRef:0 have layer 16,
  /// All proxies for PlayerRef:1 have layer 17,
  /// All proxies for PlayerRef:2 have layer 18, and so on
  /// When making lag compensation casts, you must use layer mask based on PlayerRef to hit correct proxies.
  /// Use mnethods from LagCompensationUtility.
  /// </summary>
  [Preserve]
  [SystemsOrder.Before(typeof(PhysicsSystem3D))]
  public unsafe class LagCompensationSystem : SystemMainThreadFilter<LagCompensationSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Transform3D* Transform3D;
      public Unit*        Unit;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<Bot, CharacterStateDead>();

    public override void Update(Frame f, ref Filter filter) {
      var unit          = filter.Unit;
      var unitTransform = filter.Transform3D;
      var playerRef     = unit->PlayerRef;

      if (playerRef == PlayerRef.None || f.GetPlayerData(unit->PlayerRef) == null) {
        return;
      }

      var inputFlags = f.GetPlayerInputFlags(unit->PlayerRef);
      if ((inputFlags & DeterministicInputFlags.PlayerNotPresent) == DeterministicInputFlags.PlayerNotPresent) {
        return;
      }

      if (!f.TryGetPointer(unit->ActiveWeaponRef, out Weapon* activeWeapon)) {
        return;
      }

      var input = f.GetPlayerInput(playerRef);

      // помечаем что для этого кадра создан мир лагокомпенсации
      // в противном случае будет использоваться обычный мир
      unit->LastLagCompensatedFrame = f.Number;

      var attackDistance = activeWeapon->CurrentStats.attackDistance.AsFP;

      var maxLagCompensationDistanceSquared = attackDistance * attackDistance * FP._1_25;

      // Get proxy collider layer locked to specific PlayerRef.
      int colliderLayer = LagCompensationUtility.GetProxyColliderLayer(playerRef);

      // Iterate over all entities which should be part of lag compensated casts.
      var targets = f.Filter<Transform3D, Unit, LagCompensationTarget>();
      targets.UseCulling = false;
      while (targets.NextUnsafe(out var targetEntity, out var targetTransform, out var targetUnit, out var target)) {
        // Don't create a proxy for self. The local player is never interpolated.
        if (targetUnit->PlayerRef == playerRef) {
          continue;
        }

        if (FPVector3.DistanceSquared(targetTransform->Position, unitTransform->Position) > maxLagCompensationDistanceSquared) {
          continue;
        }

        // Don't create a proxy for dead entities.
        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, targetEntity)) {
          continue;
        }

        var proxyEntity = f.Create(target->ProxyPrototype);
        f.SetCullable(proxyEntity, false);

        // We are shooting against proxy, but we'll need a reference to origin entity.
        var proxy = f.GetOrAddPointer<LagCompensationProxy>(proxyEntity);
        proxy->Target = targetEntity;

        var collider = f.Unsafe.GetPointer<PhysicsCollider3D>(proxyEntity);
        collider->Layer = colliderLayer;

        if (f.TryGetPointer(targetEntity, out PhysicsCollider3D* targetCollider)) {
          collider->Shape = targetCollider->Shape;
        }

        // Set proxy transform based on player interpolation data (passed via input).
        var transform = f.Unsafe.GetPointer<Transform3D>(proxyEntity);
        *transform = target->GetInterpolatedTransform(input->InterpolationOffset, input->InterpolationAlpha);
      }
    }
  }
}