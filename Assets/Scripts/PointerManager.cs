using System.Collections.Generic;
using UnityEngine;

public class PointerManager : MonoBehaviour {

    [SerializeField] private PointerIcon pointerPrefab;
    [SerializeField] private Transform targetTransform;
    public readonly Dictionary<Transform, PointerIcon> dictionary = new Dictionary<Transform, PointerIcon>();
    private Camera _mainCamera;

    public static PointerManager instance;
    private void Awake()
    {
        _mainCamera = FindObjectOfType<Camera>();
        if (!instance) instance = this;
        else Destroy(this);
    }

    public void AddToList(Transform enemyPointer)
    {
        var newPointer = Instantiate(pointerPrefab, transform);
        newPointer.transform.SetAsFirstSibling();
        dictionary.Add(enemyPointer, newPointer);
    }

    public void RemoveFromList(Transform enemyPointer)
    {
        Destroy(dictionary[enemyPointer].gameObject);
        dictionary.Remove(enemyPointer);
    }

    private void LateUpdate()
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);

        foreach (var (enemyPointer, pointerIcon) in dictionary)
        {
            var playerPosition = targetTransform.position;
            var toEnemy = enemyPointer.position - playerPosition;
            var ray = new Ray(playerPosition, toEnemy);
            Debug.DrawRay(playerPosition, toEnemy);
            var rayMinDistance = Mathf.Infinity;
            var index = 0;

            for (var p = 0; p < 4; p++)
            {
                if (!planes[p].Raycast(ray, out var distance)) continue;
                if (!(distance < rayMinDistance)) continue;
                rayMinDistance = distance;
                index = p;
            }

            rayMinDistance = Mathf.Clamp(rayMinDistance, 0, toEnemy.magnitude);
            var worldPosition = ray.GetPoint(rayMinDistance);
            var position = _mainCamera.WorldToScreenPoint(worldPosition);
            var rotation = toEnemy.magnitude > rayMinDistance ? GetIconRotation(index) : Quaternion.identity;

            pointerIcon.SetIconPosition(position, rotation);
        }
    }

    private static Quaternion GetIconRotation(int planeIndex)
    {
        return planeIndex switch
        {
            0 => Quaternion.Euler(0f, 0f, -90f),
            1 => Quaternion.Euler(0f, 0f, 90f),
            2 => Quaternion.identity,
            3 => Quaternion.Euler(0f, 0f, 180),
            _ => Quaternion.identity
        };
    }

}
