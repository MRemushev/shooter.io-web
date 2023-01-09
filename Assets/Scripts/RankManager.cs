using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RankManager : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI[] ratingTexts;
	[SerializeField] private TextMeshProUGUI playerRating;

	public List<MainCharacter> charactersData;

	public void ChangeRating()
	{
		charactersData = charactersData.OrderByDescending(x => x.CharacterScore).ToList();
		for (var i = 0; i < ratingTexts.Length; i++)
		{
			try {
				ratingTexts[i].text = charactersData[i].characterName + " " + charactersData[i].CharacterScore;
			} catch {
				return;
			}
		}
		var playerStats = charactersData.Find(x => x.Get<PlayerController>());
		playerRating.text = charactersData.LastIndexOf(playerStats) + 1 + " " + playerStats.characterName + " " +
		                    playerStats.CharacterScore;
	}
}
