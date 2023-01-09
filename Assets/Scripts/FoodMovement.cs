using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class FoodMovement : MonoBehaviour
{
	private float _updateTime = 1;
	private Quaternion _direction;
	
	[HideInInspector] public Transform cachedTransform;

	private void Awake()
	{
		cachedTransform = GetComponent<Transform>();
		_updateTime /= Random.Range(0.25f, 1); // Setting the rotation interval
		StartCoroutine(Rotate());
	}

	private void FixedUpdate()
	{
		if (_direction != cachedTransform.rotation) 
			cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation, _direction, 0.1f);
	}
	
	private IEnumerator Rotate()
	{
		while (true)
		{
			_direction = Quaternion.Euler(0, Random.Range(-180, 180), 0); // Randomize the turn
			yield return new WaitForSeconds(_updateTime);
		}
	}
}
