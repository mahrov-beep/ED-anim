namespace Multicast.GameProperties {
    using UserData;

    internal class UdGamePropertyData<T> : UdObject {
        public UdValue<T> Value { get; }

        public UdGamePropertyData(UdArgs args) : base(args) {
            this.Value = this.Child("value");
        }
    }
}