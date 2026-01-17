namespace Multicast.Modules.Advertising.MaxSdk {
    using UnityEngine;

    public interface IMaxSdkAdUnitOverride {
        string AdUnit { get; }

        void OnAdClosed();
    }

    public class MaxSdkRotatedAdUnitOverride : IMaxSdkAdUnitOverride {
        private readonly string[] adUnits;

        private int adUnitIndex;

        public MaxSdkRotatedAdUnitOverride(string[] adUnits) {
            this.adUnits     = adUnits;
            this.adUnitIndex = Random.Range(0, adUnits.Length);
        }

        public string AdUnit => this.adUnits[this.adUnitIndex];

        public void OnAdClosed() {
            this.adUnitIndex++;

            if (this.adUnitIndex >= this.adUnits.Length) {
                this.adUnitIndex = 0;
            }
        }
    }
}