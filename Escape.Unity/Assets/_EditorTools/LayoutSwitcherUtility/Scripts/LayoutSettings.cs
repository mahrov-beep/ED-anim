namespace _EditorTools.LayoutSwitcherUtility.Scripts {
    using UnityEngine;
    [CreateAssetMenu(fileName = "LayoutSettings", menuName = "Tools/Layout Settings")]
    public class LayoutSettings : ScriptableObject {
        [Header("Имена макетов должны соответствовать пунктам в меню Window > Layouts")]
        public string layout1Name;
        public EResizeWindow layout1ResizeOption;

        public string        layout2Name;
        public EResizeWindow layout2ResizeOption;

        public string        layout3Name;
        public EResizeWindow layout3ResizeOption;

        public string        layout4Name;
        public EResizeWindow layout4ResizeOption;
    }
}