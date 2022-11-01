using TMPro;
using UnityEngine;

public class GroupRank : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI nameText;
	public MainCharacter characterData;

	private void Start() => nameText.text = characterData.characterName;
}
