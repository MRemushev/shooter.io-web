using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class GameManager : MonoBehaviour
{
	[Header("Game settings")] [SerializeField] private int frameRate = 60;
	[Header("Menu components")] [SerializeField] private Toggle volumeToggle;
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
		Application.targetFrameRate = frameRate;
		PlayerPrefs.SetInt("CountBuyChance", 0);
		if (!PlayerPrefs.HasKey("VolumeStatus")) PlayerPrefsX.SetBool("VolumeStatus", false);
		if (!volumeToggle) return;
		volumeToggle.isOn = PlayerPrefsX.GetBool("VolumeStatus");
		SetVolume(PlayerPrefsX.GetBool("VolumeStatus"));
	}

	private void OnEnable()
	{
		YandexGame.RewardVideoEvent += AdChance;
		YandexGame.RewardVideoEvent += DoubleReward;
	}

	private void OnDisable()
	{
		YandexGame.RewardVideoEvent -= AdChance;
		YandexGame.RewardVideoEvent -= DoubleReward;
	}

	public void AsyncChangeScene(int index) => SceneManager.LoadSceneAsync(index);
	private static void ChangeScene(int index) => SceneManager.LoadScene(index);

	public void SetPause(bool status) => Time.timeScale = status ? 0 : 1;

	public static void SetVolume(bool status)
	{
		AudioListener.volume = status ? 0 : 1;
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
		FindObjectOfType<PlayerController>().Renaissance();
		SetPause(false);
		deadScreen.SetActive(false);
	}

	public void AdChance(int idAd = 0)
	{
		if (idAd != 1) return;
		FindObjectOfType<PlayerController>().Renaissance();
		SetPause(false);
		deadScreen.SetActive(false);
	}

	public void ShowReward()
	{
		var countKills = FindObjectOfType<PlayerController>().ScoreKills;
		var reward = 100 * countKills * countKills;
		rewardText.text = "+" + reward;
		resultScreen.SetActive(true);
	}

	public void CollectReward(int menuIndex)
	{
		PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + int.Parse(rewardText.text));
		ChangeScene(menuIndex);
	}

	private void DoubleReward(int idAd)
	{
		if (idAd != 2) return;
		rewardText.text = "+" + int.Parse(rewardText.text) * 2;
	}
}
