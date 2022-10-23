using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using YG;
using Random = UnityEngine.Random;

public class RewardSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    public float msToWait = 86400000;
    private Button _rewardButton;
    private ulong _lastOpen;

    private readonly int[] _gemGifts = {5, 10, 15};
    private readonly int[] _coinGifts = {100, 500, 1000};

    private void Start()
    {
        YandexGame.CloseVideoEvent += Rewarded;
        _rewardButton = GetComponent<Button>();
        if (PlayerPrefs.HasKey("lastOpen")) _lastOpen = ulong.Parse(PlayerPrefs.GetString("lastOpen"));
        _rewardButton.interactable = IsReady();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_rewardButton.IsInteractable()) return;
        if (IsReady()) 
        {
            _rewardButton.interactable = true;
            return;
        }
        var diff = (ulong)DateTime.Now.Ticks - _lastOpen;
        var m = diff / TimeSpan.TicksPerMillisecond;
        var secondLeft = (msToWait - m) / 1000.0f;

        var t = "";

        if (secondLeft / 3600 > 1) t += (int)secondLeft / 3600 + "ч ";
        secondLeft -= (int)secondLeft / 3600 * 3600;
        if (secondLeft / 60 > 1) t += (int)secondLeft / 60 + "м ";
        if ((msToWait - m) / 1000.0f / 3600 < 1) t += (int)secondLeft % 60 + "с ";

        buttonText.text = t;
    }

    public void Rewarded(int id) {
        if (id != 0) return;
        _lastOpen = (ulong)DateTime.Now.Ticks;
        PlayerPrefs.SetString("lastOpen", _lastOpen.ToString());
        _rewardButton.interactable = false;
        var isGems = Random.Range(0, 2) != 0;
        print(isGems);
        var playerMenu = FindObjectOfType<PlayerMenu>();
        if (isGems)
        {
            var count = _gemGifts[Random.Range(0, _gemGifts.Length - 1)];
            PlayerPrefs.SetInt("Gems", PlayerPrefs.GetInt("Gems") + count);
            print(count);
            playerMenu.ChangeGemText();
        }
        else
        {
            var count = _coinGifts[Random.Range(0, _coinGifts.Length - 1)];
            PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + count);
            print(count);
            playerMenu.ChangeCoinText();
        }
    }

    private bool IsReady() 
    {
        var diff = (ulong)DateTime.Now.Ticks - _lastOpen;
        var m = diff / TimeSpan.TicksPerMillisecond;
        var secondLeft = (msToWait - m) / 1000.0f;
        return !(secondLeft >= 0);
    }
}