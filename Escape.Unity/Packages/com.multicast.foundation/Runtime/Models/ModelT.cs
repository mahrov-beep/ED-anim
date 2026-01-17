namespace Multicast {
    using JetBrains.Annotations;
    using UniMob;
    using UserData;

    public abstract class Model<TDef, TData> : Model
        where TDef : Def
        where TData : class, IDataObject {
        [PublicAPI] public TDef  Def  { get; }
        [PublicAPI] public TData Data { get; }

        [PublicAPI] public string Key => this.Def.key;

        protected Model(Lifetime lifetime, TDef def, TData data) : base(lifetime) {
            this.Def  = def;
            this.Data = data;
        }
    }
}