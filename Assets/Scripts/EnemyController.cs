using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using Random = UnityEngine.Random;
using NTC.Global.Pool;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MainCharacter
{
	[Header("Enemy components")]
	[SerializeField] private NavMeshAgent agent;
	[SerializeField] private Transform point;
	[SerializeField] private CapsuleCollider capsuleCollider;

	private FoodMovement[] _foods;
	private PointerIcon _levelText;
	private PlayerController _playerScript;
	private float _previousHeath;
	private bool _isDead;
	private bool _isFound = true;

	private static readonly int DeadAnim = Animator.StringToHash("IsDead");

	private void Start()
	{
		_foods = Finds<FoodMovement>();
		skinObject.material.mainTexture = skinArray.textureList[Random.Range(0, skinArray.textureList.Length)];
		characterName = NameRandomizer.GetRandomName();
		_playerScript = FindObjectOfType<PlayerController>();
		StartCoroutine(AddCharacter(cachedTransform.position,
			CharacterCount < PlayerPrefs.GetInt("PlayerPeople")
				? Random.Range(0, PlayerPrefs.GetInt("PlayerPeople"))
				: Random.Range(0, CharacterCount)));
		ScoreKills = Random.Range(0, PlayerPrefs.GetInt("WeaponLevel") + _playerScript.ScoreKills / 2);
		weapons.ChangeWeapon(ScoreKills);
		PointerManager.Instance.AddToList(point);
		_levelText = PointerManager.Instance.Dictionary[point].GetComponent<PointerIcon>();
		_levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
		shootingArea.size = new Vector3(CharacterWeapon.FireRange * 6, 1, CharacterWeapon.FireRange * 6);
		Health = PreviousHealth = 100 + PlayerPrefs.GetInt("PlayerHealth") * 5 + _playerScript.ScoreKills * 10;
	}

	protected override void FixedRun()
	{
		if (_isDead) return;
		// Movement enemy
		relativeVector = Vector3.ClampMagnitude(transform.InverseTransformDirection(agent.velocity), 1);
		animator.SetFloat(Horizontal, relativeVector.x);
		animator.SetFloat(Vertical, relativeVector.z);
		agent.isStopped = attackTarget;
		IsStop = relativeVector.magnitude < 0.1f;
		// If the agent is stuck, then we try to find a new target
		if (_isFound && IsStop && !agent.isStopped) StartCoroutine(FindClosestFood());
	}

	private void OnCollisionEnter(Collision col)
	{
		// Picking up food and looking for a new target
		if (_isDead || TotalDamage > _playerScript.TotalDamage * 1.25f) return;
		if (col.gameObject.CompareTag("Food")) StartCoroutine(AddCharacter(cachedTransform.position, 1, col));
		else if (col.gameObject.CompareTag("FoodBox")) StartCoroutine(AddCharacter(cachedTransform.position, 5, col));
		else return;
		RankManager.ChangeRating();
	}

	private void OnTriggerStay(Collider col)
	{
		if (_isDead || Vector3.Distance(cachedTransform.position, col.transform.position) > CharacterWeapon.FireRange) return;
		if (col.CompareTag("Team") && col.GetComponent<CapsuleCollider>().enabled) EnemyShooting(col);
		else if (col.CompareTag("Enemy") && col.GetComponent<CapsuleCollider>().enabled) EnemyShooting(col);
		else if (col.CompareTag("Player")) EnemyShooting(col);
		else attackTarget = null;
	}

	private void OnTriggerExit(Collider other) => attackTarget = null;

	private void EnemyShooting(Component col)
	{
		if (!attackTarget && Vector3.Distance(cachedTransform.position, col.transform.position) > CharacterWeapon.FireRange)
		{
			attackTarget = null;
			return;
		}
		attackTarget = col.transform;
		var lookTarget = Quaternion.LookRotation(attackTarget.position - cachedTransform.position);
		lookTarget.eulerAngles = new Vector3(0, lookTarget.eulerAngles.y, 0);
		cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation, lookTarget, 10 * Time.deltaTime);
		CharacterWeapon.Shoot(); // Starting the shooting effect
		if (!CharacterWeapon.IsShot) return;
		if (col.CompareTag("Team"))
		{
			var teamController = col.GetComponent<TeamController>();
			var enemyController = teamController.targetScript.Get<EnemyController>();
			StartCoroutine(enemyController
				? enemyController.TakeDamage(TotalDamage, this)
				: _playerScript.TakeDamage(TotalDamage));
		}
		StartCoroutine(col.CompareTag("Enemy")
			? col.GetComponent<EnemyController>().TakeDamage(TotalDamage, this)
			: _playerScript.TakeDamage(TotalDamage));
	}

	public IEnumerator TakeDamage(float damage, EnemyController enemyController = null)
	{
		if (_isDead) yield break;
		Health -= damage;
		if (CharacterCount == 0)
		{
			bloodFX.Play();
			if (Health > 1) yield break;
			foreach (var character in characterList)
			{
				NightPool.Despawn(character);
				characterList.Remove(character);
			}
			DeathPlay(enemyController);
		}
		else
		{
			characterList[Random.Range(0, CharacterCount)].bloodFX.Play();
			while (Health < 1)
			{
				if (CharacterCount > 0)
				{
					var deadCharacter = Random.Range(0, CharacterCount);
					StartCoroutine(characterList[deadCharacter].DeathPlay());
					Health += PreviousHealth;
					RankManager.ChangeRating();
				}
				else
				{
					DeathPlay(enemyController);
					break;
				}
				yield return null;
			}
		}
	}

	private IEnumerator AddKill()
	{
		attackTarget = null;
		ScoreKills += 1;
		// Updating weapons to the main man
		weapons.ChangeWeapon(ScoreKills);
		shootingArea.size = new Vector3(CharacterWeapon.FireRange * 6, 1, CharacterWeapon.FireRange * 6);
		// Updating weapons to all the player's teammates
		_levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
		foreach (var people in characterList)
		{
			people.LevelUp(weapons.WeaponLevel);
			yield return null;
		}
		RankManager.ChangeRating();
	}

	private void DeathPlay(EnemyController enemyController = null)
	{
		_isDead = true;
		attackTarget = null;
		agent.isStopped = true;
		capsuleCollider.enabled = false;
		shootingArea.enabled = false;
		animator.SetBool(DeadAnim, true);
		PointerManager.Instance.RemoveFromList(point);
		RankManager.charactersData.Remove(this);
		EnemySpawner.SpawnObject();
		StartCoroutine(enemyController ? enemyController.AddKill() : _playerScript.AddKill());
		Destroy(gameObject, 5f);
	}
	
	private IEnumerator FindClosestFood()
	{
		if (_isDead) yield break;
		_isFound = false;
		while (TotalDamage > _playerScript.TotalDamage * 1.25f)
		{
			agent.destination = _playerScript.cachedTransform.position;
			yield return new WaitForSeconds(1);
		}
		var closestDistance = Mathf.Infinity;
		Transform closestPeople = null;
		foreach (var person in _foods)
		{
			var currentDistance = Vector3.Distance(cachedTransform.position, person.cachedTransform.position);
			if (currentDistance > closestDistance) continue;
			closestDistance = currentDistance;
			closestPeople = person.cachedTransform;
			yield return null;
		}
		agent.destination = closestPeople ? closestPeople.position : _playerScript.cachedTransform.position;
		_isFound = true;
	}
}