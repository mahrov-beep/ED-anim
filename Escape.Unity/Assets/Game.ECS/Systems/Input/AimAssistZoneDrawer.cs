using System.Diagnostics;
using Game.Domain.GameProperties;
using Game.ECS.Systems.Input;
using Multicast;
using Multicast.GameProperties;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class AimAssistZoneDrawer : MonoBehaviour {
    public Image zonePrefab;
    public Color outerZoneColor = new(0, 1, 0, .4f);
    public Color innerZoneColor = new(1, 0, 0, .4f);
    public Image outerZoneImage;
    public Image innerZoneImage;

    [SerializeField]
    private RectTransform canvasRect;
    private Canvas canvas;

    private AimingAssistSystem systemAimingAssist;

    private void Awake() {
        if (!canvas) {
            canvas     = GetComponent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        systemAimingAssist = App.Get<AimingAssistSystem>();
    }

    [Conditional("UNITY_EDITOR")]
    public void SetZones(Rect outerRectPx, Rect innerRectPx) {
        DrawZone(outerZoneImage, outerRectPx);
        outerZoneImage.color = outerZoneColor;

        DrawZone(innerZoneImage, innerRectPx);
        innerZoneImage.color = innerZoneColor;
    }

    [Conditional("UNITY_EDITOR")]
    private void OnGUI() {
        if (!App.Get<GamePropertiesModel>().Get(DebugGameProperties.Booleans.DebugAimAssist)) {
            return;
        }

        if (!systemAimingAssist.TryGetAimAssistDebugInfo(out var power, out var normalizedPower, out var durationInOuterZone, out var durationWithoutRaycastHit, out var playerInputDeg,
                out var assistAppliedDeg, out var activeMode)) {
            return;
        }

        var style = new GUIStyle {
            fontSize  = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = {
                textColor = Color.white,
            },
        };

        var shadowStyle = new GUIStyle(style) {
            normal = {
                textColor = Color.black,
            },
        };

        var centerX = Screen.width * 0.5f;
        var centerY = Screen.height * 0.5f;
        var width   = 400f;
        var height  = 100f;

        var rect = new Rect(centerX - width * 0.5f, centerY - height * 0.5f - 100f, width, height);

        var text = $"Aim Assist Power: {power:F2}\nNormalized: {normalizedPower:F2} ({normalizedPower * 100:F0}%)";

        var shadowRect = new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height);
        GUI.Label(shadowRect, text, shadowStyle);

        GUI.Label(rect, text, style);

        var topLeftStyle = new GUIStyle {
            fontSize  = 18,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft,
            normal = {
                textColor = Color.white,
            },
        };

        var topLeftShadowStyle = new GUIStyle(topLeftStyle) {
            normal = {
                textColor = Color.black,
            },
        };

        var topLeftRect = new Rect(10f, 10f, 500f, 100f);
        var topLeftText = $"Duration in Outer Zone: {durationInOuterZone:F2}s\nDuration without Raycast Hit: {durationWithoutRaycastHit:F2}s";

        var topLeftShadowRect = new Rect(topLeftRect.x + 1, topLeftRect.y + 1, topLeftRect.width, topLeftRect.height);
        GUI.Label(topLeftShadowRect, topLeftText, topLeftShadowStyle);

        GUI.Label(topLeftRect, topLeftText, topLeftStyle);

        var topRightStyle = new GUIStyle {
            fontSize  = 18,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperRight,
            normal = {
                textColor = Color.white,
            },
        };

        var topRightShadowStyle = new GUIStyle(topRightStyle) {
            normal = {
                textColor = Color.black,
            },
        };

        var topRightRect = new Rect(Screen.width - 510f, 10f, 500f, 100f);
        var topRightText = $"Player Input: {playerInputDeg:F2}°\nAssist Applied: {assistAppliedDeg:F2}°\nMode: {activeMode}";

        var topRightShadowRect = new Rect(topRightRect.x + 1, topRightRect.y + 1, topRightRect.width, topRightRect.height);
        GUI.Label(topRightShadowRect, topRightText, topRightShadowStyle);

        GUI.Label(topRightRect, topRightText, topRightStyle);
    }

    private void DrawZone(Image img, Rect rectPx) {
        if (!img) {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, rectPx.center, null, out var localPoint);

        var scale  = canvas.scaleFactor;
        var sizeUI = rectPx.size / scale;

        var rt = img.rectTransform;
        rt.anchoredPosition = localPoint;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeUI.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeUI.y);
        img.enabled = sizeUI.sqrMagnitude > 0f;
    }
}