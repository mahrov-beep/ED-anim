namespace Quantum {
  using System.Runtime.InteropServices;
  // using Collections;
  using JetBrains.Annotations;
  using Photon.Deterministic;
  // using UnityEngine;

  [StructLayout(LayoutKind.Auto)]
  public unsafe partial struct AIContextUser {
    // public RNGSession* RNG;

    /*
    TODO погонять гпт вспомнить нету ли тут каких-то проблем с указателем,
      почему раньше рнг хранился в поле из разыменованного указателя
    */
    // ReSharper disable once InconsistentNaming
    public ref RNGSession RNG => ref Unit->RNG;

    public Bot*              Bot;
    public PerceptionMemory* PerceptionMemory;
    public Way*              CurrentWay;

    public Unit*                     Unit;
    public Transform3D*              Transform;
    public InputContainer*           InputContainer;
    public NavMeshPathfinder*        Pathfinder;
    public CharacterSpectatorCamera* SpectatorCamera;
    public KCC*                      KCC;

    // public ref Input Input => ref InputContainer->Input;

    [CanBeNull]
    public Weapon* ActiveWeapon;

    // public FP WeaponTriggerDistance;
    // public FP WeaponTriggerAngle;

    public FPVector3    Position => Transform->Position;
    public FPQuaternion Rotation => Transform->Rotation;

    // public bool HasRealPlayerNearby;

    // public QList<FPVector3> GetWaypoints(FrameThreadSafe f) {
    //   if (!f.Exists(Bot->Way)) {
    //     Debug.LogError("No Way Ref");
    //   }
    //
    //   Way* way = f.GetPointer<Way>(Bot->Way);
    //
    //   return f.ResolveList(way->Points);
    // }

    // public void ResetMovementInput() {
    //   InputContainer->Input.MovementDirection = FPVector2.Zero;
    //   InputContainer->Input.MovementMagnitude = FP._0;
    // }
    //
    // public Input* GetInput(FrameThreadSafe f) {
    //   return &InputContainer->Input;
    // }
    //
    // public void SetLookRotationDelta(FP deltaAngle) {
    //   InputContainer->Input.LookRotationDelta.X = deltaAngle;
    // }
    //
    // public void SetPathfinderTarget(FrameThreadSafe f, NavMesh navMesh, FPVector3 movementTarget) {
    //   Pathfinder->SetTarget(f, movementTarget, navMesh);
    // }
  }
}