using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointerIcon : MonoBehaviour 
{
    [SerializeField] private Image image;
    public TextMeshProUGUI countText;
    private Transform _mainTransform;

    private void Awake()
    {
        image.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        _mainTransform = transform;
    }

    public void SetIconPosition(Vector3 position, Quaternion rotation) {
        _mainTransform.position = position;
        _mainTransform.rotation = rotation;
    }
}
