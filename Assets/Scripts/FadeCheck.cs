using UnityEngine;

public class FadeCheck : MonoBehaviour
{
    [HideInInspector] public GameObject objectHit;
    [HideInInspector] public GameObject parentObjectHit;
    [Range(0.0f, 1.0f)] public float fadeTo;

    private Transform _cameraTransform;

    private void Start() => _cameraTransform = FindObjectOfType<Camera>().GetComponent<Transform>();
    
    private void LateUpdate()
    {
        var cameraPosition = _cameraTransform.position;
        var direction = (transform.position - cameraPosition).normalized;
        var ray = new Ray(cameraPosition - new Vector3(0, -4, 6), direction);
        if (!Physics.Raycast(ray, out var hit, 50f)) return;
        var hitObject = hit.transform.gameObject;
        objectHit = hitObject;
        var findParent = hitObject;
        while (findParent.transform.parent) findParent = findParent.transform.parent.gameObject;
        parentObjectHit = findParent;
    }
}