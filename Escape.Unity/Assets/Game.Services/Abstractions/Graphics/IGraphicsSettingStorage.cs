namespace Game.Services.Graphics {
    using System;

    public interface IGraphicsSettingStorage {
        int ReadInt(string key, int fallback);
        void SaveInt(string key, int value);

        T ReadEnum<T>(string key, T fallback) where T : struct, Enum;
        void SaveEnum<T>(string key, T value) where T : struct, Enum;

        string ReadString(string key, string fallback = "");
        void SaveString(string key, string value);
    }
}
