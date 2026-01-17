namespace Multicast.Misc.CurrencyFly {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Scripts/Currency Fly Animation Destination (Dynamic Binding)")]
    public class CurrencyFlyAnimationDestinationDynamicBinding : ApplicatorBase {
        [SerializeField, Required] private string primaryKey;

        [SerializeField] private ViewVariableString secondaryKey;

        [NonSerialized] private CurrencyFlyDestinationId? lastId;

        private void OnEnable() {
            if (this.lastId != null) {
                CurrencyFlyAnimation.RegisterDestination(this.lastId.Value, (RectTransform) this.transform);
            }
        }

        private void OnDisable() {
            if (this.lastId != null) {
                CurrencyFlyAnimation.UnregisterDestination(this.lastId.Value, (RectTransform) this.transform);
            }
        }

        protected override void Apply() {
            if (this.lastId != null) {
                CurrencyFlyAnimation.UnregisterDestination(this.lastId.Value, (RectTransform) this.transform);
            }

            this.lastId = new CurrencyFlyDestinationId {
                primaryKey   = this.primaryKey,
                secondaryKey = this.secondaryKey.Value,
            };

            CurrencyFlyAnimation.RegisterDestination(this.lastId.Value, (RectTransform) this.transform);
        }
    }
}