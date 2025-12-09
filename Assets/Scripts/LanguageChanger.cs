
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


public class LanguageChanger : MonoBehaviour
{
    public void SetLanguage(string localeCode)
    {
        // localeCode: "en", "es", "pt".
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Debug.LogError("Locale no encontrada: " + localeCode);

        }


    }


}


