namespace Game.Services.Graphics {
    using System;
    using UnityEngine;
   
    public class PlayerPrefsGraphicsSettingStorage : IGraphicsSettingStorage {
        public int ReadInt(string key, int fallback) {
            return PlayerPrefs.GetInt(key, fallback);
        }

        public void SaveInt(string key, int value) {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public T ReadEnum<T>(string key, T fallback) where T : struct, Enum {
            var storedString = PlayerPrefs.GetString(key, string.Empty);
            if (!string.IsNullOrEmpty(storedString) && Enum.TryParse<T>(storedString, out var parsed)) {
                return parsed;
            }

            var storedInt = PlayerPrefs.GetInt(key, Convert.ToInt32(fallback));
            if (Enum.IsDefined(typeof(T), storedInt)) {
                return (T)Enum.ToObject(typeof(T), storedInt);
            }

            return fallback;
        }

        public void SaveEnum<T>(string key, T value) where T : struct, Enum {
            PlayerPrefs.SetInt(key, Convert.ToInt32(value));
            PlayerPrefs.Save();
        }

        public string ReadString(string key, string fallback = "") {
            return PlayerPrefs.GetString(key, fallback);
        }

        public void SaveString(string key, string value) {
            PlayerPrefs.SetString(key, value ?? string.Empty);
            PlayerPrefs.Save();
        }
    }
}
