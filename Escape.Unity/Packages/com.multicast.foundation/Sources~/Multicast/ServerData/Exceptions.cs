using System;

namespace Multicast.ServerData {
    public class SdDuplicatedPropertyKeyException : Exception {
        public SdDuplicatedPropertyKeyException(string message) : base(message) {
        }
    }
}