namespace Multicast.Server {
    using System;
    using Multicast.ServerData;
    using UniMob;

    public class SdObjectAtomTracker : ISdObjectTracker {
        public object Create(SdObjectBase owner) {
            var atom = (Atom<int>) Atom.Value(0);
            return atom;
        }

        public void OnRead(object obj) {
            var atom = (Atom<int>) obj;
            atom.Get();
        }

        public void OnWrite(object obj) {
            var atom = (Atom<int>) obj;
            atom.Invalidate();
        }

        public void AssertOnWrite(object obj) {
            if (Atom.CurrentScope != null) {
                throw new InvalidOperationException("SdObject cannot be modified in Atom's watched scope");
            }
        }
    }
}