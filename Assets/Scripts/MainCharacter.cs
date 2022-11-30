using System.Collections.Generic;
using NTC.Global.Cache;
using static NTC.Global.Pool.NightPool;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(BoxCollider))]
public class MainCharacter : MonoCache
{
	[Header("Character components")]
	[SerializeField] protected Animator animator;
	[SerializeField] protected BoxCollider shootingArea;
	[SerializeField] protected Renderer skinObject;
	[SerializeField] protected TextureList skinArray;
	[SerializeField] protected ParticleSystem bloodFX;
	[SerializeField] private GameObject characterTeam;
	[Space]
	public WeaponSwitch weapons;
	public List<TeamController> characterList;
	public Transform attackTarget;
	public string characterName;
	[HideInInspector] public Vector3 relativeVector;
	[HideInInspector] public Transform cachedTransform;
	[Space]

	private Spawner _foodSpawner;
	private Spawner _foodBoxSpawner;
	protected WeaponController CharacterWeapon;
	protected RankManager RankManager;
	protected int ScoreKills;
	protected float Health;
	protected Spawner EnemySpawner;
	protected int PreviousHealth;
	protected bool IsStop;
	protected static readonly int
		Horizontal = Animator.StringToHash("Horizontal"), Vertical = Animator.StringToHash("Vertical");

	private Material MainSkin => skinObject.material;
	public float TotalDamage => (characterList.Count + 1) * CharacterWeapon.Damage;
	public bool IsStopped => IsStop;
	public int CountKills => ScoreKills;
	public int CharacterCount => characterList.Count;
	public int CharacterScore => (int)(CharacterWeapon.DamagePerSecond * (CharacterCount + 1));

	private void Awake()
	{
		cachedTransform = GetComponent<Transform>();
		var spawners = FindObjectsOfType<Spawner>();
		_foodSpawner = spawners[0];
		EnemySpawner = spawners[1];
		_foodBoxSpawner = spawners[2];
		RankManager = FindObjectOfType<RankManager>();
		RankManager.charactersData.Add(this);
	}

	// The function of adding a teammate
	public void AddCharacter(Vector3 position, int count = 1, Collision col = null)
	{
		for (var i = 0; i < count; i++)
		{
			Vector3 characterPosition;
			if (count > 1) characterPosition = position + Random.insideUnitSphere * (count * (4f / count));
			else characterPosition = position + Random.insideUnitSphere * (count / 2f);
			characterPosition.y = position.y;
			var teamController = Spawn(characterTeam, characterPosition).GetComponent<TeamController>();
			teamController.SetTarget(this, MainSkin);
		}
		if (col == null) return;
		Despawn(col.gameObject);
		if (count > 1) _foodBoxSpawner.SpawnObject(); // Spawn a new food
		else _foodSpawner.SpawnObject();
	}

	public void ChangeWeapon(WeaponController weapon) => CharacterWeapon = weapon;
}
