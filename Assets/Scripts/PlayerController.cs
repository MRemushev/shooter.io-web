using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using System.Collections;
using YG;

public class PlayerController : MainCharacter
{
	[Header("Player components")]
	[SerializeField] private new Rigidbody rigidbody;
	[SerializeField] private LineRenderer laserBeam;
	[SerializeField] private Joystick walkJoystick;
	[SerializeField] private GameObject deadScreen;
	[SerializeField] private TextMeshProUGUI countTeamText;
	[SerializeField] private TextMeshProUGUI killsCountText;
	[SerializeField] private TextMeshProUGUI weaponLevelText;
	[SerializeField] private TextMeshProUGUI hpText;
	[SerializeField] private TextMeshProUGUI weaponStatsText;
	[SerializeField] private float movementSpeed;
	
	private bool _isImmortality;
	private int _killPromotion = 2;
	private CameraController _cameraOffset;
	private Vector3 _movementVector;

	private void Start()
	{
		characterName = "Player";
		if (YandexGame.EnvironmentData.isDesktop) walkJoystick.gameObject.SetActive(false);
		skinObject.material.mainTexture = skinArray.textureList[PlayerPrefs.GetInt("PlayerSkin")];
		_cameraOffset = Find<CameraController>();
		var spawnPosition = EnemySpawner.RandomPosition();
		spawnPosition.y = cachedTransform.position.y;
		cachedTransform.position = spawnPosition;
		_cameraOffset.cachedTransform.position = spawnPosition;
		PreviousHealth = 100 + PlayerPrefs.GetInt("PlayerHealth") * 10;
		Renaissance();
		shootingArea.size = new Vector3(CharacterWeapon.FireRange * 6, 1, CharacterWeapon.FireRange * 6);
		laserBeam.SetPosition(1, new Vector3(0, 2.2f, CharacterWeapon.FireRange * 3));
		weaponLevelText.text = (weapons.WeaponLevel + 1).ToString();
		_cameraOffset.ChangeOffset(CharacterWeapon.FireRange);
		ChangeWeaponStatsText();
	}

	protected override void Run() => _movementVector = walkJoystick.isActiveAndEnabled
		? new Vector3(walkJoystick.Horizontal, 0, walkJoystick.Vertical).normalized
		: new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;	
	
	protected override void FixedRun()
	{
		rigidbody.velocity = _movementVector * movementSpeed;
		if (!attackTarget) rigidbody.MoveRotation(Quaternion.LookRotation(_movementVector));
		relativeVector = cachedTransform.InverseTransformDirection(_movementVector);
		animator.SetFloat(Horizontal, relativeVector.x);
		animator.SetFloat(Vertical, relativeVector.z);
		laserBeam.enabled = attackTarget;
		IsStop = rigidbody.velocity == Vector3.zero;
	}

	private void OnTriggerStay(Collider col) // Shooting area stay
	{
		if (Vector3.Distance(cachedTransform.position, col.transform.position) > CharacterWeapon.FireRange) return;
		if (col.CompareTag("Team") && col.GetComponent<CapsuleCollider>().enabled) AutoShooting(col);
		else if (col.CompareTag("Enemy") && col.GetComponent<CapsuleCollider>().enabled) AutoShooting(col);
		else attackTarget = null;
	}

	private void OnTriggerExit(Collider other) => attackTarget = null;

	private void OnCollisionEnter(Collision col)
	{
		// If the player touched an object with the tag "Food", then we call the function of adding a team
		if (col.gameObject.CompareTag("Food"))
		{
			StartCoroutine(AddCharacter(cachedTransform.position, 1, col));
			ChangeStats();
		}
		else if (col.gameObject.CompareTag("FoodBox"))
		{
			StartCoroutine(AddCharacter(cachedTransform.position, 5, col));
			ChangeStats();
		}
	}

	private void ChangeHpText() =>
		hpText.text = "HP " + Mathf.Max(0, Mathf.Round(CharacterCount * PreviousHealth + Health));

	private void ChangeWeaponStatsText() =>
		weaponStatsText.text = CharacterWeapon.gameObject.name + " - " + CharacterWeapon.DamagePerSecond;

