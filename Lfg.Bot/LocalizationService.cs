using System.Globalization;
using System.Text.Json;

namespace LfgBot.Localization
{
    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations;

        public CultureInfo CurrentCulture { get; private set; }
        public string CurrentLanguage { get; private set; }

        public LocalizationService(CultureInfo cultureInfo)
        {
            CurrentCulture = cultureInfo;
            CurrentLanguage = cultureInfo.Name;
            _localizations = new Dictionary<string, Dictionary<string, string>>();

            LoadLocalizations();
        }

        private void LoadLocalizations()
        {
            string langFolder = "Lang";

            string enUsFilePath = Path.Combine(langFolder, "en-US.json");
            string ruRuFilePath = Path.Combine(langFolder, "ru-RU.json");
            string uaUAFilePath = Path.Combine(langFolder, "ua-UA.json");


            string enUsFileContent = File.ReadAllText(enUsFilePath);
            string ruRuFileContent = File.ReadAllText(ruRuFilePath);
            string uaUAFileContent = File.ReadAllText(uaUAFilePath);

            var enUsLocalization = JsonSerializer.Deserialize<Dictionary<string, string>>(enUsFileContent);
            var ruRuLocalization = JsonSerializer.Deserialize<Dictionary<string, string>>(ruRuFileContent);
            var uaUALocalization = JsonSerializer.Deserialize<Dictionary<string, string>>(uaUAFileContent);

            _localizations["en-US"] = enUsLocalization;
            _localizations["ru-RU"] = ruRuLocalization;
            _localizations["ua-UA"] = uaUALocalization;
        }

        public string GetLocalizedString(string key)
        {
            if (_localizations.TryGetValue(CurrentLanguage, out var currentLanguageLocalization) &&
                currentLanguageLocalization.TryGetValue(key, out var localizedString))
            {
                return localizedString;
            }

            return $"{key}";
        }

        public void SetCulture(CultureInfo cultureInfo)
        {
            CurrentCulture = cultureInfo;
            CurrentLanguage = cultureInfo.Name;
        }

        public void SetCurrentLanguage(string language)
        {
            CurrentLanguage = language;
        }
    }
}
