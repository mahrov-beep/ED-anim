namespace Game.ServerRunner;

public class OrleansConstants {
    public static class Streams {
        public const string SERVER_EVENTS = "server-events";

        public static class Ids {
            public static StreamId AppServerEventsForUser(Guid userId)   => StreamId.Create("app", userId);
            public static StreamId GameServerEventForGame(string gameId) => StreamId.Create("game", gameId);
        }
    }
}