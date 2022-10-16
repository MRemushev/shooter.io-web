using static NTC.Global.Pool.NightPool;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField][Range (0,1000)] private int prefabCount;
    [SerializeField] private Vector3 worldSize;
    [SerializeField] private bool isPoolable;

    private void Awake()
    {
        for (var index = 0; index < prefabCount; index++) SpawnObject();
    }

    public void SpawnObject()
    {
        var position = transform.position;
        var randomDirection = new Vector3 (Random.value * worldSize.x, position.y, Random.value * worldSize.z);
        randomDirection += position;
        if (!NavMesh.SamplePosition(randomDirection, out var hit, worldSize.x, 1)) return;
        if (isPoolable) Spawn(prefab, hit.position);
        else Instantiate(prefab, hit.position, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + (worldSize / 2), worldSize);
    }
}
