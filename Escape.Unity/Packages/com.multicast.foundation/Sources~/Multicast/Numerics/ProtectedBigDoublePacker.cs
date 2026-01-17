namespace Multicast.Numerics {
    using System.Runtime.InteropServices;

    internal static class ProtectedBigDoublePacker {
        private static int state = 688575;

        public static int Next() {
            var x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;

            return state = x;
        }

        public static void Pack(double d, bool randomize, out int a, out int b, out int c) {
            var x = randomize ? Next() : (d.GetHashCode() ^ 72493545);

            Box s = default;
            s.d = d;

            a = s.p2 ^ x;
            b = s.p2 ^ 34310595;
            c = s.p1 ^ x ^ 985321;
        }

        public static void Unpack(int a, int b, int c, out double d) {
            Box s = default;
            s.p1 = c ^ b ^ a ^ 34310595 ^ 985321;
            s.p2 = b ^ 34310595;

            d = s.d;
        }

        [StructLayout((LayoutKind.Explicit))]
        private struct Box {
            [FieldOffset(0)] public double d;
            [FieldOffset(0)] public int    p1;
            [FieldOffset(4)] public int    p2;
        }
    }
}