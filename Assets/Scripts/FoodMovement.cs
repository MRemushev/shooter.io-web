using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoodMovement : MonoBehaviour
{
    private float _updateTime = 5;
    private Quaternion _direction;

    private void Start()
    {
        _updateTime /= Random.Range(0.75f, 1.25f); // Setting the rotation interval
        StartCoroutine(Rotate());
    }

    private void FixedUpdate()
    {
        if (_direction == transform.rotation) return;
        transform.rotation = Quaternion.Lerp(transform.rotation, _direction, 0.1f);
    }

    private void OnCollisionEnter() => _direction.y -= 180; // If the object has encountered a collision, then turn around

    private IEnumerator Rotate()
    {
        while (true)
        {
            _direction = Quaternion.Euler(0, Random.Range(-180, 180), 0); // Randomize the turn
            yield return new WaitForSeconds(_updateTime);
        }
    }
}
