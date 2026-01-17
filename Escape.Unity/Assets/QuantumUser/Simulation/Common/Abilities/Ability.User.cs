using Photon.Deterministic;
using System;

namespace Quantum {
  using UnityEngine;
  public unsafe partial struct Ability {
    public struct AbilityState {
      public bool IsDelayed;
      public bool IsActive;
      public bool IsActiveStartTick;
      public bool IsActiveEndTick;
      public bool IsOnCooldown;
    }

    public bool HasBufferedInput  => InputBufferTimer.IsRunning;
    public bool IsDelayed         => DelayTimer.IsRunning;
    public bool IsActive          => DurationTimer.IsRunning;
    public bool IsDelayedOrActive => IsDelayed || IsActive;
    public bool IsOnCooldown      => CooldownTimer.IsRunning;

    public AbilityItemAsset GetConfig(Frame f) {
      var config = f.FindAsset(Config);
      if (!config) {
        Debug.LogError($"Config not found!");
      }
      return config;
    }

    public bool TryActivateAbility(Frame f, EntityRef ownerRef) {
      if (IsOnCooldown) {
        return false;
      }

      var unit = f.GetPointer<Unit>(ownerRef);

      if (unit->HasDelayedOrActiveAbility(f)) {
        return false;
      }

      AbilityItemAsset abilityItem = f.FindAsset(Config);

      InputBufferTimer.Reset();
      DelayTimer.Start(abilityItem.delay);
      if (!abilityItem.startCooldownAfterDelay) {
        CooldownTimer.Start(abilityItem.cooldownSec);
      }

      return true;
    }

    public AbilityState Update(Frame f, EntityRef ownerRef) {
      AbilityState state = new();

      InputBufferTimer.Tick(f.DeltaTime);
      CooldownTimer.Tick(f.DeltaTime);

      state.IsOnCooldown = IsOnCooldown;

      if (IsDelayedOrActive) {
        if (CharacterFsm.CurrentStateIs<CharacterStateDead>(f, ownerRef) || f.Has<UnitExited>(ownerRef)) {
          StopAbility(f, ownerRef);

          return state;
        }

        FP delayTimeLeft = DelayTimer.TimeLeft;

        if (IsDelayed) {
          DelayTimer.Tick(f.DeltaTime);

          if (DelayTimer.IsRunning) {
            state.IsDelayed = true;
          }
          else {
            state.IsActiveStartTick = true;

            AbilityItemAsset abilityItem = f.FindAsset(Config);

            var unit = f.GetPointer<Unit>(ownerRef);
            unit->ActiveAbilityInfo.CastDirectionNormalized = GetCastDirection(f, abilityItem, ownerRef);
            unit->ActiveAbilityInfo.CastRotation            = FPQuaternion.LookRotation(unit->ActiveAbilityInfo.CastDirectionNormalized);

            DurationTimer.Start(abilityItem.durationSec);

            if (abilityItem.startCooldownAfterDelay) {
              CooldownTimer.Start(abilityItem.cooldownSec);
            }
          }
        }

        if (IsActive) {
          state.IsActive = true;

          DurationTimer.Tick(f.DeltaTime - delayTimeLeft);

          if (DurationTimer.IsDone) {
            state.IsActiveEndTick = true;

            StopAbility(f, ownerRef);
          }
        }
      }

      return state;
    }

    public void BufferInput(Frame f) {
      AbilityItemAsset abilityItem = f.FindAsset(Config);
      InputBufferTimer.Start(abilityItem.inputBuffer);
    }

    public void StopAbility(Frame f, EntityRef entityRef) {
      f.LogTrace(entityRef, "Stop ability");

      DelayTimer.Reset();
      DurationTimer.Reset();
    }

    public void ResetCooldown() {
      CooldownTimer.Reset();
    }

    FPVector3 GetCastDirection(Frame f, AbilityItemAsset abilityItem, EntityRef ownerRef) {
      var transform      = f.GetPointer<Transform3D>(ownerRef);
      var inputContainer = f.GetPointer<InputContainer>(ownerRef);

      EAbilityCastDirectionType castDirectionType = abilityItem.castDirectionType;

      if (castDirectionType.HasFlag(EAbilityCastDirectionType.Aim)) {
        var aim  = f.GetPointer<UnitAim>(ownerRef);

        var position = f.GameModeAiming.GetAimOrigin(f, ownerRef);
        
        return (aim->AimCurrentPosition - position).Normalized;
      }

      if (castDirectionType.HasFlag(EAbilityCastDirectionType.FacingDirection)) {
        return transform->Forward;
      }

      if (castDirectionType.HasFlag(EAbilityCastDirectionType.Movement)) {
        if (inputContainer->Input.MovementMagnitude > FP._0_03) {
          return transform->InverseTransformDirection(inputContainer->Input.MovementDirection.XOY);
        }
        return transform->Forward;
      }

      f.LogError(ownerRef, $"Unknown {nameof(EAbilityCastDirectionType)}: {abilityItem.castDirectionType} ");

      return transform->Forward;
    }

  }

}