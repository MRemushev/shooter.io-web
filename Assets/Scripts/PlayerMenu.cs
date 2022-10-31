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
    [SerializeField] private TMP_InputField inputName;
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
    [SerializeField] private TextMeshProUGUI peopleTitleText;
    [SerializeField] private TextMeshProUGUI peoplePriceText;
    [Header("Health shop")]
    [SerializeField] private TextMeshProUGUI healthTitleText;
    [SerializeField] private TextMeshProUGUI healthPriceText;
    [Header("Weapon shop")] 
    [SerializeField] private Button weaponButton;
    [SerializeField] private TextMeshProUGUI weaponTitleText;
    [SerializeField] private TextMeshProUGUI weaponPriceText;
    
    public Skin[] skinsInfo;

    private bool[] _stockCheck;
    private int _skinIndex;
    private int _countCoins;
    private int _countGems;

    private void Start()
    {
        // PlayerPrefs.DeleteAll();
#if UNITY_EDITOR
        PlayerPrefs.SetInt("Coins", 1000000);
        if (!PlayerPrefs.HasKey("Gems")) PlayerPrefs.SetInt("Gems", 1000);
#endif
        if (!PlayerPrefs.HasKey("PlayerName")) PlayerPrefs.SetString("PlayerName", "Player");
        if (bestCountText) bestCountText.text = PlayerPrefs.GetInt("HighScore").ToString();
        if (inputName) inputName.text = PlayerPrefs.GetString("PlayerName");
        if (peopleTitleText && peoplePriceText) UpdatePricePeople();
        if (healthTitleText && healthPriceText) UpdatePriceHealth();
        if (healthTitleText && healthPriceText) UpdatePriceWeapon();
        if (coinsText) ChangeCoinText();
        if (gemsText) ChangeGemText();
        _stockCheck = new bool[skinsInfo.Length];
        if (PlayerPrefs.HasKey("StockArray")) _stockCheck = PlayerPrefsX.GetBoolArray("StockArray");
        else _stockCheck[0] = true; _stockCheck[1] = true;
        for (var i = 0; i < skinsInfo.Length; i++) skinsInfo[i].inStock = _stockCheck[i];
        _skinIndex = PlayerPrefs.GetInt("PlayerSkin");
        _countCoins = PlayerPrefs.GetInt("Coins");
        _countGems = PlayerPrefs.GetInt("Gems");
        skinObject.material.mainTexture = skinArray.textureList[_skinIndex];
        weapons.ChangeWeapon(PlayerPrefs.GetInt("WeaponLevel"));
    }

    public void ChangeCoinText() => coinsText.text = PlayerPrefs.GetInt("Coins").ToString();
    public void ChangeGemText() => gemsText.text = PlayerPrefs.GetInt("Gems").ToString();
    
    public void ChangeName(string newName)
    {
        if (newName != "" && newName.Length <= 8)
        {
            battleButton.GetComponent<Button>().interactable = true;
            PlayerPrefs.SetString("PlayerName", newName);
        }
        else battleButton.GetComponent<Button>().interactable = false;
    }

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
    public void UpdatePricePeople()
    {
        var countPeople = PlayerPrefs.GetInt("PlayerPeople") + 1;
        peopleTitleText.text = "Человечки - " + PlayerPrefs.GetInt("PlayerPeople");
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
    
    public void UpdatePriceHealth()
    {
        var countHealth = PlayerPrefs.GetInt("PlayerHealth") + 1;
        healthTitleText.text = "Здоровье - " + (100 + PlayerPrefs.GetInt("PlayerHealth") * 10);
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
        var indexWeapon = PlayerPrefs.GetInt("WeaponLevel") + 1;
        if (indexWeapon > 9) weaponButton.interactable = false;
        weaponTitleText.text = weapons.weaponsName[indexWeapon - 1];
        weaponPriceText.text = (indexWeapon * 1000 * indexWeapon).ToString();
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
