namespace Multicast.GameProperties {
    using JetBrains.Annotations;
    using UserData;

    public class UdGamePropertiesData : UdObject {
        internal UdDict<UdGamePropertyData<int>>   Ints     { get; }
        internal UdDict<UdGamePropertyData<bool>>  Booleans { get; }
        internal UdDict<UdGamePropertyData<float>> Floats   { get; }

        public UdGamePropertiesData(UdArgs args) : base(args) {
            this.Ints     = new UdDict<UdGamePropertyData<int>>(this.Child("ints"), a => new UdGamePropertyData<int>(a));
            this.Booleans = new UdDict<UdGamePropertyData<bool>>(this.Child("booleans"), a => new UdGamePropertyData<bool>(a));
            this.Floats   = new UdDict<UdGamePropertyData<float>>(this.Child("floats"), a => new UdGamePropertyData<float>(a));
        }

        [PublicAPI]
        public bool TryGetInt(IntGamePropertyName name, out int value) {
            if (this.Ints.TryGetValue(name.Name, out var data)) {
                value = data.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        [PublicAPI]
        public bool TryGetBool(BoolGamePropertyName name, out bool value) {
            if (this.Booleans.TryGetValue(name.Name, out var property)) {
                value = property.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        [PublicAPI]
        public bool TryGetFloat(FloatGamePropertyName name, out float value) {
            if (this.Floats.TryGetValue(name.Name, out var property)) {
                value = property.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        [PublicAPI]
        public void SetInt(IntGamePropertyName name, int value) {
            var property = this.Ints.GetOrCreate(name.Name, out _);
            property.Value.Value = value;
        }

        [PublicAPI]
        public void SetBool(BoolGamePropertyName name, bool value) {
            var property = this.Booleans.GetOrCreate(name.Name, out _);
            property.Value.Value = value;
        }

        [PublicAPI]
        public void SetFloat(FloatGamePropertyName name, float value) {
            var property = this.Floats.GetOrCreate(name.Name, out _);
            property.Value.Value = value;
        }

        [PublicAPI]
        public void IncrementInt(IntGamePropertyName name, int valueToAdd) {
            var property = this.Ints.GetOrCreate(name.Name, out _);
            property.Value.Value += valueToAdd;
        }
    }
}