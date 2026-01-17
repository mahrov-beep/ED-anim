namespace Quantum.CellsInventory {
    using System.Buffers;
    using JetBrains.Annotations;

    public readonly ref struct CellsInventoryAccess {
        public readonly int    Width;
        public readonly int    Height;
        public readonly bool[] Array;

        public CellsInventoryAccess(int width, int height, bool[] array) {
            this.Width  = width;
            this.Height = height;
            this.Array  = array;
        }

        [PublicAPI]
        public static CellsInventoryAccess Rent((int width, int height) size) {
            return Rent(size.width, size.height);
        }

        [PublicAPI]
        public static CellsInventoryAccess Rent(int width, int height) {
            var array = ArrayPool<bool>.Shared.Rent(width * height);
            System.Array.Clear(array, 0, array.Length);
            return new CellsInventoryAccess(width, height, array);
        }

        [PublicAPI]
        public void Dispose() {
            ArrayPool<bool>.Shared.Return(this.Array, true);
        }

        [PublicAPI]
        public ref bool this[int i, int j] => ref this.Array[i * this.Width + j];
    }
}