namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class QuestDonateItemTaskDef : Def {
        [DDE("quest"), DDEExternalKey("Quests")]
        public string quest;

        [DDE("item_setup"), DDEExternalKey("ItemSetups")]
        public string itemSetup;
    }
}