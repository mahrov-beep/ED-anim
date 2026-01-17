using Game.ECS.Systems.Player;
using Multicast;
using Quantum;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

public class CameraAimZoom : QuantumSceneViewComponent {
    [SerializeField, Required] private CinemachineCamera cinemachineCamera;
    [SerializeField]           private float             zoomSpeed = 1f;
    [SerializeField]           private float             dropTimer = 0.1f;

    private float defaultZoom;
    private float dropTimerValue;

    private LocalPlayerSystem localPlayerSystem;

    public override void OnActivate(Frame frame) {
        base.OnActivate(frame);
        defaultZoom       = cinemachineCamera.Lens.FieldOfView;
        localPlayerSystem = App.Get<LocalPlayerSystem>();
    }

    public override void OnDeactivate() {
        this.cinemachineCamera.Lens.FieldOfView = this.defaultZoom;
        
        base.OnDeactivate();
    }

    public override void OnUpdateView() {
        base.OnUpdateView();

        if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
            return;
        }

        UpdateZoom(localRef);
    }

    public unsafe void UpdateZoom(EntityRef entityRef) {
        var weaponOwner = PredictedFrame.GetPointer<Quantum.Unit>(entityRef);
        var input       = PredictedFrame.GetPointer<InputContainer>(entityRef);

        var weaponConfig = weaponOwner->GetActiveWeaponConfig(PredictedFrame);

        if (!weaponConfig) {
            return;
        }

        bool inAimState = weaponOwner->Aiming;

        if (inAimState) {
            dropTimerValue = dropTimer;
        }
        else {
            dropTimerValue -= Time.deltaTime;
        }

        float targetZoom;
        if (dropTimerValue > 0) {
            targetZoom = defaultZoom * weaponConfig.zoomMultOnAim.AsFloat;
        }
        else {
            targetZoom = defaultZoom;
        }

        cinemachineCamera.Lens.FieldOfView.LerpTo(targetZoom, Time.deltaTime * zoomSpeed);
    }
}