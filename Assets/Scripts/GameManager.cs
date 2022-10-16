using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Menu components")]
    [SerializeField] private Toggle volumeToggle;
    [SerializeField] private Toggle shootingType;
    [Space]
    [Header("Game components")]
    [Header("Dead screen")]
    [SerializeField] private GameObject deadScreen;
    [SerializeField] private TextMeshProUGUI priceChance;
    [Header("Result screen")]
    [SerializeField] private GameObject resultScreen;
    [SerializeField] private TextMeshProUGUI rewardText;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        PlayerPrefs.SetInt("CountBuyChance", 0);
        if (shootingType) shootingType.isOn = PlayerPrefsX.GetBool("AutoShooting");
        if (!volumeToggle) return;
        if (PlayerPrefs.HasKey("VolumeStatus")) PlayerPrefsX.SetBool("VolumeStatus", true);
        volumeToggle.isOn = PlayerPrefsX.GetBool("VolumeStatus");
        SetVolume(PlayerPrefsX.GetBool("VolumeStatus"));
    }

    public void ChangeScene(int index) => SceneManager.LoadScene(index);
    public void SetAutoShooting(bool status) => PlayerPrefsX.SetBool("AutoShooting", status);
    public void SetPause(bool status) => Time.timeScale = status ? 0 : 1;

    public static void SetVolume(bool status)
    {
        AudioListener.volume = status ? 1 : 0;
        PlayerPrefsX.SetBool("VolumeStatus", status);
    } 
    
    public void UpdatePriceChance()
    {
        var countChance = PlayerPrefs.GetInt("CountBuyChance");
        PlayerPrefs.SetInt("CountBuyChance", countChance + 1);
        priceChance.text = (PlayerPrefs.GetInt("CountBuyChance") * 5).ToString();
    }
    
    public void BuyChance()
    {
        var price = int.Parse(priceChance.text);
        var countGems = PlayerPrefs.GetInt("Gems");
        if (price > countGems) return;
        PlayerPrefs.SetInt("Gems", countGems - price);
        FindObjectOfType<PlayerController>().Renaissance(true);
        SetPause(false);
        deadScreen.SetActive(false);
    }

    public void ShowReward()
    {
        var countKills = FindObjectOfType<PlayerController>().CountKills;
        var reward = countKills * 5 * countKills;
        rewardText.text = "+"+reward;
        resultScreen.SetActive(true);
    }

    public void CollectReward(int menuIndex)
    {
        PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + int.Parse(rewardText.text));
        ChangeScene(menuIndex);
    }
}
