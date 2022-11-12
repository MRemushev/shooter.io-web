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

	private Transform _cachedTransform;
	private Vector3 _previousOffset;

	private void Awake()
	{
		_cachedTransform = GetComponent<Transform>();
		_previousOffset = new Vector3(_cachedTransform.eulerAngles.x, offset.y, offset.z);
	}

	private void FixedUpdate() =>
		_cachedTransform.position = Vector3.Lerp(_cachedTransform.position, mainPlayer.position + offset, speed);

	public void ChangeOffset(float newOffset)
	{
		_cachedTransform.eulerAngles = new Vector3(_previousOffset.x + newOffset * forceOffset / 1.25f, 0, 0);
		if (newOffset >= 40)offset = new Vector3(0, _previousOffset.y + newOffset * forceOffset, _previousOffset.z - newOffset * forceOffset);
		else offset = new Vector3(0, _previousOffset.y + 40 * forceOffset, _previousOffset.z - 40 * forceOffset);
	}
}
