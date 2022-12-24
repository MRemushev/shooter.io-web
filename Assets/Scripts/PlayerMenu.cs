using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
	[Header("Player")]
	[SerializeField] private WeaponSwitch weapons;
	[SerializeField] private SkinnedMeshRenderer skinObject;
	[SerializeField] private TextureList skinArray;
	[Header("Menu")]
	[SerializeField] private TextMeshProUGUI coinsText;
	[SerializeField] private TextMeshProUGUI gemsText;
	[SerializeField] private GameObject battleButton;
	[SerializeField] private GameObject buySkinButton;
	[SerializeField] private TextMeshProUGUI bestCountText;
	[Header("Skin shop")]
	[SerializeField] private TextMeshProUGUI priceSkinText;
	[SerializeField] private Image priceSkinIcon;
	[SerializeField] private Sprite[] priceSkinIcons;
	[Header("People shop")]
	[SerializeField] private Button peopleButton;
	[SerializeField] private TextMeshProUGUI peopleLevelText;
	[SerializeField] private TextMeshProUGUI peoplePriceText;
	[SerializeField] [Tooltip("Set 0 to unlimited")] private int peopleMaxLevel;
	[Header("Health shop")]
	[SerializeField] private Button healthButton;
	[SerializeField] private TextMeshProUGUI healthLevelText;
	[SerializeField] private TextMeshProUGUI healthPriceText;
	[SerializeField] [Tooltip("Set 0 to unlimited")] private int healthMaxLevel;
	[Header("Weapon shop")]
	[SerializeField] private Button weaponButton;
	[SerializeField] private TextMeshProUGUI weaponLevelText;
	[SerializeField] private TextMeshProUGUI weaponPriceText;
	[SerializeField] [Tooltip("Set 0 to unlimited")] private int weaponMaxLevel;

	public Skin[] skinsInfo;

	private bool[] _stockCheck;
	private int _skinIndex;
	private int _countCoins;
	private int _countGems;

	private void Awake()
	{
		_stockCheck = new bool[skinsInfo.Length];
		if (PlayerPrefs.HasKey("StockArray")) _stockCheck = PlayerPrefsX.GetBoolArray("StockArray");
		else _stockCheck[0] = true; _stockCheck[1] = true;
		for (var i = 0; i < skinsInfo.Length; i++) skinsInfo[i].inStock = _stockCheck[i];
		_skinIndex = PlayerPrefs.GetInt("PlayerSkin");
		_countCoins = PlayerPrefs.GetInt("Coins");
		_countGems = PlayerPrefs.GetInt("Gems");
		skinObject.material.mainTexture = skinArray.textureList[_skinIndex];
		weapons.ChangeWeapon(PlayerPrefs.GetInt("WeaponLevel"));
		bestCountText.text = PlayerPrefs.GetInt("HighScore").ToString();
		ChangeCoinText();
		ChangeGemText();
		UpdatePricePeople();
		UpdatePriceHealth();
		UpdatePriceWeapon();
	}

	public void ChangeCoinText() => coinsText.text = PlayerPrefs.GetInt("Coins").ToString();
	public void ChangeGemText() => gemsText.text = PlayerPrefs.GetInt("Gems").ToString();

	private void CheckSkin()
	{
		skinObject.material.mainTexture = skinArray.textureList[_skinIndex];
		if (skinsInfo[_skinIndex].inStock)
		{
			PlayerPrefs.SetInt("PlayerSkin", _skinIndex);
			battleButton.SetActive(true);
			buySkinButton.gameObject.SetActive(false);
		}
		else
		{
			priceSkinText.text = skinsInfo[_skinIndex].price.ToString();
			priceSkinIcon.sprite = skinsInfo[_skinIndex].gemPrice ? priceSkinIcons[1] : priceSkinIcons[0];
			buySkinButton.gameObject.SetActive(true);
			battleButton.SetActive(false);
		}
	}

	public void NextSkin()
	{
		if (_skinIndex >= skinArray.textureList.Length - 1)
		{
			_skinIndex = 0;
			battleButton.SetActive(true);
			buySkinButton.gameObject.SetActive(false);
			CheckSkin();
		}
		else
		{
			_skinIndex += 1;
			CheckSkin();
		}
	}

	public void PreviousSkin()
	{
		if (_skinIndex <= 0)
		{
			_skinIndex = skinArray.textureList.Length - 1;
			CheckSkin();
		}
		else
		{
			_skinIndex -= 1;
			CheckSkin();
		}
	}

	public void BuySkin()
	{
		if (!skinsInfo[_skinIndex].gemPrice)
		{
			if (_countCoins < skinsInfo[_skinIndex].price) return;
			_countCoins -= skinsInfo[_skinIndex].price;
			PlayerPrefs.SetInt("Coins", _countCoins);
			coinsText.text = PlayerPrefs.GetInt("Coins").ToString();
			PurchaseSkin();
		}
		else
		{
			if (_countGems < skinsInfo[_skinIndex].price) return;
			_countGems -= skinsInfo[_skinIndex].price;
			PlayerPrefs.SetInt("Gems", _countGems);
			gemsText.text = PlayerPrefs.GetInt("Gems").ToString();
			PurchaseSkin();
		}
	}

	private void PurchaseSkin()
	{
		_stockCheck[_skinIndex] = true;
		skinsInfo[_skinIndex].inStock = true;
		battleButton.SetActive(true);
		buySkinButton.gameObject.SetActive(false);
		PlayerPrefsX.SetBoolArray("StockArray", _stockCheck);
		PlayerPrefs.SetInt("PlayerSkin", _skinIndex);
	}

	// Buy people functions
	private void UpdatePricePeople()
	{
		var countPeople = PlayerPrefs.GetInt("PlayerPeople") + 1;
		if (peopleMaxLevel != 0 && countPeople > peopleMaxLevel)
		{
			peopleButton.interactable = false;
			peopleLevelText.text = PlayerPrefs.GetInt("PlayerPeople").ToString();
			peoplePriceText.text = "Max";
			return;
		}
		peopleLevelText.text = PlayerPrefs.GetInt("PlayerPeople") +" > "+ countPeople;
		peoplePriceText.text = (countPeople * 100 * countPeople).ToString();
	}

	public void BuyPeople()
	{
		var unitPrice = int.Parse(peoplePriceText.text);
		if (PlayerPrefs.GetInt("Coins") < unitPrice) return;
		PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - unitPrice);
		PlayerPrefs.SetInt("PlayerPeople", PlayerPrefs.GetInt("PlayerPeople") + 1);
		coinsText.text = PlayerPrefs.GetInt("Coins").ToString();
		UpdatePricePeople();
	}

	private void UpdatePriceHealth()
	{
		var countHealth = PlayerPrefs.GetInt("PlayerHealth") + 1;
		if (healthMaxLevel != 0 && countHealth > healthMaxLevel)
		{
			healthButton.interactable = false;
			healthLevelText.text = (100 + PlayerPrefs.GetInt("PlayerHealth") * 10).ToString();
			healthPriceText.text = "Max";
			return;
		}
		healthLevelText.text = 100 + PlayerPrefs.GetInt("PlayerHealth") * 10 +" > "+ (100 + countHealth * 10);
		healthPriceText.text = (countHealth * 500 * countHealth).ToString();
	}

	public void BuyHealth()
	{
		var healthPrice = int.Parse(healthPriceText.text);
		if (PlayerPrefs.GetInt("Coins") < healthPrice) return;
		PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - healthPrice);
		PlayerPrefs.SetInt("PlayerHealth", PlayerPrefs.GetInt("PlayerHealth") + 1);
		coinsText.text = PlayerPrefs.GetInt("Coins").ToString();
		UpdatePriceHealth();
	}

	private void UpdatePriceWeapon()
	{
		var weaponLevel = weapons.WeaponLevel + 1;
		if (weaponMaxLevel != 0 && weaponLevel >= weaponMaxLevel || weaponLevel > weapons.transform.childCount - 1)
		{
			weaponButton.interactable = false;
			weaponLevelText.text = weapons.weaponsName[weaponLevel - 1];
			weaponPriceText.text = "Max";
			return;
		}
		weaponLevelText.text = weapons.weaponsName[weaponLevel - 1] +" > "+ weapons.weaponsName[weaponLevel];
		weaponPriceText.text = (weaponLevel * 1000 * weaponLevel).ToString();
	}

	public void BuyWeapon()
	{
		var weaponPrice = int.Parse(weaponPriceText.text);
		if (PlayerPrefs.GetInt("Coins") < weaponPrice) return;
		PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - weaponPrice);
		PlayerPrefs.SetInt("WeaponLevel", PlayerPrefs.GetInt("WeaponLevel") + 1);
		coinsText.text = PlayerPrefs.GetInt("Coins").ToString();
		weapons.ChangeWeapon(PlayerPrefs.GetInt("WeaponLevel"));
		UpdatePriceWeapon();
	}
}

[System.Serializable]
public class Skin
{
	public int price;
	public bool gemPrice;
	public bool inStock;
}
