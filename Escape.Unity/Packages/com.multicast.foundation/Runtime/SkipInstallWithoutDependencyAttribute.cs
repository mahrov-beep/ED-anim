namespace Multicast {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class SkipInstallWithoutDependencyAttribute : Attribute {
        public Type Type { get; }

        public SkipInstallWithoutDependencyAttribute(Type type) {
            this.Type = type;
        }
    }
}