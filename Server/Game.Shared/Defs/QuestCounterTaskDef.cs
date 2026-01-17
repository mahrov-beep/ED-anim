namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Quantum;

    [Serializable, DDEObject]
    public class QuestCounterTaskDef : Def {
        [DDE("quest"), DDEExternalKey("Quests")]
        public string quest;

        [DDE("property")]
        public QuestCounterPropertyTypes counterProperty;

        [DDE("value")]
        public int counterValue;

        [DDE("op")]
        public QuestPropertyOperation counterOperation;

        [DDE("filters", DDE.Empty)]
        public List<QuestTaskFilters> counterFilters;

        [DDE("reset_by_trigger", DDE.Empty)]
        public List<QuestCounterPropertyTypes> resetByTriggers;
    }
}