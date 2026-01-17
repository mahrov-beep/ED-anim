namespace Multicast.Server {
    public enum ServerAccessTokenState {
        Valid     = 0,
        Null      = 1,
        Malformed = 2,
        Expired   = 3,
    }
}