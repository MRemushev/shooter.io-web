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
	public Transform fireTarget;
	public string characterName;
	public bool isFire;
	[HideInInspector] public Vector3 relativeVector;
	[Space]

	private Spawner _foodSpawner;
	private Spawner _foodBoxSpawner;
	protected WeaponController characterWeapon;
	protected Transform cachedTransform;
	protected RankManager rankManager;
	protected int scoreKills;
	protected float health;
	protected Spawner enemySpawner;
	protected int previousHealth;
	protected bool isStop;
	protected static readonly int
		Horizontal = Animator.StringToHash("Horizontal"), Vertical = Animator.StringToHash("Vertical");

	private Material MainSkin => skinObject.material;
	public float TotalDamage => (characterList.Count + 1) * characterWeapon.Damage;
	public bool IsStopped => isStop;
	public int CountKills => scoreKills;
	public int CharacterCount => characterList.Count;
	public int CharacterScore => (int)(characterWeapon.DamagePerSecond * (CharacterCount + 1));

	private void Awake()
	{
		cachedTransform = GetComponent<Transform>();
		var spawners = FindObjectOfType<Spawner>().GetComponents<Spawner>();
		_foodSpawner = spawners[0];
		enemySpawner = spawners[1];
		_foodBoxSpawner = spawners[2];
		rankManager = FindObjectOfType<RankManager>();
		rankManager.charactersData.Add(this);
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
			teamController.SetTarget(this, MainSkin, transform);
		}
		if (col == null) return;
		Despawn(col.gameObject);
		if (count > 1) _foodBoxSpawner.SpawnObject(); // Spawn a new food
		else _foodSpawner.SpawnObject();
	}

	public void ChangeWeapon(WeaponController weapon)
	{
		if (weapon != null) characterWeapon = weapon;
	}
}
