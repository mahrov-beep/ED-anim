namespace Game.Shared.UserProfile.Commands.Game {
    using MessagePack;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileApplyGameResultsCommand : IUserProfileServerCommand {
        [Key(0)] public string       GameId;
        [Key(1)] public GameSnapshot GameSnapshot;
    }
}