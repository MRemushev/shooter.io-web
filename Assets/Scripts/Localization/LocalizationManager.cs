using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    private string _currentLanguage;
    public List<string> languages;
    private Dictionary<string, string> _localizedText;
    private static bool _isReady;

	public delegate void ChangeLangText();
    public event ChangeLangText OnLanguageChanged;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Language"))
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian or SystemLanguage.Ukrainian or SystemLanguage.Belarusian:
                    PlayerPrefs.SetString("Language", languages[1]);
                    break;
                case SystemLanguage.Spanish:
                    PlayerPrefs.SetString("Language", languages[2]);
                    break;
                case SystemLanguage.Turkish:
                    PlayerPrefs.SetString("Language", languages[3]);
                    break;
                default:
                    PlayerPrefs.SetString("Language", languages[0]);
                    break;
            }
        }
        _currentLanguage = PlayerPrefs.GetString("Language");
        LoadLocalizedText(_currentLanguage);
    }

    private void LoadLocalizedText(string langName)
    {
        var path = Application.streamingAssetsPath + "/Languages/" + langName + ".json";
        string dataAsJson;
        if (Application.platform == RuntimePlatform.Android)
        {
            var reader = new WWW(path);
            while (!reader.isDone) { }
            dataAsJson = reader.text;
        }
        else dataAsJson = File.ReadAllText(path);
        
        var loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

        _localizedText = new Dictionary<string, string>();
        foreach (var t in loadedData.items) _localizedText.Add(t.key, t.value);
        
        PlayerPrefs.SetString("Language", langName);
        _currentLanguage = PlayerPrefs.GetString("Language");
        _isReady = true;

        OnLanguageChanged?.Invoke();
    }

    public string GetLocalizedValue(string key)
    {
        if (_localizedText.ContainsKey(key)) return _localizedText[key];
        throw new Exception("Localized text with key \"" + key + "\" not found");
    }

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set => LoadLocalizedText(value);
    }
    public bool IsReady => _isReady;
}