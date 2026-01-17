namespace Multicast.Install {
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterScriptableModulePlatformAttribute : Attribute {
        public string Platform { get; }

        public RegisterScriptableModulePlatformAttribute(string platform) {
            this.Platform = platform;
        }
    }
}