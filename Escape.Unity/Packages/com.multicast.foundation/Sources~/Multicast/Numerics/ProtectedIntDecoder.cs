namespace Multicast.Numerics {
    public class ProtectedIntDecoder {
        // Encode Key = 3172090000u; // bin: ‭1011_1101_0001_0010_0100_0000_1001_0000‬
        // Decode Key = 2049212705u; // bin:  ‭011_1101_0001_0010_0100_0000_1001_00001‬

        public static int Encode(int value, int constant) {
            value += constant;

            var val = (uint) value;
            for (var i = 0; i < 10; i++) {
                val = ((val & 1) == 0) ? (val >> 1) : ((val >> 1) ^ 3172090000u);
            }

            return (int) val;
        }

        public static int Decode(int value, int constant) {
            var val = (uint) value;
            for (var i = 0; i < 10; i++) {
                val = ((val & 2147483648u) == 0) ? (val << 1) : ((val << 1) ^ 2049212705u);
            }

            return (int) val - constant;
        }

        public static int GetChecksum(int value) {
            value += 286331153;

            for (var i = 1; i < 10; i++) {
                value = (int) ((value >> 1) ^ ((value & 1) * 2567483615u));
            }

            return value;
        }
    }
}