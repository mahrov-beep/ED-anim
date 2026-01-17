using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using Game.ECS.Systems.Input;
using Game.ECS.Systems.Player;
using Game.Services.Photon;
using Multicast;
using Quantum;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;

public class AimButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler {
    [InputControl(layout = "Button")]
    [SerializeField]
    private new string controlPath = "<Mouse>/middleButton";

    [Header("UI")]
    [SerializeField, Required] private Image background;
    [SerializeField, Required] private Image icon;

    [Header("Colors")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color crouchingColor = Color.yellow;

    protected override string controlPathInternal { get => controlPath; set => controlPath = value; }

    private PhotonService     photonService;
    private LocalPlayerSystem localPlayerSystem;

    private bool lastAimState;
    private bool initializedState;

    void Awake() {
        photonService     = App.Get<PhotonService>();
        localPlayerSystem = App.Get<LocalPlayerSystem>();
    }

    private void Start() {
        UpdateVisualState(force: true);
    }

    private void Update() {
        UpdateVisualState();
    }

    protected override void OnDisable() {
        base.OnDisable();
        
        initializedState   = false;
        SentDefaultValueToControl();
    }
    
    public void OnPointerDown(PointerEventData eventData) {
        SendValueToControl(1f);
    }

    public void OnPointerUp(PointerEventData eventData) {
        this.SentDefaultValueToControl();
    }
    
    private unsafe void UpdateVisualState(bool force = false) {
        var isAim = EvaluateAimState();

        if (!force && initializedState && isAim == this.lastAimState) {
            return;
        }

        initializedState = true;
        lastAimState     = isAim;

        var color = isAim ? this.crouchingColor : this.availableColor;
        ApplyColor(background, color);
        ApplyColor(icon, color);
    }

    private unsafe bool EvaluateAimState() {
        if (photonService == null || localPlayerSystem == null) {
            return false;
        }

        if (!photonService.TryGetPredicted(out var frame)) {
            return false;
        }

        if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
            return false;
        }

        if (!frame.TryGet(localRef, out Unit unit)) {
            return false;
        }

        return unit.Aiming;
    }
    
    private static void ApplyColor(Image target, Color color) {
        if (target == null) {
            return;
        }

        color.a      = target.color.a;
        target.color = color;
    }
}