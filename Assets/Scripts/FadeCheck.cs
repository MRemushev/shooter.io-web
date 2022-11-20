using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FadeCheck : MonoBehaviour
{
	[SerializeField] private LayerMask layerMask;
	[SerializeField] private Transform mainCamera;
	[SerializeField] private Transform target;
	[SerializeField] [Range(0, 1f)] private float fadedAlpha = 0.15f;
	[SerializeField] private bool retainShadows;
	[SerializeField] private Vector3 targetPositionOffset = Vector3.up;
	[SerializeField] private float fadeSpeed = 1;

	[Header("Read Only Data")]
	[SerializeField]
	private List<FadeObject> objectsBlockingView = new List<FadeObject>();
	private readonly Dictionary<FadeObject, Coroutine> _runningCoroutines = new Dictionary<FadeObject, Coroutine>();

	private readonly RaycastHit[] _hits = new RaycastHit[10];
	private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
	private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
	private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
	private static readonly int Surface = Shader.PropertyToID("_Surface");
	
	private void OnEnable() => StartCoroutine(CheckForObjects());

	private void Start() => mainCamera = mainCamera.GetComponent<Transform>();

	private IEnumerator CheckForObjects()
	{
		while (true)
		{
			var cameraPosition = mainCamera.position;
			var targetPosition = target.position;
			var hits = Physics.RaycastNonAlloc(
				cameraPosition,
				(targetPosition + targetPositionOffset - cameraPosition).normalized, _hits,
				Vector3.Distance(cameraPosition, targetPosition + targetPositionOffset), layerMask
			);

			if (hits > 0)
			{
				for (var i = 0; i < hits; i++)
				{
					var fadingObject = GetFadingObjectFromHit(_hits[i]);
					if (!fadingObject || objectsBlockingView.Contains(fadingObject)) continue;
					if (_runningCoroutines.ContainsKey(fadingObject))
					{
						if (_runningCoroutines[fadingObject] != null) StopCoroutine(_runningCoroutines[fadingObject]);
						_runningCoroutines.Remove(fadingObject);
					}
					_runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectOut(fadingObject)));
					objectsBlockingView.Add(fadingObject);
				}
			}

			FadeObjectsNoLongerBeingHit();
			ClearHits();
			yield return null;
		}
	}

	private void FadeObjectsNoLongerBeingHit()
	{
		var objectsToRemove = new List<FadeObject>(objectsBlockingView.Count);

		foreach (var fadingObject in from fadingObject in objectsBlockingView
		         let objectIsBeingHit =
			         _hits.Select(GetFadingObjectFromHit).Any(hitFadingObject =>
				         hitFadingObject && fadingObject == hitFadingObject)
		         where !objectIsBeingHit
		         select fadingObject)
		{
			if (_runningCoroutines.ContainsKey(fadingObject))
			{
				if (_runningCoroutines[fadingObject] != null) StopCoroutine(_runningCoroutines[fadingObject]);
				_runningCoroutines.Remove(fadingObject);
			}
			_runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectIn(fadingObject)));
			objectsToRemove.Add(fadingObject);
		}

		foreach (var removeObject in objectsToRemove) objectsBlockingView.Remove(removeObject);
	}

	private IEnumerator FadeObjectOut(FadeObject fadingObject)
	{
		foreach (var material in fadingObject.materials)
		{
			material.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			material.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt(ZWrite, 0);
			material.SetInt(Surface, 1);

			material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

			material.SetShaderPassEnabled("DepthOnly", false);
			material.SetShaderPassEnabled("SHADOWCASTER", retainShadows);

			material.SetOverrideTag("RenderType", "Transparent");

			material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
			material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
		}

		float time = 0;

		while (fadingObject.materials[0].color.a > fadedAlpha)
		{
			foreach (var material in fadingObject.materials.Where(material => material.HasProperty("_Color")))
			{
				material.color = new Color(
					material.color.r,
					material.color.g,
					material.color.b,
					Mathf.Lerp(fadingObject.initialAlpha, fadedAlpha, time * fadeSpeed)
				);
			}

			time += Time.deltaTime;
			yield return null;
		}

		if (!_runningCoroutines.ContainsKey(fadingObject)) yield break;
		StopCoroutine(_runningCoroutines[fadingObject]);
		_runningCoroutines.Remove(fadingObject);
	}

	private IEnumerator FadeObjectIn(FadeObject fadingObject)
	{
		float time = 0;

		while (fadingObject.materials[0].color.a < fadingObject.initialAlpha)
		{
			foreach (var material in fadingObject.materials.Where(material => material.HasProperty("_Color")))
			{
				material.color = new Color(
					material.color.r,
					material.color.g,
					material.color.b,
					Mathf.Lerp(fadedAlpha, fadingObject.initialAlpha, time * fadeSpeed)
				);
			}

			time += Time.deltaTime;
			yield return null;
		}

		foreach (var material in fadingObject.materials)
		{
			material.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
			material.SetInt(ZWrite, 1);
			material.SetInt(Surface, 0);

			material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

			material.SetShaderPassEnabled("DepthOnly", true);
			material.SetShaderPassEnabled("SHADOWCASTER", true);

			material.SetOverrideTag("RenderType", "Opaque");

			material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		}

		if (!_runningCoroutines.ContainsKey(fadingObject)) yield break;
		StopCoroutine(_runningCoroutines[fadingObject]);
		_runningCoroutines.Remove(fadingObject);
	}

	private void ClearHits() => System.Array.Clear(_hits, 0, _hits.Length);
	

	private FadeObject GetFadingObjectFromHit(RaycastHit hit)
	{
		return hit.collider ? hit.collider.GetComponent<FadeObject>() : null;
	}
}