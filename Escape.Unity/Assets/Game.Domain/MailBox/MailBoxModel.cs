namespace Game.Domain.MailBox {
    using System.Linq;
    using Multicast;
    using Shared.UserProfile.Data;
    using UniMob;

    public class MailBoxModel : Model {
        [Inject] private SdUserProfile userProfile;

        public MailBoxModel(Lifetime lifetime) : base(lifetime) {
        }

        [Atom] public int Notify => this.userProfile.MailBox.Messages.Count(it => it.Viewed.Value == false || it.Claimed.Value == false);
    }
}