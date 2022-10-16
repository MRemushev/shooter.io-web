using UnityEngine;
using UnityEngine.UI;

public class DeadSlider : MonoBehaviour
{
    [SerializeField] private GameObject deadScreen;
    [SerializeField] private float speed;
    
    private Slider _slider;
    
    private void Awake() => _slider = GetComponent<Slider>();
    private void OnEnable() => _slider.value = 1;
    private void FixedUpdate()
    {
        _slider.value -= speed;
        if (_slider.value > 0) return;
        deadScreen.SetActive(false);
        FindObjectOfType<GameManager>().ShowReward();
    }
}
