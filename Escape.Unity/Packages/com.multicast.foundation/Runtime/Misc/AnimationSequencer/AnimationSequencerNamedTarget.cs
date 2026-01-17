namespace Multicast.Misc.AnimationSequencer {
    using System;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    [Serializable]
    public struct AnimationSequencerNamedTarget {
        [SerializeField] private string name;

        public AnimationSequencerNamedTarget(string name) {
            this.name = name;
        }

        public static implicit operator string(AnimationSequencerNamedTarget namedTarget) {
            return namedTarget.name;
        }

        public static implicit operator AnimationSequencerNamedTarget(string name) {
            return new AnimationSequencerNamedTarget(name);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AnimationSequencerNamedTarget))]
    internal class AnimationSequencerNamedTargetEditor : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var nameProp = property.FindPropertyRelative("name");

            position = EditorGUI.PrefixLabel(position, label);

            var selectedTargetName = nameProp.stringValue;

            var isValidTarget = TryGetTargets(property, out var targets) && targets.IsTargetDefined(selectedTargetName);

            var oldColor = GUI.color;

            if (!isValidTarget) {
                GUI.color = new Color(1f, 0.2f, 0.2f);
            }

            if (GUI.Button(position, selectedTargetName, EditorStyles.popup)) {
                ShowDropdown(position, nameProp);
            }

            GUI.color = oldColor;
        }

        private static bool TryGetTargets(SerializedProperty prop, out AnimationSequencerNamedTargetsBase targets) {
            targets = prop.serializedObject.targetObject is MonoBehaviour mb
                ? mb.GetComponentInParent<AnimationSequencerNamedTargetsBase>()
                : null;
            return targets != null;
        }

        private static void ShowDropdown(Rect position, SerializedProperty nameProp) {
            var menu = new GenericMenu();

            var namePropertyPath = nameProp.propertyPath;

            if (TryGetTargets(nameProp, out var targets)) {
                var hasAny = false;

                foreach (var targetName in targets.EnumerateTargetNames()) {
                    hasAny = true;

                    var isOn = nameProp.stringValue == targetName;

                    menu.AddItem(new GUIContent(targetName), isOn, OnSelect, targetName);
                }

                if (!hasAny) {
                    menu.AddDisabledItem(new GUIContent("No targets in AnimationSequencerNamedTargetsBase defined"), false);
                }
            }
            else {
                menu.AddDisabledItem(new GUIContent("No AnimationSequencerNamedTargetsBase found"), false);
            }

            menu.DropDown(position);

            void OnSelect(object newTargetNameObj) {
                var so = new SerializedObject(nameProp.serializedObject.targetObject);

                so.Update();
                var p = so.FindProperty(namePropertyPath).stringValue = (string) newTargetNameObj;
                so.ApplyModifiedProperties();
            }
        }
    }
#endif
}