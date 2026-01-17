namespace Multicast.Cheats {
    using System;
    using GameProperties;
    using JetBrains.Annotations;

    public interface ICheatGamePropertiesRegistry {
        [PublicAPI] public void Register(BoolGamePropertyName name);
        [PublicAPI] public void Register(IntGamePropertyName name);
        [PublicAPI] public void Register(FloatGamePropertyName name);

        [PublicAPI] public void Register(string name, Func<bool> getter, Action<bool> setter = null);
        [PublicAPI] public void Register(string name, Func<int> getter, Action<int> setter = null);
        [PublicAPI] public void Register(string name, Func<float> getter, Action<float> setter = null);
        [PublicAPI] public void Register(string name, Func<string> getter, Action<string> setter = null);
    }
}