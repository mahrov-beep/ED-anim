namespace Multicast.Numerics {
    using System;

    public class ProtectedIntAccess {
        private static readonly Random Random = new Random();

        public static bool IsValid(ref int value, ref int constant, ref int checksum) {
            return GetChecksum(ref value, ref constant) == checksum;
        }

        private static int GetChecksum(ref int value, ref int constant) {
            return ProtectedIntDecoder.GetChecksum(value ^ constant);
        }

        public static int GetValue(ref int value, ref int constant, ref int checksum, bool throws) {
            return !throws || IsValid(ref value, ref constant, ref checksum)
                ? ProtectedIntDecoder.Decode(value, constant)
                : throw new ProtectedDataCorruptedException();
        }

        public static void SetValue(ref int value, ref int constant, ref int checksum, int newValue, bool force, bool randomize = true) {
            if (force || IsValid(ref value, ref constant, ref checksum)) {
                if (force || Random.NextDouble() < 0.2f) constant = randomize ? RndUtils.RandomValue : (newValue.GetHashCode() ^ 98321156);
                value    = ProtectedIntDecoder.Encode(newValue, constant);
                checksum = GetChecksum(ref value, ref constant);
            }
            else {
                checksum += Random.Next(-1000000, 1000000);
            }
        }

        private static class RndUtils {
            private static readonly DateTime UnixStart = new DateTime(1970, 1, 1);

            public static int RandomValue => (int) ((long) (DateTime.Now - UnixStart).TotalSeconds % 643728285);
        }
    }
}