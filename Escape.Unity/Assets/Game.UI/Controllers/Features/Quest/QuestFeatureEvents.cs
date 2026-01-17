namespace Game.UI.Controllers.Features.Quest {
    using System;
    using Multicast;

    public static class QuestFeatureEvents {
        public static readonly EventSource Open  = new();
        public static readonly EventSource Close = new();

        public static readonly EventSource<RevealQuestArgs> RevealQuest = new();
        public static readonly EventSource<ClaimQuestArgs>  ClaimQuest  = new();
        public static readonly EventSource<DonateItemArgs>  DonateItem  = new();

        [Serializable, RequireFieldsInit]
        public struct RevealQuestArgs {
            public string questKey;
        }

        [Serializable, RequireFieldsInit]
        public struct ClaimQuestArgs {
            public string questKey;
        }

        [Serializable, RequireFieldsInit]
        public struct DonateItemArgs {
            public string questDonateItemTaskKey;
        }
    }
}