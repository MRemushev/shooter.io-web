using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoodMovement : MonoBehaviour
{
    private float _updateTime = 2;
    private Vector3 _direction;

    private void Start()
    {
        _updateTime /= Random.Range(0.75f, 1.25f); // Setting the rotation interval
        StartCoroutine(Rotate());
    }

    private void FixedUpdate()
    {
        if (_direction == Vector3.zero) return;
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(_direction), 1);
    }

    private void OnCollisionEnter() => _direction = -_direction; // If the object has encountered a collision, then turn around

    private IEnumerator Rotate()
    {
        while (true)
        {
            _direction = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)); // Randomize the turn
            yield return new WaitForSeconds(_updateTime);
        }
    }
}
