// ReSharper disable InconsistentNaming

namespace Quantum {
    using System;

    [Serializable]
    public struct CellsRange {
        public static CellsRange Empty => new CellsRange();

        public readonly int  J; // индекс по горизонтали
        public readonly int  I; // индекс по вертикали
        public readonly int  Width;
        public readonly int  Height;
        public readonly bool Rotated;

        public int MinJ => this.J;
        public int MinI => this.I;
        public int MaxJ => this.J + this.Width - 1;
        public int MaxI => this.I + this.Height - 1;

        private CellsRange(int i, int j, int width, int height, bool rotated) {
            this.I       = i;
            this.J       = j;
            this.Width   = width;
            this.Height  = height;
            this.Rotated = rotated;
        }

        public static CellsRange FromIJWH(int i, int j, int width, int height, bool rotated) {
            return new CellsRange(i: i, j: j, width: width, height: height, rotated: rotated);
        }

        public bool Contains(int i, int j) {
            return i >= this.MinI && i <= this.MaxI &&
                   j >= this.MinJ && j <= this.MaxJ;
        }

        public CellsRange GetRotated() {
            return new CellsRange(
                i: this.I,
                j: this.J,
                width: this.Height,
                height: this.Width,
                rotated: !this.Rotated
            );
        }

        public CellsRange GetMovedIJ(int moveI, int moveJ) {
            return new CellsRange(
                i: this.I + moveI,
                j: this.J + moveJ,
                width: this.Width,
                height: this.Height,
                rotated: this.Rotated
            );
        }

        public CellsRange WithIJ(int i, int j) {
            return new CellsRange(
                i: i,
                j: j,
                width: this.Width,
                height: this.Height,
                rotated: this.Rotated
            );
        }

        public override string ToString() {
            return $"[I={this.I}, J={this.J}, Width={this.Width}, Height={this.Height}]";
        }
    }
}