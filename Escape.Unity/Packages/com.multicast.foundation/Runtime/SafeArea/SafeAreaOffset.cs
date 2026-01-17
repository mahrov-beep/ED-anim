namespace Multicast.SafeArea {
    using System;

    [Serializable]
    public class SafeAreaOffset {
        public SafeAreaOffset() {
        }

        public SafeAreaOffset(float left, float right, float top, float bottom) {
            this.left   = left;
            this.right  = right;
            this.top    = top;
            this.bottom = bottom;
        }

        public float left;
        public float right;
        public float top;
        public float bottom;
        public float Horizontal => this.left + this.right;
        public float Vertical   => this.top + this.bottom;
    }
}