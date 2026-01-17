namespace Multicast.Misc.CurrencyFly {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Scripts/Currency Fly Animation Destination (Static Binding)")]
    public class CurrencyFlyAnimationDestinationStaticBinding : ApplicatorBase {
        [SerializeField, Required] private string primaryKey;
        [SerializeField, Required] private string secondaryKey = "shared";

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
                secondaryKey = this.secondaryKey,
            };

            CurrencyFlyAnimation.RegisterDestination(this.lastId.Value, (RectTransform) this.transform);
        }
    }
}