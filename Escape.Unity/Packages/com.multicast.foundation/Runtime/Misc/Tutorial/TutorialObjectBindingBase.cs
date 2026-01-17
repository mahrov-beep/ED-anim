#if TUTORIAL_MASK

namespace Multicast.Misc.Tutorial {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CodeWriter.UIExtensions;
    using CodeWriter.ViewBinding;
    using TriInspector;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    [DrawWithTriInspector]
    [RequireComponent(typeof(TutorialObject))]
    [DeclareBoxGroup("Button", HideTitle = true)]
    public abstract class TutorialObjectBindingBase : ApplicatorBase, IAutoViewListener {
        [SerializeField, Required] private TutorialObject tutorialObject;

        [SerializeField] private TutorialObjectActivationMode activationMode = TutorialObjectActivationMode.EnableTutorialObjectComponent;

        [SerializeField]
#if UNITY_EDITOR
        [Dropdown(nameof(EnumerateTutorialPrimaryKeys)), HideLabel, Title("Primary Key")]
#endif
        private string primaryKey;

        private TutorialObjectID lastId;

        private Action<bool> activatorBaking;
        private bool         reActivationRequested;

        private Action<bool> Activator => this.activatorBaking ??= this.SetTutorialActive;

        protected abstract string GetSecondaryKey();

        void IAutoViewListener.Activate() {
            this.Activator(false);

            TutorialObjectGlobal.Register(this.lastId, this.Activator);
        }

        void IAutoViewListener.Deactivate() {
            TutorialObjectGlobal.Unregister(this.lastId, this.Activator);

            this.Activator(false);
        }

        protected override void Apply() {
            TutorialObjectGlobal.Unregister(this.lastId, this.Activator);

            this.lastId = new TutorialObjectID(this.primaryKey, this.GetSecondaryKey());

            TutorialObjectGlobal.Register(this.lastId, this.Activator);
        }

        private void SetTutorialActive(bool active) {
            switch (this.activationMode) {
                case TutorialObjectActivationMode.ActivateGameObject:
                    this.gameObject.SetActive(active);
                    break;
                case TutorialObjectActivationMode.EnableTutorialObjectComponent:
                    this.tutorialObject.enabled = active;
                    break;
            }
        }

        protected virtual void Update() {
            if (this.reActivationRequested) {
                this.reActivationRequested = false;

                this.gameObject.SetActive(false);
                this.gameObject.SetActive(true);
            }
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();

            this.tutorialObject = this.GetComponent<TutorialObject>();
        }

        private static IEnumerable<TriDropdownItem<string>> EnumerateTutorialPrimaryKeys() {
            return TypeCache.GetTypesWithAttribute<TutorialIDsContainerAttribute>()
                .SelectMany(type => EnumerateNestedTypesRecursive(type))
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(field => field.FieldType == typeof(TutorialObjectID))
                .Select(field => (type: field.DeclaringType, value: (TutorialObjectID)field.GetValue(null)))
                .Select(it => new TriDropdownItem<string> {
                    Text  = $"{GetTypePath(it.type)}{it.value.primary}",
                    Value = it.value.primary,
                })
                .ToList();
        }

        private static string GetTypePath(Type type) {
            var path = "";
            for (; type != null; type = type.DeclaringType) {
                path = type.Name + " / " + path;
            }

            return path;
        }

        private static IEnumerable<Type> EnumerateNestedTypesRecursive(Type type) {
            yield return type;

            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
                foreach (var result in EnumerateNestedTypesRecursive(nestedType)) {
                    yield return result;
                }
            }
        }
#endif
    }
}

#endif