namespace Multicast.Misc.CurrencyFly {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Scripts/Currency Fly Animation Source (Static Binding)")]
    public class CurrencyFlyAnimationSourceStaticBinding : ApplicatorBase {
        [SerializeField, Required] private string primaryKey;
        [SerializeField, Required] private string secondaryKey = "shared";

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
                secondaryKey = this.secondaryKey,
            };

            CurrencyFlyAnimation.RegisterSource(this.lastId.Value, (RectTransform) this.transform);
        }
    }
}