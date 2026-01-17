namespace Multicast.Server {
    using System;

    public class ServerRequestException : Exception {
        public ServerRequestFailReason Reason           { get; }
        public string                  UserErrorMessage { get; set; }

        public ServerRequestException(ServerRequestFailReason reason, string message) : base(message) {
            this.Reason = reason;
        }
    }

    public enum ServerRequestFailReason {
        Unknown            = 0,
        NetworkError       = 1,
        UserError          = 2,
        ConfigurationError = 3,
    }
}