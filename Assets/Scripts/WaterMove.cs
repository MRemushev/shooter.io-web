using UnityEngine;

public class WaterMove : MonoBehaviour
{
    [SerializeField] private float speed;
    private Renderer _renderer;

    private void Start() => _renderer = GetComponent<Renderer>();
    private void FixedUpdate() => _renderer.material.mainTextureOffset += new Vector2(speed, speed);
}
