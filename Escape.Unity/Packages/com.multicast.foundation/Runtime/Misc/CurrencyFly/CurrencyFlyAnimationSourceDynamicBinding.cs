namespace Multicast.Misc.CurrencyFly {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Scripts/Currency Fly Animation Source (Dynamic Binding)")]
    public class CurrencyFlyAnimationSourceDynamicBinding : ApplicatorBase {
        [SerializeField, Required] private string primaryKey;

        [SerializeField] private ViewVariableString secondaryKey;

        [NonSerialized] private CurrencyFlySourceId? lastId;

        private void OnEnable() {
            if (this.lastId != null) {
                CurrencyFlyAnimation.RegisterSource(this.lastId.Value, (RectTransform) this.transform);
            }
        }

        private void OnDisable() {
            if (this.lastId != null) {
                CurrencyFlyAnimation.UnregisterSource(this.lastId.Value, (RectTransform) this.transform);
            }
        }

        protected override void Apply() {
            if (this.lastId != null) {
                CurrencyFlyAnimation.UnregisterSource(this.lastId.Value, (RectTransform) this.transform);
            }

            this.lastId = new CurrencyFlySourceId {
                primaryKey   = this.primaryKey,
                secondaryKey = this.secondaryKey.Value,
            };

            CurrencyFlyAnimation.RegisterSource(this.lastId.Value, (RectTransform) this.transform);
        }
    }
}