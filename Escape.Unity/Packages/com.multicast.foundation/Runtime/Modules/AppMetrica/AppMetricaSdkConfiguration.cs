namespace Multicast.Modules.AppMetrica {
    using System;
    using UnityEngine;

    [Serializable]
    public class AppMetricaSdkConfiguration {
        [SerializeField] private bool needToSendPurchases;
        
        public bool NeedToSendPurchases => this.needToSendPurchases;
    }
}
