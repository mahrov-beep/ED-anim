namespace Quantum {
  public partial class EventQuestCounterTaskDone {
    public QuestCounterPropertyTypes Property;
    public QuestTaskFilters[]        Filters;
    public int                       Value;
    public int?                      PhotonActorNr;
  }

  public partial class Frame {
    public partial struct FrameEvents {
      public EventQuestCounterTaskDone QuestCounterTaskDone(Frame f,
        int? photonActorNr,
        QuestCounterPropertyTypes property,
        int value,
        QuestTaskFilters[] filters
      ) {
        var ev = f.Events.QuestCounterTaskDone();
        if (ev != null) {
          ev.PhotonActorNr = photonActorNr;
          ev.Property      = property;
          ev.Value         = value;
          ev.Filters       = filters;
        }

        return ev;
      }
    }
  }
}