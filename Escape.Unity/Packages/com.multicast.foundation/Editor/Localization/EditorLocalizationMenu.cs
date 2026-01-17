namespace Multicast.Localization {
    using UnityEditor;
    using UnityEngine;

    internal static class EditorLocalizationMenu {
        private const string LANG_MENU = "Localization/Language/";

        // Begin Menu

        [MenuItem(LANG_MENU + "KEYS")]     private static void Keys()     => SetLang((SystemLanguage) (-1));
        [MenuItem(LANG_MENU + "English")]  private static void English()  => SetLang(SystemLanguage.English);
        [MenuItem(LANG_MENU + "Russian")]  private static void Russian()  => SetLang(SystemLanguage.Russian);
        [MenuItem(LANG_MENU + "German")]   private static void German()   => SetLang(SystemLanguage.German);
        [MenuItem(LANG_MENU + "French")]   private static void French()   => SetLang(SystemLanguage.French);
        [MenuItem(LANG_MENU + "Japanese")] private static void Japanese() => SetLang(SystemLanguage.Japanese);
        [MenuItem(LANG_MENU + "Korean")]   private static void Korean()   => SetLang(SystemLanguage.Korean);
        [MenuItem(LANG_MENU + "Spanish")]  private static void Spanish()  => SetLang(SystemLanguage.Spanish);

        // End Menu

        private static void SetLang(SystemLanguage lang) {
            Menu.SetChecked(LANG_MENU + LocalizationService.EditorLanguage, false);
            LocalizationService.EditorLanguage = lang;
            Menu.SetChecked(LANG_MENU + LocalizationService.EditorLanguage, true);

            if (Application.isPlaying) {
                LocalizationService.SelectedLang = LocalizationService.GetCodeBySystemLanguage(lang);
            }
        }
    }
}