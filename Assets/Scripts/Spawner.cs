using static NTC.Global.Pool.NightPool;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
	[SerializeField] private GameObject prefab;
	[SerializeField][Range(0, 1000)] private int prefabCount;
	[SerializeField] private Vector3 worldSize;
	[SerializeField] private bool isPoolable;

	private Transform _cachedTransform;

	private void Awake()
	{
		_cachedTransform = GetComponent<Transform>();
		for (var index = 0; index < prefabCount; index++) SpawnObject();
	}

	public void SpawnObject()
	{
		if (isPoolable) Spawn(prefab, RandomPosition());
		else Instantiate(prefab, RandomPosition(), Quaternion.identity);
	}

	public Vector3 RandomPosition()
	{
		var position = transform.position;
		var randomDirection = new Vector3(Random.value * worldSize.x, position.y, Random.value * worldSize.z);
		randomDirection += position;
		if (NavMesh.SamplePosition(randomDirection, out var hit, worldSize.x, NavMesh.AllAreas))
			return hit.position;
		return Vector3.zero;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(transform.position + (worldSize / 2), worldSize);
	}
}
