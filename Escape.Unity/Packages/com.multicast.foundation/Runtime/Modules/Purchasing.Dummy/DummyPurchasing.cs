namespace Multicast.Modules.Purchasing.Dummy {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Collections;
    using Cysharp.Threading.Tasks;
    using Multicast.Purchasing;
    using UniMob;
    using UnityEngine;

    public class DummyPurchasing : IPurchasing {
        private readonly LookupCollection<PurchaseDef>              purchaseDefs;
        private readonly Options                                    options;
        private readonly MutableAtom<PurchasingInitializationState> state;

        private HashSet<string> purchases = new();

        public PurchasingInitializationState InitializationState => this.state.Value;

        public DummyPurchasing(Lifetime lifetime, LookupCollection<PurchaseDef> purchaseDefs, Options options) {
            this.purchaseDefs = purchaseDefs;
            this.options      = options;
            this.state        = Atom.Value(lifetime, PurchasingInitializationState.Loading);
        }

        public void Initialize() {
            if (this.options.failOnInitialization) {
                this.state.Value = PurchasingInitializationState.InitializationFailed;
            }
            else {
                this.state.Value = PurchasingInitializationState.Initialized;

                this.LoadPurchasesFromPrefs();
            }

            this.AssetNoIdDuplicates();
        }

        public void ClearPurchases() {
            this.purchases.Clear();
            this.SavePurchasesToPrefs();
            this.state.Invalidate();
        }

        public string GetPurchaseKeyByStoreSpecificId(string storeSpecificId) {
            foreach (var item in this.purchaseDefs.Items) {
                if (item.androidID == storeSpecificId || item.iosID == storeSpecificId) {
                    return item.key;
                }
            }

            Debug.LogError($"Purchase with store id {storeSpecificId} not exist");
            return null;
        }

        public bool HasProduct(string purchaseKey) {
            if (this.InitializationState != PurchasingInitializationState.Initialized) {
                return false;
            }

            if (!this.purchaseDefs.TryGet(purchaseKey, out _)) {
                return false;
            }

            return this.purchases.Contains(purchaseKey);
        }

        public string GetLocalizedPriceString(string purchaseKey) {
            if (this.InitializationState != PurchasingInitializationState.Initialized) {
                return "...";
            }

            if (!this.purchaseDefs.TryGet(purchaseKey, out var purchaseDef)) {
                return "...";
            }

            var usdPrice = purchaseDef.PriceUsd.ToString(CultureInfo.InvariantCulture);
            return $"${usdPrice}";
        }

        public (string, decimal) GetLocalizedPrice(string purchaseKey) {
            if (this.InitializationState != PurchasingInitializationState.Initialized) {
                return ("...", 0);
            }

            if (!this.purchaseDefs.TryGet(purchaseKey, out var purchaseDef)) {
                return ("...", 0);
            }

            var usdPrice = purchaseDef.PriceUsd;

            return ("USD", (decimal) usdPrice);
        }

        public async UniTask<PurchaseResult> Purchase(string purchaseKey) {
            if (this.InitializationState != PurchasingInitializationState.Initialized) {
                return PurchaseResult.Failed("Not initialized");
            }

            if (!this.purchaseDefs.TryGet(purchaseKey, out var purchaseDef)) {
                return PurchaseResult.Failed("Purchase not found");
            }

            await UniTask.Yield();

            var confirmed = await NativeDialog.OkCancel("Purchaings", $"Do you really want to purchase {purchaseDef.key}?", "Purchase", "Cancel");
            if (!confirmed) {
                return PurchaseResult.Cancelled;
            }

            if (purchaseDef.type == PurchaseDef.ProductType.NonConsumable ||
                purchaseDef.type == PurchaseDef.ProductType.Subscription) {
                if (this.purchases.Add(purchaseKey)) {
                    this.SavePurchasesToPrefs();
                }

                this.state.Invalidate();
            }

            return PurchaseResult.Succeed(purchaseDef, new SucceedPurchaseDetails(purchaseDef));
        }

        public async UniTask<PurchasesRestoreResult> RestorePurchases() {
            await UniTask.Yield();

            return PurchasesRestoreResult.Restored(this.purchases.Count);
        }

        private void AssetNoIdDuplicates() {
            var android = new HashSet<string>();
            var ios     = new HashSet<string>();

            foreach (var def in this.purchaseDefs.Items) {
                if (!android.Add(def.androidID)) {
                    Debug.LogError($"Purchase duplicate found - android - {def.androidID}");
                }

                if (!ios.Add(def.iosID)) {
                    Debug.LogError($"Purchase duplicate found - ios - {def.iosID}");
                }
            }
        }

        private void LoadPurchasesFromPrefs() =>
            this.purchases = PlayerPrefs.GetString("Multicast_DummyPurchasing_list").Split('&').ToHashSet();

        private void SavePurchasesToPrefs() =>
            PlayerPrefs.SetString("Multicast_DummyPurchasing_list", string.Join('&', this.purchases));

        [Serializable]
        public class Options {
            public bool failOnInitialization;
        }
    }
}