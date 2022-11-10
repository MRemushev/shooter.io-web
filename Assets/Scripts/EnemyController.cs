using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Random = UnityEngine.Random;
using NTC.Global.Pool;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MainCharacter
{
	[Header("Enemy components")]
	[SerializeField] private NavMeshAgent _agent;
	[SerializeField] private Transform point;
	[SerializeField] private CapsuleCollider capsuleCollider;

	private FoodMovement[] _foods;
	private PointerIcon _levelText;
	private GameObject _playerPrefab;
	private PlayerController _playerScript;
	private float _previousHeath;
	public bool IsDead { get; private set; }

	private static readonly int DeadAnim = Animator.StringToHash("IsDead");

	protected override void OnEnabled()
	{
		skinObject.material.mainTexture = skinArray.textureList[Random.Range(0, skinArray.textureList.Length)];
		characterName = NameRandomizer.GetRandomName();
		_foods = Finds<FoodMovement>();
		_playerPrefab = GameObject.Find("Player");
		_playerScript = _playerPrefab.GetComponent<PlayerController>();
		PointerManager.instance.AddToList(point);
		_levelText = PointerManager.instance.dictionary[point].GetComponent<PointerIcon>();
		_levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
		if (_playerScript.CharacterCount < PlayerPrefs.GetInt("PlayerPeople"))
			AddCharacter(cachedTransform.position, Random.Range(0, PlayerPrefs.GetInt("PlayerPeople")));
		else AddCharacter(cachedTransform.position, Random.Range(0, _playerScript.CharacterCount));
		if (_playerScript.CountKills / 2 < PlayerPrefs.GetInt("WeaponLevel"))
			scoreKills = Random.Range(0, PlayerPrefs.GetInt("WeaponLevel"));
		else scoreKills = Random.Range(0, _playerScript.CountKills / 2);
		weapons.ChangeWeapon(scoreKills);
		shootingArea.size = new Vector3(characterWeapon.FireRange * 6, 1, characterWeapon.FireRange * 6);
		health = previousHealth = 100 + PlayerPrefs.GetInt("PlayerHealth") * 10 + _playerScript.CountKills * 5;
	}

	protected override void Run()
	{
		if (IsDead) return;
		// Movement enemy
		relativeVector = Vector3.ClampMagnitude(transform.InverseTransformDirection(_agent.velocity), 1);
		animator.SetFloat(Horizontal, relativeVector.x);
		animator.SetFloat(Vertical, relativeVector.z);
		isStop = relativeVector.magnitude < 0.1f;
		// If the agent is stuck, then we try to find a new target
		if (isStop && !_agent.isStopped) FindClosestFood();
	}

	private void OnCollisionEnter(Collision col)
	{
		// Picking up food and looking for a new target
		if (col.gameObject.CompareTag("Food")) AddCharacter(cachedTransform.position, 1, col);
		else if (col.gameObject.CompareTag("FoodBox")) AddCharacter(cachedTransform.position, 5, col);
		else return;
		rankManager.ChangeRating();
	}

	private void OnTriggerStay(Collider col)
	{
		if (IsDead) return;
		if (Vector3.Distance(cachedTransform.position, col.transform.position) > characterWeapon.FireRange) return;
		if (col.CompareTag("Team") && col.GetComponent<CapsuleCollider>().enabled) EnemyShooting(col);
		else if (col.CompareTag("Enemy") && col.GetComponent<CapsuleCollider>().enabled) EnemyShooting(col);
		else if (col.CompareTag("Player")) EnemyShooting(col);
	}

	private void OnTriggerExit(Collider other)
	{
		if (IsDead) return;
		isFire = false;
		_agent.isStopped = false;
		fireTarget = null;
	}

	private void EnemyShooting(Component col)
	{
		isFire = true;
		_agent.isStopped = true;
		fireTarget = col.transform;
		cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation,
			Quaternion.LookRotation(fireTarget.position - cachedTransform.position), 10 * Time.deltaTime);
		characterWeapon.Shoot(); // Starting the shooting effect
		if (!characterWeapon.IsShot) return;
		if (col.CompareTag("Team"))
		{
			var teamController = col.GetComponent<TeamController>();
			var enemyController = teamController.targetScript.Get<EnemyController>();
			if (enemyController) enemyController.TakeDamage(TotalDamage, this);
			else _playerScript.TakeDamage(TotalDamage);
		}
		if (col.CompareTag("Enemy"))
			col.GetComponent<EnemyController>().TakeDamage(TotalDamage, this);
		else _playerScript.TakeDamage(TotalDamage);
	}

	public void TakeDamage(float damage, EnemyController enemyController = null)
	{
		if (IsDead) return;
		health -= damage;
		if (CharacterCount == 0)
		{
			bloodFX.Play();
			if (health > 1) return;
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
			while (health < 1)
			{
				if (CharacterCount > 0)
				{
					var deadCharacter = Random.Range(0, CharacterCount);
					characterList[deadCharacter].DeathPlay();
					health += previousHealth;
					rankManager.ChangeRating();
				}
				else
				{
					DeathPlay(enemyController);
					break;
				}
			}
		}
	}

	private void AddKill()
	{
		scoreKills += 1;
		// Updating weapons to the main man
		weapons.ChangeWeapon(scoreKills);
		shootingArea.size = new Vector3(characterWeapon.FireRange * 6, 1, characterWeapon.FireRange * 6);
		// Updating weapons to all the player's teammates
		_levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
		foreach (var people in characterList) people.LevelUp();
		rankManager.ChangeRating();
	}

	private void DeathPlay(EnemyController enemyController = null)
	{
		IsDead = true;
		_agent.isStopped = true;
		capsuleCollider.enabled = false;
		rigidbody.isKinematic = true;
		animator.SetBool(DeadAnim, true);
		PointerManager.instance.RemoveFromList(point);
		rankManager.charactersData.Remove(this);
		StartCoroutine(DeadTimer());
		if (enemyController) enemyController.AddKill();
		else _playerScript.AddKill();
	}

	private IEnumerator DeadTimer()
	{
		yield return new WaitForSeconds(5f);
		enemySpawner.SpawnObject();
		Destroy(gameObject);
	}

	private void FindClosestFood()
	{
		var closestDistance = Mathf.Infinity;
		Transform closestPeople = null;
		foreach (var person in _foods)
		{
			var currentDistance = Vector3.Distance(cachedTransform.position, person.transform.position);
			if (currentDistance > closestDistance) continue;
			closestDistance = currentDistance;
			closestPeople = person.transform;
		}
		_agent.destination = _playerPrefab.transform.position;
	}
}