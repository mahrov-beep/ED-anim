namespace Multicast {
    using System;

    public class ModelAccessNotAllowedException : Exception {
        public ModelAccessNotAllowedException(string message) : base(message) {
        }
    }
}