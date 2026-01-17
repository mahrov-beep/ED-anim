namespace Multicast.DropSystem {
    using System;

    public class NoDropException : ApplicationException {
        public NoDropException(DropDef def)
            : base($"No drop at '{def.GetType().Name}'") {
        }
    }
}