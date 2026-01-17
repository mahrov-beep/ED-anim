using System.Collections;
using Quantum;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
public class OdinInlinePingButtonAttributeDrawer : OdinAttributeDrawer<InlinePingButtonAttribute> {
    protected override bool CanDrawAttributeProperty(InspectorProperty property) {
        if (typeof(IList).IsAssignableFrom(property.ValueEntry.TypeOfValue)) {
            return false;
        }

        return base.CanDrawAttributeProperty(property);
    }

    protected override void DrawPropertyLayout(GUIContent label) {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        this.CallNextDrawer(label);
        EditorGUILayout.EndVertical();

        var guiContent = new GUIContent("", "Click to focus on asset in Project window");
        SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(guiContent.text, null, true, EditorGUIUtility.singleLineHeight, out _, out _, out _, out var totalWidth);
        var controlRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.MaxWidth(totalWidth));

        if (SirenixEditorGUI.SDFIconButton(controlRect, guiContent, SdfIconType.GeoAlt)) {
            var value = this.Property.ValueEntry.WeakSmartValue;
            if (value is Object obj) {
                EditorGUIUtility.PingObject(obj);
            }
            else if (value.GetType().GetField("Id") is { } quantumIdField && quantumIdField.FieldType == typeof(AssetGuid)) {
                var quantumAssetGuid = (AssetGuid)quantumIdField.GetValue(value);
                var quantumAsset     = QuantumUnityDB.GetGlobalAssetEditorInstance(quantumAssetGuid);
                EditorGUIUtility.PingObject(quantumAsset);
            }
            else {
                Debug.LogError($"Ping of type '{value?.GetType()}' not implemented");
            }
        }

        EditorGUILayout.EndHorizontal();
    }
}