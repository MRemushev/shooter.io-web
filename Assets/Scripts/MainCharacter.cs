using System.Collections.Generic;
using NTC.Global.Cache;
using static NTC.Global.Pool.NightPool;
using UnityEngine;

using Random = UnityEngine.Random;

public class MainCharacter : MonoCache
{
    [Header("Character components")]
    [SerializeField] protected new Rigidbody rigidbody;
    [SerializeField] protected Animator animator;
    [SerializeField] protected BoxCollider shootingArea;
    [SerializeField] protected Renderer skinObject;
    [SerializeField] protected TextureList skinArray;
    [SerializeField] protected ParticleSystem bloodFX;
    [SerializeField] private GameObject characterTeam;
    [Space]
    public WeaponSwitch weapons;
    public List<TeamController> characterList;
    [HideInInspector] public Vector3 relativeVector;
    [HideInInspector] public string characterName;
    [HideInInspector] public bool isFire;
    public Transform fireTarget;
    protected WeaponController characterWeapon;
    [Space]

    private Spawner _foodSpawner;
    private Spawner _foodBoxSpawner;
    protected Transform cachedTransform;
    protected RankManager rankManager;
    protected int scoreKills;
    protected float health;
    protected Spawner enemySpawner;
    protected FoodMovement[] foods;
    protected int previousHealth;
    protected bool isStop;
    protected static readonly int 
        Horizontal = Animator.StringToHash("Horizontal"), Vertical = Animator.StringToHash("Vertical");

    private Material MainSkin => skinObject.material;
    protected float TotalDamage => (characterList.Count + 1) * characterWeapon.Damage;
    public bool IsStopped => isStop;
    public int CountKills => scoreKills;
    public int CharacterCount => characterList.Count;
    public int CharacterScore => (int)(characterWeapon.DamagePerSecond * (CharacterCount + 1));

    private void Awake()
    {
        cachedTransform = transform;
        var spawners = FindObjectOfType<Spawner>().GetComponents<Spawner>();
        _foodSpawner = spawners[0];
        enemySpawner = spawners[1];
        _foodBoxSpawner = spawners[2];
        foods = Finds<FoodMovement>();
    }

    // The function of adding a teammate
    public void AddCharacter(int count = 1, Collision col = null)
    {
        TeamController teamController;
        var mainPosition = transform.position;
        if (count == 1)
        {
            teamController = col != null && col.gameObject.activeSelf
                ? Spawn(characterTeam, col.transform.position).GetComponent<TeamController>()
                : Spawn(characterTeam, mainPosition).GetComponent<TeamController>();
            teamController.SetTarget(this, MainSkin, transform);
        }
        else
        {
            
            for (var i = 0; i < count; i++)
            {
                if (col != null)
                {
                    var characterPosition = col.transform.position + Random.insideUnitSphere * (count / 4f);
                    characterPosition.y = mainPosition.y;
                    teamController = Spawn(characterTeam, characterPosition).GetComponent<TeamController>();
                }
                else
                {
                    var characterPosition = mainPosition + Random.insideUnitSphere * (count / 4f);
                    characterPosition.y = mainPosition.y;
                    teamController = Spawn(characterTeam, characterPosition).GetComponent<TeamController>();
                }
                teamController.SetTarget(this, MainSkin, transform);
            }
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
