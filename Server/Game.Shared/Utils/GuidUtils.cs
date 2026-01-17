namespace Game.Shared.Utils {
    using System;
    using System.Text;

    public static class GuidUtils {
        public static string ToCompactString(Guid guid) {
            var bytes = ToByteArrayMatchingStringRepresentation(guid);
            return new StringBuilder(Convert.ToBase64String(bytes))
                .Replace('+', '-')
                .Replace('/', '_')
                .ToString();
        }

        public static Guid FromByteArrayMatchingStringRepresentation(byte[] bytes) {
            TweakOrderOfGuidBytesToMatchStringRepresentation(bytes);
            var guid = new Guid(bytes);
            TweakOrderOfGuidBytesToMatchStringRepresentation(bytes);
            return guid;
        }

        public static byte[] ToByteArrayMatchingStringRepresentation(Guid guid) {
            var bytes = guid.ToByteArray();
            TweakOrderOfGuidBytesToMatchStringRepresentation(bytes);
            return bytes;
        }

        private static void TweakOrderOfGuidBytesToMatchStringRepresentation(byte[] guidBytes) {
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(guidBytes, 0, 4);
                Array.Reverse(guidBytes, 4, 2);
                Array.Reverse(guidBytes, 6, 2);
            }
        }
    }
}