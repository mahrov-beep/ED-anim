namespace Multicast.DirtyDataEditor {
    using System;
    using System.Runtime.CompilerServices;
    using global::Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public ref struct DirtyDataStringBuffer {
        internal readonly ReadOnlySpan<char> Span;

        internal int Ptr;

        public DirtyDataStringBuffer(string s) : this(s.AsSpan()) {
        }

        public DirtyDataStringBuffer(ReadOnlySpan<char> span) {
            this.Span   = span;
            this.Ptr    = 0;
        }

        public override string ToString() {
            return new string(this.Span.ToArray());
        }
    }

    public static class DirtyDataStringBufferExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AtEnd(this ref DirtyDataStringBuffer self) {
            return self.Ptr >= self.Span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char Peek(this ref DirtyDataStringBuffer self) {
            return self.Span[self.Ptr];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ReadAnyChar(this ref DirtyDataStringBuffer self) {
            if (AtEnd(ref self)) {
                throw new DirtyDataParseException($"Expected any char but found end of string");
            }

            return self.Span[self.Ptr++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadChar(this ref DirtyDataStringBuffer self, char c) {
            if (AtEnd(ref self)) {
                throw new DirtyDataParseException($"Expected '{c}' but found end of string");
            }

            if (self.Span[self.Ptr] != c) {
                throw new DirtyDataParseException($"Expected '{c}' but found '{self.Span[self.Ptr]}'");
            }

            self.Ptr++;
        }

        public static void ReadString(this ref DirtyDataStringBuffer self, string s) {
            if (AtEnd(ref self)) {
                throw new DirtyDataParseException($"Expected '{s}' but found end of string");
            }

            for (var index = 0; index < s.Length; index++) {
                if (self.Span[self.Ptr + index] != s[index]) {
                    var found = new string(self.Span.Slice(self.Ptr, s.Length).ToArray());
                    throw new DirtyDataParseException($"Expected '{s}' but found '{found}'");
                }
            }

            self.Ptr += s.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipWhiteSpaces(this ref DirtyDataStringBuffer self) {
            while (self.Ptr < self.Span.Length && char.IsWhiteSpace(self.Span[self.Ptr])) {
                self.Ptr++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> Slice(this ref DirtyDataStringBuffer self, int startIndex, int len) {
            return self.Span.Slice(startIndex, len);
        }

        public static DirtyDataStringBuffer ReadLine(this ref DirtyDataStringBuffer self) {
            var readStart = self.Ptr;

            while (self.Ptr < self.Span.Length && self.Span[self.Ptr] != '\n') {
                self.Ptr++;
            }

            var lineBufferLen = self.Ptr - readStart;
            var lineBuffer    = new DirtyDataStringBuffer(self.Span.Slice(readStart, lineBufferLen));

            if (self.Ptr < self.Span.Length) {
                self.Ptr++; // skip '\n'
            }

            return lineBuffer;
        }

        public static DirtyDataStringBuffer ReadWhile(this ref DirtyDataStringBuffer self, Func<char, bool> func) {
            var readStart = self.Ptr;

            while (self.Ptr < self.Span.Length && func(self.Span[self.Ptr])) {
                self.Ptr++;
            }

            var bufferLen = self.Ptr - readStart;
            return new DirtyDataStringBuffer(self.Span.Slice(readStart, bufferLen));
        }
    }
}