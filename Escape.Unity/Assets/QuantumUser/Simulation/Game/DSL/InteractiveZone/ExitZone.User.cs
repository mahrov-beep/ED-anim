namespace Quantum {
  public partial class EventExitZoneUsed {
    public GameSnapshot GameSnapshot;
  }

  public partial class Frame {
    public partial struct FrameEvents {
      public EventExitZoneUsed ExitZoneUsed(Frame f, int photonActorNr, GameSnapshot gameSnapshot) {
        var ev = f.Events.ExitZoneUsed(photonActorNr);
        if (ev != null) {
          ev.GameSnapshot = gameSnapshot;
        }

        return ev;
      }
    }
  }
}