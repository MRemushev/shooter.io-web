using UnityEngine;

public class CameraController : MonoBehaviour
{
	[Header("Target")]
	[SerializeField] private Transform mainPlayer;
	[Space]
	[Header("Camera Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float forceOffset;
	[SerializeField] private Vector3 offset;
	[SerializeField] private Vector2 limitOffset;

	[HideInInspector] public Transform cachedTransform;
	private Vector3 _previousOffset;

	private void Awake()
	{
		cachedTransform = GetComponent<Transform>();
		_previousOffset = new Vector3(cachedTransform.eulerAngles.x, offset.y, offset.z);
	}

	private void FixedUpdate() =>
		cachedTransform.position = Vector3.Lerp(cachedTransform.position, mainPlayer.position + offset, speed);

	public void ChangeOffset(float newOffset)
	{
		if (newOffset > limitOffset.y) return;
		if (newOffset >= limitOffset.x)
		{
			offset = new Vector3(0, _previousOffset.y + newOffset * forceOffset, _previousOffset.z - newOffset * forceOffset);
			cachedTransform.eulerAngles = new Vector3(_previousOffset.x + newOffset * forceOffset / 1.25f, 0, 0);
		}
		else
		{
			offset = new Vector3(0, _previousOffset.y + limitOffset.x * forceOffset, _previousOffset.z - limitOffset.x * forceOffset);
			cachedTransform.eulerAngles = new Vector3(_previousOffset.x + limitOffset.x * forceOffset / 1.25f, 0, 0);
		}
	}
}
