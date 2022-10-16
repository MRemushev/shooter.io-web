using System;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    private string _key;
    private LocalizationManager _localizationManager;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        if (!_localizationManager) _localizationManager = FindObjectOfType<LocalizationManager>();
        if (!_text) _text = GetComponent<TextMeshProUGUI>();
        _key = _text.text;
        _localizationManager.OnLanguageChanged += UpdateText;
    }

    private void OnEnable() => UpdateText();

    private void OnDestroy() => _localizationManager.OnLanguageChanged -= UpdateText;
    
    public virtual void UpdateText()
    {
        if (!gameObject) return;
        if (!_localizationManager) _localizationManager = FindObjectOfType<LocalizationManager>();
        if (!_text) _text = GetComponent<TextMeshProUGUI>();
        _text.text = _localizationManager.GetLocalizedValue(_key);
    }
}