namespace Multicast.Advertising {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct AdImpressionEvent : IEventData {
        public AdType type;

        public string allData;
        public string adNetwork;
        public string placement;
        public string adUnit;
        public double revenue;
        public double ltv;

        public override string ToString() {
            return "Impression Data\n" +
                   $"All Data: {this.allData}\n" +
                   $"Ad Network: {this.adNetwork}\n" +
                   $"Placement: {this.placement}\n" +
                   $"AdUnit: {this.adUnit}\n" +
                   $"Revenue: {this.revenue}\n" +
                   $"Ltv: {this.ltv}";
        }
    }

    [Serializable]
    public struct RevenuePaidEvent : IEventData {
        public string AdUnitIdentifier   { get; set; }
        public string AdFormat           { get; set; }
        public string NetworkName        { get; set; }
        public string NetworkPlacement   { get; set; }
        public string Placement          { get; set; }
        public string CreativeIdentifier { get; set; }
        public double Revenue            { get; set; }
    }

    public enum AdType {
        Rewarded,
        Interstitial,
        AppOpen,
    }
}