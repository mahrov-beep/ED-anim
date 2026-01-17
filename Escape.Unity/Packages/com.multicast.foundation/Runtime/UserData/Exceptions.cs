namespace Multicast.UserData {
    using System;

    public class UdDuplicatedPropertyKeyException : Exception {
        public UdDuplicatedPropertyKeyException(string message) : base(message) {
        }
    }
}