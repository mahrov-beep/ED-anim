namespace Game.Shared.UserProfile.Commands {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileSetNickNameCommand : IUserProfileServerCommand {
        [Key(0)] public string NewNickName;

        public static int MaxLength => 15;

        public static bool IsValidChar(char c) {
            // do not allow TextMeshPro tags as names
            if (c is '<' or '>') {
                return false;
            }

            return c == ' ' || char.IsLetterOrDigit(c) || char.IsPunctuation(c);
        }
    }
}