using UnityEngine.AI;
using UnityEngine;
using Random = UnityEngine.Random;
using NTC.Global.Pool;
using System.Collections;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MainCharacter
{
	[Header("Enemy components")]
	[SerializeField] private NavMeshAgent agent;
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
		_playerPrefab = GameObject.Find("Player");
		_playerScript = _playerPrefab.GetComponent<PlayerController>();
		if (_playerScript.CharacterCount < PlayerPrefs.GetInt("PlayerPeople"))
			AddCharacter(CachedTransform.position, Random.Range(0, PlayerPrefs.GetInt("PlayerPeople")));
		else AddCharacter(CachedTransform.position, Random.Range(0, _playerScript.CharacterCount));
		if (_playerScript.CountKills / 2 < PlayerPrefs.GetInt("WeaponLevel"))
			ScoreKills = Random.Range(0, PlayerPrefs.GetInt("WeaponLevel"));
		else ScoreKills = Random.Range(0, _playerScript.CountKills / 2);
		weapons.ChangeWeapon(ScoreKills);
		PointerManager.Instance.AddToList(point);
		_levelText = PointerManager.Instance.Dictionary[point].GetComponent<PointerIcon>();
		_levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
		shootingArea.size = new Vector3(CharacterWeapon.FireRange * 6, 1, CharacterWeapon.FireRange * 6);
		Health = PreviousHealth = 100 + PlayerPrefs.GetInt("PlayerHealth") * 10 + _playerScript.CountKills * 10;
	}

	private void Start() => _foods = Finds<FoodMovement>();

	protected override void Run()
	{
		if (IsDead) return;
		// Movement enemy
		relativeVector = Vector3.ClampMagnitude(transform.InverseTransformDirection(agent.velocity), 1);
		animator.SetFloat(Horizontal, relativeVector.x);
		animator.SetFloat(Vertical, relativeVector.z);
		IsStop = relativeVector.magnitude < 0.1f;
		// If the agent is stuck, then we try to find a new target
		if (IsStop && !agent.isStopped) FindClosestFood();
	}

	private void OnCollisionEnter(Collision col)
	{
		// Picking up food and looking for a new target
		if (col.gameObject.CompareTag("Food")) AddCharacter(CachedTransform.position, 1, col);
		else if (col.gameObject.CompareTag("FoodBox")) AddCharacter(CachedTransform.position, 5, col);
		else return;
		RankManager.ChangeRating();
	}

	private void OnTriggerStay(Collider col)
	{
		if (IsDead) return;
		if (Vector3.Distance(CachedTransform.position, col.transform.position) > CharacterWeapon.FireRange) return;
		if (col.CompareTag("Team") && col.GetComponent<CapsuleCollider>().enabled) EnemyShooting(col);
		else if (col.CompareTag("Enemy") && col.GetComponent<CapsuleCollider>().enabled) EnemyShooting(col);
		else if (col.CompareTag("Player")) EnemyShooting(col);
		else FireReset();
	}

	private void OnTriggerExit(Collider other)
	{
		if (IsDead) return;
		FireReset();
	}

	private void EnemyShooting(Component col)
	{
		isFire = true;
		agent.isStopped = true;
		fireTarget = col.transform;
		CachedTransform.rotation = Quaternion.Lerp(CachedTransform.rotation,
			Quaternion.LookRotation(fireTarget.position - CachedTransform.position), 10 * Time.deltaTime);
		CharacterWeapon.Shoot(); // Starting the shooting effect
		if (!CharacterWeapon.IsShot) return;
		if (col.CompareTag("Team"))
		{
			var teamController = col.GetComponent<TeamController>();
			var enemyController = teamController.targetScript.Get<EnemyController>();
			if (enemyController) enemyController.TakeDamage(TotalDamage, this);
			else _playerScript.TakeDamage(this, TotalDamage);
		}
		if (col.CompareTag("Enemy"))
			col.GetComponent<EnemyController>().TakeDamage(TotalDamage, this);
		else _playerScript.TakeDamage(this, TotalDamage);
	}

	public void TakeDamage(float damage, EnemyController enemyController = null)
	{
		if (IsDead) return;
		Health -= damage;
		if (CharacterCount == 0)
		{
			bloodFX.Play();
			if (Health > 1) return;
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
					characterList[deadCharacter].DeathPlay();
					Health += PreviousHealth;
					RankManager.ChangeRating();
					if (enemyController) enemyController.FireReset();
					else _playerScript.FireReset();
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
		FireReset();
		ScoreKills += 1;
		// Updating weapons to the main man
		weapons.ChangeWeapon(ScoreKills);
		shootingArea.size = new Vector3(CharacterWeapon.FireRange * 6, 1, CharacterWeapon.FireRange * 6);
		// Updating weapons to all the player's teammates
		_levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
		foreach (var people in characterList) people.LevelUp();
		RankManager.ChangeRating();
	}

	private void DeathPlay(EnemyController enemyController = null)
	{
		IsDead = true;
		agent.enabled = false;
		capsuleCollider.enabled = false;
		shootingArea.enabled = false;
		animator.SetBool(DeadAnim, true);
		PointerManager.Instance.RemoveFromList(point);
		RankManager.charactersData.Remove(this);
		StartCoroutine(DeadTimer());
		if (enemyController) enemyController.AddKill();
		else _playerScript.AddKill();
	}

	public void FireReset()
	{
		isFire = false;
		agent.isStopped = false;
		fireTarget = null;
	}

	private IEnumerator DeadTimer()
	{
		yield return new WaitForSeconds(5f);
		EnemySpawner.SpawnObject();
		Destroy(gameObject);
	}

	private void FindClosestFood()
	{
		if (TotalDamage > _playerScript.TotalDamage * 1.25f) agent.destination = EnemySpawner.RandomPosition();
		else
		{
			var closestDistance = Mathf.Infinity;
			Transform closestPeople = null;
			foreach (var person in _foods)
			{
				var currentDistance = Vector3.Distance(CachedTransform.position, person.transform.position);
				if (currentDistance > closestDistance) continue;
				closestDistance = currentDistance;
				closestPeople = person.transform;
			}
			agent.destination = closestPeople ? closestPeople.position : _playerPrefab.transform.position;
		}
	}
}