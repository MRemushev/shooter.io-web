using NTC.Global.Cache;
using UnityEngine;

public class CameraController : MonoCache
{
	[Header("Target")]
	[SerializeField] private Transform mainPlayer;

	[Space] [Header("Camera Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float forceOffset;
	[SerializeField] private Vector3 offset;
	[SerializeField] private float maxOffset;

	[HideInInspector] public Transform cachedTransform;
	
	private Vector3 _previousOffset;

	private void Awake()
	{
		cachedTransform = GetComponent<Transform>();
		_previousOffset = new Vector3(cachedTransform.eulerAngles.x, offset.y, offset.z);
	}

	private void Start() => cachedTransform.position = mainPlayer.position + offset;

	protected override void FixedRun() =>
		cachedTransform.position = Vector3.Lerp(cachedTransform.position, mainPlayer.position + offset, speed);

	public void ChangeOffset(float newOffset)
	{
		var forcedOffset = newOffset * forceOffset;
		offset = forcedOffset <= maxOffset
			? new Vector3(0, _previousOffset.y + forcedOffset, _previousOffset.z - forcedOffset)
			: new Vector3(0, _previousOffset.y + maxOffset, _previousOffset.z - maxOffset);
	}
}
