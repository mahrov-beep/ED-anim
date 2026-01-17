namespace Multicast {
    using Advertising;
    using Analytics;
    using FeatureToggles;
    using GameProperties;
    using JetBrains.Annotations;
    using Notifications;
    using UniMob;

    public partial class App {
        [PublicAPI] public static Lifetime                      Lifetime                   => Get<Lifetime>();
        [PublicAPI] public static IAnalytics                    Analytics                  => Get<IAnalytics>();
        [PublicAPI] public static IAdvertising                  Advertising                => Get<IAdvertising>();
        [PublicAPI] public static ITimeService                  Time                       => Get<ITimeService>();
        [PublicAPI] public static AppSharedFormulaContext       SharedFormulaContext       => Get<AppSharedFormulaContext>();
        [PublicAPI] public static AppSharedNumberFormulaContext SharedNumberFormulaContext => Get<AppSharedNumberFormulaContext>();
        [PublicAPI] public static GamePropertiesModel           GameProperties             => Get<GamePropertiesModel>();
        [PublicAPI] public static FeatureTogglesModel           FeatureToggles             => Get<FeatureTogglesModel>();
        [PublicAPI] public static IGameNotifications            Notifications              => Get<IGameNotifications>();

        internal static IUserDataService UserDataService => Get<IUserDataService>();
    }
}