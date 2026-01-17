namespace Game.Domain.Gunsmiths {
    using Multicast;
    using Multicast.Collections;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Gunsmiths;
    using UniMob;

    public class GunsmithsModel : KeyedSingleInstanceModel<GunsmithDef, SdGunsmith, GunsmithModel> {
        public GunsmithsModel(Lifetime lifetime, LookupCollection<GunsmithDef> defs, SdUserProfile userProfile)
            : base(lifetime, defs, userProfile.Gunsmiths.Lookup) {
            this.AutoConfigureData = true;
        }
    }

    public class GunsmithModel : Model<GunsmithDef, SdGunsmith> {
        public GunsmithModel(Lifetime lifetime, GunsmithDef def, SdGunsmith data) : base(lifetime, def, data) {
        }
    }
}