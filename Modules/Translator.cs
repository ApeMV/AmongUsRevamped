using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace AmongUsRevamped;

public static class Translator
{
    public enum SupportedLangs
    {
        English
    }

    private static readonly Dictionary<SupportedLangs, Dictionary<string, string>> _languages = new();
    private static SupportedLangs _currentLang = SupportedLangs.English;
    private static string LanguageFolder => $"{BanManager.DataPath}/Language";

    public static void Init()
    {
        CheckLanguageFile(_currentLang);
        LoadLanguage(_currentLang);
    }

    private static void CheckLanguageFile(SupportedLangs lang)
    {
        if (!Directory.Exists(LanguageFolder)) Directory.CreateDirectory(LanguageFolder);

        string path = GetLanguagePath(lang);
        var assembly = Assembly.GetExecutingAssembly();
        string targetFile = $"{lang}.json";

        Dictionary<string, string> embeddedDict = new();
        Dictionary<string, string> diskDict = new();

        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.EndsWith(targetFile, StringComparison.OrdinalIgnoreCase))
            {
                using var stream = assembly.GetManifestResourceStream(name);
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                embeddedDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                break;
            }
        }

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                diskDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch
            {
                diskDict = new Dictionary<string, string>();
            }
        }

        bool updated = false;
        foreach (var kvp in embeddedDict)
        {
            if (!diskDict.ContainsKey(kvp.Key))
            {
                diskDict[kvp.Key] = kvp.Value;
                updated = true;
            }
        }

        if (!File.Exists(path) || updated)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(diskDict, options));
        }
    }

    private static void LoadLanguage(SupportedLangs lang)
    {
        string path = GetLanguagePath(lang);

        if (!File.Exists(path)) return;

        try
        {
            string json = File.ReadAllText(path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (dict != null) _languages[lang] = dict;
        }
        catch
        {
            _languages[lang] = new Dictionary<string, string>();
        }
    }

    private static string GetLanguagePath(SupportedLangs lang)
    {
        return $"{LanguageFolder}/{lang}.json";
    }

    public static string Get(string key)
    {
        if (_languages.TryGetValue(_currentLang, out var dict) && dict.TryGetValue(key, out var value))
        {
            return SendChatPatch.ConvertNum(value);
        }

        return $"<MISSING:{key}>";
    }
    public static string Get(string key, params object[] args)
    {
        if (_languages.TryGetValue(_currentLang, out var dict) && dict.TryGetValue(key, out var value))
        {
            return SendChatPatch.ConvertNum(string.Format(value, args));
        }
        return $"<MISSING:{key}>";
    }

    public static void Reload()
    {
        LoadLanguage(_currentLang);
    }
}