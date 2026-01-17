namespace Multicast.ServerData {
    public interface ISdObjectTracker {
        object Create(SdObjectBase owner);

        void OnRead(object obj);
        void OnWrite(object obj);

        void AssertOnWrite(object obj);
    }
}
