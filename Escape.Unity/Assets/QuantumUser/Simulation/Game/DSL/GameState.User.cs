namespace Quantum {
  public partial class EventGameExit {
    public GameSnapshot GameSnapshot;
  }

  public partial class EventGameLost {
    public GameSnapshot GameSnapshot;
  }

  public partial class Frame {
    public partial struct FrameEvents {
      public EventGameExit GameExit(Frame f, GameSnapshot gameSnapshot) {
        var ev = f.Events.GameExit();
        if (ev != null) {
          ev.GameSnapshot = gameSnapshot;
        }

        return ev;
      }

      public EventGameLost GameLost(Frame f, int photonActorNr, GameSnapshot gameSnapshot) {
        var ev = f.Events.GameLost(photonActorNr);
        if (ev != null) {
          ev.GameSnapshot = gameSnapshot;
        }

        return ev;
      }
    }
  }
}