	public void ChangeStats()
	{
		countTeamText.text = CharacterCount.ToString();
		var score = (int)((CharacterCount + 1) * CharacterWeapon.DamagePerSecond);
		if (PlayerPrefs.GetInt("HighScore") < score) PlayerPrefs.SetInt("HighScore", score);
		ChangeHpText();
		RankManager.ChangeRating();
	}

	private void AutoShooting(Component col)
	{
		// We turn in the direction of the shot
		if (!attackTarget) attackTarget = col.transform;
		var lookTarget = Quaternion.LookRotation(attackTarget.position - cachedTransform.position);
		lookTarget.eulerAngles = new Vector3(0, lookTarget.eulerAngles.y, 0);
		cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation, lookTarget, 10 * Time.deltaTime);
		CharacterWeapon.Shoot(); // Starting the shooting effect
		if (!CharacterWeapon.IsShot) return;
		var isEnemy = col.GetComponent<EnemyController>();
		if (isEnemy) isEnemy.TakeDamage(TotalDamage);
		else col.GetComponent<TeamController>().targetScript.GetComponent<EnemyController>().TakeDamage(TotalDamage);
	}

	// Damage acceptance function
	public IEnumerator TakeDamage(EnemyController enemyController, float damage)
	{
		if (damage < 1 || _isImmortality) yield break; // Check that the damage is not less than one
		Health -= damage;
		ChangeHpText();
		// Check if the player has teammates
		if (CharacterCount == 0)
		{
			bloodFX.Play();
			if (Health > 1) yield break; // Check how much health the player has
			var gameManager = Find<GameManager>();
			gameManager.UpdatePriceChance();
			deadScreen.SetActive(true); // Calling the screen of death
			gameManager.SetPause(true); // Putting the game on pause
		}
		else
		{
			characterList[Random.Range(0, CharacterCount)].bloodFX.Play();
			if (Health > 1) yield break;
			while (Health < 1)
			{
				if (CharacterCount > 0)
				{
					var deadCharacter = Random.Range(0, CharacterCount);
					characterList[deadCharacter].DeathPlay();
					countTeamText.text = CharacterCount.ToString();
					Health += PreviousHealth;
					RankManager.ChangeRating();
				}
				else
				{
					deadScreen.SetActive(true); // Calling the screen of death
					Find<GameManager>().SetPause(true);
					break;
				}
				yield return null;
			}
		}
	}

	public void AddKill()
	{
		attackTarget = null;
		ScoreKills += 1;
		killsCountText.text = ScoreKills.ToString();
		_killPromotion -= 1;
		if (_killPromotion != 0) return;
		var weaponLevel = PlayerPrefs.GetInt("WeaponLevel") + ScoreKills / 2;
		weapons.ChangeWeapon(weaponLevel); // Updating weapons to the main man
		weaponLevelText.text = (weaponLevel + 1).ToString();
		// Update fire area
		shootingArea.size = new Vector3(CharacterWeapon.FireRange * 6, 1, CharacterWeapon.FireRange * 6);
		laserBeam.SetPosition(1, new Vector3(0, 2.1f, CharacterWeapon.FireRange * 3));
		_cameraOffset.ChangeOffset(CharacterWeapon.FireRange);
		ChangeWeaponStatsText();
		RankManager.ChangeRating();
		foreach (var people in characterList) people.LevelUp();
		_killPromotion = 2;
	}

	public void Renaissance()
	{
		weapons.ChangeWeapon(PlayerPrefs.GetInt("WeaponLevel") + ScoreKills / 2);
		Health = PreviousHealth;
		_cameraOffset.ChangeOffset(CharacterWeapon.FireRange);
		if (PlayerPrefs.HasKey("PlayerPeople"))
			StartCoroutine(AddCharacter(cachedTransform.position, PlayerPrefs.GetInt("PlayerPeople")));
		ChangeHpText();
		StartCoroutine(Immortality());
	}

	private IEnumerator Immortality()
	{
		_isImmortality = true;
		yield return new WaitForSeconds(4f);
		_isImmortality = false;
	}
}