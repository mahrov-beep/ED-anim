using Photon.Deterministic;
using System;

namespace Quantum {
using Physics3D;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

public abstract unsafe partial class AttackData : AssetObject {
public FP TTL;
public bool IgnoreOwner;
public bool IgnoreEnemies;
public bool IgnoreAlies;
public bool DestroyOnHitDynamic;
public bool DestroyOnHitStatic;

public Effect[] hitEffects;

[BoxGroup("Visual"), FormerlySerializedAs("destroyFX")] public UnityEngine.GameObject attackPerformedFX;
[BoxGroup("Visual"), FormerlySerializedAs("destroyFXLifetimeSec")] public FP attackPerformedFXLifetimeSec;
[BoxGroup("Visual")] public bool attackPerformedSmoothFade;
[BoxGroup("Visual"), ShowIf(nameof(attackPerformedSmoothFade)), MinValue(0), ValidateInput(nameof(ValidatePerformedFadeSeconds), "Must be > 0")]
public FP attackPerformedFadeSeconds;

[BoxGroup("Visual")] public UnityEngine.GameObject attackHitFX;
[BoxGroup("Visual")] public FP attackHitFXLifetimeSec;
[BoxGroup("Visual")] public bool attackHitSmoothFade;
[BoxGroup("Visual"), ShowIf(nameof(attackHitSmoothFade)), MinValue(0), ValidateInput(nameof(ValidateHitFadeSeconds), "Must be > 0")]
public FP attackHitFadeSeconds;

private bool ValidatePerformedFadeSeconds(FP value) {
  return !attackPerformedSmoothFade || value > FP._0;
}

private bool ValidateHitFadeSeconds(FP value) {
  return !attackHitSmoothFade || value > FP._0;
}

[LabelText("Hit Impulse (on target)")]
public AssetRef<CinemachineImpulseAsset> hitImpulse;

[BoxGroup("Audio")]
[LabelText("Enable Hit Sound (2D)")]
[InfoBox("Plays looping 2D sound while local player is inside damage zone (e.g. molotov fire)")]
public bool enableHitSound = false;

[BoxGroup("Audio"), ShowIf(nameof(enableHitSound))]
[LabelText("Hit Sound Clip")]
public UnityEngine.AudioClip hitSound;

[BoxGroup("Audio"), ShowIf(nameof(enableHitSound))]
[LabelText("Hit Sound Volume")]
public FP hitSoundVolume = FP._1;

public virtual void OnUpdate(Frame f, EntityRef attackRef, Attack* attack) { }

public virtual void OnCreate(Frame f, EntityRef attackRef, Attack* attack) {
  f.Signals.OnCreateAttack(attackRef, attack);
}

public virtual void Deactivate(Frame f, EntityRef attackRef) {
  f.Signals.OnDisableAttack(attackRef);

  var attack = f.GetPointer<Attack>(attackRef);
  var attackTransform = f.GetPointer<Transform3D>(attackRef);

  f.Events.AttackPerformedSynced(attackRef, *attack, this, attackTransform->Position);

  f.Destroy(attackRef);
}

public virtual void HandleHit(Frame f, EntityRef attackRef, EntityRef targetRef, FPVector3 hitPoint, FPVector3 hitNormal) {
  var attack = f.GetPointer<Attack>(attackRef);

  f.Events.AttackHitSynced(attackRef, *attack, this, hitPoint, hitNormal, targetRef);
}

protected EntityRef CheckHits(Frame f, HitCollection3D hits, EntityRef attackRef, out bool needToDisable) {
  needToDisable = false;

  for (var i = 0; i < hits.Count; i++) {
    var target = CheckHit(f, hits[i], attackRef, out needToDisable);

    if (target != EntityRef.None) {
      return target;
    }
  }

  return EntityRef.None;
}
protected EntityRef CheckHit(Frame f, Hit3D hit, EntityRef attackRef, out bool needToDisable) {
  needToDisable = false;

  if (!hit.IsDynamic) {
    if (hit.IsTrigger) {
      return EntityRef.None;
    }

    if (DestroyOnHitStatic) {
      needToDisable = true;
      return EntityRef.None;
    }
  }

  var target = hit.Entity;

  if (DestroyOnHitDynamic) {
    var attackTransform = f.GetPointer<Transform3D>(attackRef);
    // DebugDrawHelper.DrawLine(f, attackTransform->Position, hit.Point, ColorRGBA.White, FP._5);
    attackTransform->Position = hit.Point;
    needToDisable = true;
  }

  return CanDamage(f, attackRef, target) ? target : EntityRef.None;
}

protected bool CanDamage(Frame f, EntityRef attackRef, EntityRef target) {
  if (!f.Exists(target)) {
    return false;
  }

  if (!f.TryGetPointer(target, out Health* targetHealth)) {
    return false;
  }

  if (targetHealth->IsDead) {
    if (!CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, target)) {
      return false;
    }
  }

  var attack = f.GetPointer<Attack>(attackRef);

  if (IgnoreOwner && target == attack->SourceUnitRef) {
    return false;
  }

  var isAlly = f.IsAlly(attack->SourceUnitRef, target);
  if (IgnoreEnemies && !isAlly) {
    return false;
  }
  if (IgnoreAlies && isAlly) {
    return false;
  }

  return true;
}
}
}