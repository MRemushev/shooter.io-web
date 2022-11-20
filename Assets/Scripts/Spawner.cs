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

	private Vector3 _cachedPosition;

	private void Awake()
	{
		_cachedPosition = GetComponent<Transform>().position;
		for (var index = 0; index < prefabCount; index++) SpawnObject();
	}

	public void SpawnObject()
	{
		if (isPoolable) Spawn(prefab, RandomPosition());
		else Instantiate(prefab, RandomPosition(), Quaternion.identity);
	}

	public Vector3 RandomPosition()
	{
		var randomDirection = new Vector3(Random.value * worldSize.x, _cachedPosition.y, Random.value * worldSize.z);
		randomDirection += _cachedPosition;
		return NavMesh.SamplePosition(randomDirection, out var hit, worldSize.x, NavMesh.AllAreas) ? hit.position : Vector3.zero;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(transform.position + (worldSize / 2), worldSize);
	}
}
