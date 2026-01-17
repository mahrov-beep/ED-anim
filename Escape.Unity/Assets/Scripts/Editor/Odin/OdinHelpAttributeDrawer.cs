using Quantum;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
public class OdinHelpAttributeDrawer : OdinAttributeDrawer<HelpAttribute> {
    protected override void DrawPropertyLayout(GUIContent label) {
        var newLabel = label == null ? null : new GUIContent(label.text + " [?]", this.Attribute.Help);
        this.CallNextDrawer(newLabel);
    }
}