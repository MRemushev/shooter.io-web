using System;
using UnityEngine;

public class BtnSwitchLang: MonoBehaviour
{
    [SerializeField]
    private LocalizationManager localizationManager;

    private int _index;

    private void Start() => _index = localizationManager.languages.IndexOf(localizationManager.CurrentLanguage);

    public void OnButtonClick()
    {
        if (_index < localizationManager.languages.Count - 1) _index += 1;
        else _index = 0;
        localizationManager.CurrentLanguage = localizationManager.languages[_index];
    }
}