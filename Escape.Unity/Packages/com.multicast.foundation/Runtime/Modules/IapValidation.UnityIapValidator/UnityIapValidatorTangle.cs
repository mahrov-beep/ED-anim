namespace Multicast.Modules.IapValidation.UnityIapValidator {
    public class UnityIapValidatorTangle {
        public byte[] GooglePlayTangle { get; }
        public byte[] AppleTangle      { get; }

        public UnityIapValidatorTangle(byte[] googlePlayTangle, byte[] appleTangle) {
            this.GooglePlayTangle = googlePlayTangle;
            this.AppleTangle      = appleTangle;
        }
    }
}