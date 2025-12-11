using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using System;

public class LanguageChanger : MonoBehaviour
{
    public void ChangeLanguage(string localeCode)
    {
        try
        {
            //encontrar la configuración regional(Locale)
            Locale targetLocale = GetLocale(localeCode);

            if (targetLocale != null)
            {
                // 2. Intentamos establecer la configuración regional
                StartCoroutine(SetLocale(targetLocale));
            }
            else
            {
                // Si la búsqueda falla, lanzamos una excepción con mensaje concatenado
                string errorMessage = "No se encontró la configuración regional (Locale) para el código: " + localeCode + ".";
                throw new InvalidOperationException(errorMessage);
            }
        }
        catch (InvalidOperationException e)
        {
            // 3. Capturamos una excepción específica (ej. código no válido)
            Debug.LogError("[ERROR DE IDIOMA] Fallo al cambiar el idioma: " + e.Message);
        }
        catch (Exception e)
        {
            // 4. Capturamos cualquier otra excepción imprevista
            Debug.LogError("[ERROR FATAL DE LOCALIZACIÓN] Se capturó una excepción inesperada: " + e.Message);
        }
    }

    private Locale GetLocale(string localeCode)
    {
        if (LocalizationSettings.AvailableLocales == null)
            return null;

        // Busca el Locale por el código (ej. "en", "es")
        return LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }

    private IEnumerator SetLocale(Locale locale)
    {
        yield return LocalizationSettings.InitializationOperation;

        // Establece el Locale
        LocalizationSettings.SelectedLocale = locale;
        Debug.Log("Idioma cambiado a: " + locale.LocaleName);
    }
}


