using System;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour, IEquatable<FadeObject>
{
    public List<Renderer> renderers = new List<Renderer>();
    public Vector3 objectPosition;
    public List<Material> materials = new List<Material>();
    [HideInInspector]
    public float initialAlpha;

    private void Awake()
    {
        objectPosition = transform.position;
        if (renderers.Count == 0) renderers.AddRange(GetComponentsInChildren<Renderer>());
        foreach (var rendererMaterial in renderers) materials.AddRange(rendererMaterial.materials);

        initialAlpha = materials[0].color.a;
    }

    public bool Equals(FadeObject other)
    {
        return other && objectPosition.Equals(other.objectPosition);
    }

    public override int GetHashCode()
    {
        return objectPosition.GetHashCode();
    }
}