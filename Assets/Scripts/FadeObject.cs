using System;
using UnityEngine;

public class FadeObject : MonoBehaviour
{
	public enum FadeType
	{
		Single,
		Whole,
		Parent,
		Multiple
	}

	public FadeType fadeType;
	[Header("Custom Alpha Fade")]
	public bool customFade;
	[Range(0.0f, 1.0f)] public float fadeTo;
	[Header("Multiple Fade Type")]
	public GameObject[] otherObjects;
	public bool checkParentObject;
	[HideInInspector] public bool faded;
	private FadeCheck _fadeCheck;
	private Renderer _objectRenderer;
	private GameObject _highestParent;

	// Start is called before the first frame update
	private void Start()
	{
		_fadeCheck = FindObjectOfType<PlayerController>().GetComponent<FadeCheck>();
		_objectRenderer = GetComponent<Renderer>();
		_highestParent = gameObject;
		while (_highestParent.transform.parent) _highestParent = _highestParent.transform.parent.gameObject;
	}

	// Update is called once per frame
	private void Update()
	{
		switch (fadeType)
		{
			case FadeType.Single when _fadeCheck.objectHit == gameObject:
				{
					if (!faded)
					{
						foreach (var material in _objectRenderer.materials)
						{
							MaterialObjectFade.MakeFade(material);
							var newColor = material.color;
							newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
							material.color = newColor;
						}
						faded = true;
					}

					break;
				}
			case FadeType.Single:
				{
					if (faded)
					{
						foreach (var material in _objectRenderer.materials)
						{
							MaterialObjectFade.MakeOpaque(material);
							var newColor = material.color;
							newColor.a = 1;
							material.color = newColor;
						}
						faded = false;
					}

					break;
				}
			case FadeType.Whole when _fadeCheck.parentObjectHit == _highestParent:
				{
					if (!faded)
					{
						var findParent = transform.parent ? transform.parent.gameObject : null;
						while (findParent)
						{
							if (findParent.GetComponent<Renderer>())
							{
								foreach (var material in findParent.GetComponent<Renderer>().materials)
								{
									MaterialObjectFade.MakeFade(material);
									var newColor = material.color;
									newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
									material.color = newColor;
								}
							}
							for (var i = 0; i < findParent.transform.childCount; i++)
							{
								foreach (var material in findParent.transform.GetChild(i).GetComponent<Renderer>().materials)
								{
									MaterialObjectFade.MakeFade(material);
									var newColor = material.color;
									newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
									material.color = newColor;
								}
							}
							if (findParent.transform.parent)
							{
								findParent = findParent.transform.parent.gameObject;
								if (findParent.GetComponent<Renderer>()) findParent = null;
							}
							else findParent = null;
						}
						faded = true;
					}
					break;
				}
			case FadeType.Whole:
				{
					if (faded)
					{
						var findParent = transform.parent ? transform.parent.gameObject : null;
						while (findParent)
						{
							if (findParent.GetComponent<Renderer>())
							{
								foreach (var material in findParent.GetComponent<Renderer>().materials)
								{
									MaterialObjectFade.MakeOpaque(material);
									var newColor = material.color;
									newColor.a = 1f;
									material.color = newColor;
								}
							}
							for (var i = 0; i < findParent.transform.childCount; i++)
							{
								foreach (var material in findParent.transform.GetChild(i).GetComponent<Renderer>().materials)
								{
									MaterialObjectFade.MakeOpaque(material);
									var newColor = material.color;
									newColor.a = 1f;
									material.color = newColor;
								}
							}
							if (findParent.transform.parent)
							{
								findParent = findParent.transform.parent.gameObject;
								if (findParent.GetComponent<Renderer>()) findParent = null;
							}
							else findParent = null;
						}
						faded = false;
					}

					break;
				}
			case FadeType.Parent when _fadeCheck.parentObjectHit == _highestParent:
				{
					if (!faded)
					{
						foreach (var material in _objectRenderer.materials)
						{
							MaterialObjectFade.MakeFade(material);
							var newColor = material.color;
							newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
							material.color = newColor;
						}
						foreach (var material in transform.parent.GetComponent<Renderer>().materials)
						{
							MaterialObjectFade.MakeFade(material);
							var newColor = material.color;
							newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
							material.color = newColor;
						}
						faded = true;
					}

					break;
				}
			case FadeType.Parent:
				{
					if (faded)
					{
						foreach (var material in _objectRenderer.materials)
						{
							MaterialObjectFade.MakeOpaque(material);
							var newColor = material.color;
							newColor.a = 1;
							material.color = newColor;
						}
						foreach (var material in transform.parent.GetComponent<Renderer>().materials)
						{
							MaterialObjectFade.MakeOpaque(material);
							var newColor = material.color;
							newColor.a = 1;
							material.color = newColor;
						}
						faded = false;
					}

					break;
				}
			case FadeType.Multiple:
				switch (checkParentObject)
				{
					case false when _fadeCheck.objectHit == gameObject:
						{
							foreach (var material in _objectRenderer.materials)
							{
								MaterialObjectFade.MakeFade(material);
								var newColor = material.color;
								newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
								material.color = newColor;
							}
							foreach (var t in otherObjects)
							{
								foreach (var material in t.GetComponent<Renderer>().materials)
								{
									MaterialObjectFade.MakeFade(material);
									var newColor = material.color;
									newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
									material.color = newColor;
								}
							}
							faded = true;
							break;
						}
					case false:
						{
							if (faded)
							{
								foreach (var material in _objectRenderer.materials)
								{
									MaterialObjectFade.MakeOpaque(material);
									var newColor = material.color;
									newColor.a = 1;
									material.color = newColor;
								}
								foreach (var t in otherObjects)
								{
									if (t.GetComponent<FadeObject>() == null)
									{
										foreach (var material in t.GetComponent<Renderer>().materials)
										{
											MaterialObjectFade.MakeOpaque(material);
											var newColor = material.color;
											newColor.a = 1;
											material.color = newColor;
										}
									}
									else t.GetComponent<FadeObject>().faded = true;
								}
								faded = false;
							}

							break;
						}
					case true when _fadeCheck.parentObjectHit == _highestParent:
						{
							foreach (var material in _objectRenderer.materials)
							{
								MaterialObjectFade.MakeFade(material);
								var newColor = material.color;
								newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
								material.color = newColor;
							}
							foreach (var t in otherObjects)
							{
								foreach (var material in t.GetComponent<Renderer>().materials)
								{
									MaterialObjectFade.MakeFade(material);
									var newColor = material.color;
									newColor.a = customFade ? fadeTo : _fadeCheck.fadeTo;
									material.color = newColor;
								}
							}
							faded = true;
							break;
						}
					case true:
						{
							if (faded)
							{
								foreach (var material in _objectRenderer.materials)
								{
									MaterialObjectFade.MakeOpaque(material);
									var newColor = material.color;
									newColor.a = 1;
									material.color = newColor;
								}
								foreach (var t in otherObjects)
								{
									if (t.GetComponent<FadeObject>() == null)
									{
										foreach (Material material in t.GetComponent<Renderer>().materials)
										{
											MaterialObjectFade.MakeOpaque(material);
											var newColor = material.color;
											newColor.a = 1;
											material.color = newColor;
										}
									}
									else t.GetComponent<FadeObject>().faded = true;
								}
								faded = false;
							}

							break;
						}
				}

				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}
