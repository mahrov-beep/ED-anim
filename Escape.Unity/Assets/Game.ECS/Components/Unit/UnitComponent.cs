namespace Game.ECS.Components.Unit {
using System;
using global::Quantum;
using JetBrains.Annotations;
using Game.ECS.Scripts.GameView;
using Photon.Deterministic;
using Sirenix.OdinInspector;
using Systems.Unit;
using UnityEngine;
using UnityEngine.Serialization;
using IComponent = Scellecs.Morpeh.IComponent;

[Serializable, RequireFieldsInit]
public struct UnitComponent : IComponent {
    public Vector3 healthBarOffset;
    
    [Required] public QuantumEntityView quantumEntityView;
    
    [CanBeNull] public ReconOutline reconOutline;

    [CanBeNull] public CharacterVisual[] visuals;
    
    [CanBeNull] public DebuffVisual debuffVisual;

    public GameObject AimAssistTarget;
    
    public Transform fpsCameraRoot;
    public Transform root;

    public EntityRef EntityRef => quantumEntityView.EntityRef;

    public Vector3 PositionView => quantumEntityView.Transform.position;
    public Quaternion RotationView => quantumEntityView.Transform.rotation;
    
    public FPVector3 FPPositionView => quantumEntityView.Transform.position.ToFPVector3();
    public FPQuaternion FPRotationView => quantumEntityView.Transform.rotation.ToFPQuaternion();
}

[Serializable]
public struct UnitDeadMarker : IComponent { }
}