using System.Collections.Generic;

namespace FrameworkAPI.Helpers;

/// <summary>
/// Helper to validate language keys.
/// </summary>
internal class ValidLanguageTagHelper
{
    private static readonly List<string> ValidLanguageTags = new()
    {
        "ar-EG", // arabic (egypt)
        "bg-BG", // bulgarian
        "zh-CN", // chinese (zhongwen, China)
        "da-DK", // danish
        "en-US", // english (USA)
        "de-DE", // german
        "fi-FI", // finnish
        "fr-FR", // french
        "el-GR", // greek
        "it-IT", // italian
        "ja-JP", // japanese
        "ko-KR", // korean
        "hr-HR", // croatian
        "lt-LT", // lithuanian
        "nl-NL", // dutch
        "nb-NO", // norwegian
        "pl-PL", // polish
        "pt-PT", // portuguese
        "ro-RO", // romanian
        "ru-RU", // russian
        "sv-SE", // swedish
        "sr-Latn-CS", // serbian
        "sl-SI", // slovenian
        "es-ES", // spanish
        "cs-CZ", // czech
        "tr-TR", // turkish
        "hu-HU" // hungarian
    };

    /// <summary>
    /// Returns if a language key is valid.
    /// </summary>
    public static bool IsLanguageTagValid(string languageTag)
    {
        return !string.IsNullOrWhiteSpace(languageTag) && ValidLanguageTags.Contains(languageTag);
    }
}