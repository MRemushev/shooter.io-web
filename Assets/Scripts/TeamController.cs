using System.Collections;
using NTC.Global.Cache;
using NTC.Global.Pool;
using UnityEngine;
using static NTC.Global.Pool.NightPool;
using Random = UnityEngine.Random;


public class TeamController : MonoCache, IPoolItem
{
	[SerializeField] private new Rigidbody rigidbody;
	[SerializeField] private Animator animator;
	[SerializeField] private Renderer skinObject;
	[SerializeField] private CapsuleCollider capsuleCollider;
	[SerializeField] private float speedMove;
	[SerializeField] private ParticleSystem takeFX;
	[Space]
	public WeaponSwitch weapons;
	public ParticleSystem bloodFX;
	[HideInInspector] public Transform cachedTransform;
	[HideInInspector] public MainCharacter targetScript;

	private WeaponController _thisWeapon;
	private int _teamIndex;
	private Transform _targetTransform;
	private Vector3 _relativeVector;
	public bool IsDead { get; private set; }

	private static readonly int
		Horizontal = Animator.StringToHash("Horizontal"), Vertical = Animator.StringToHash("Vertical"),
		DeadAnim = Animator.StringToHash("IsDead");

	private void Awake() => cachedTransform = transform;

	public void OnSpawn() => takeFX.Play();

	public void SetTarget(MainCharacter targetObject, Material targetSkin, Transform targetTransform)
	{
		targetScript = targetObject;
		skinObject.material = targetSkin;
		_targetTransform = targetTransform;
		weapons.ChangeWeapon(targetScript.weapons.WeaponLevel);
		_thisWeapon.fireRate *= Random.Range(0.1f, 0.9f);
		targetScript.characterList.Add(this);
		_teamIndex = targetObject.characterList.Count;
		var isPlayer = targetObject.GetCached<PlayerController>();
		if (isPlayer) isPlayer.ChangeStats();
	}

	public void OnDespawn()
	{
		IsDead = false;
		targetScript = null;
		_targetTransform = null;
		capsuleCollider.enabled = true;
	}

	protected override void Run()
	{
		if (IsDead) return;
		rigidbody.isKinematic = targetScript.IsStopped;
		animator.SetFloat(Horizontal, targetScript.relativeVector.x);
		animator.SetFloat(Vertical, targetScript.relativeVector.z);
		if (targetScript.isFire) _thisWeapon.Shoot(_teamIndex < 5);
		if (targetScript.fireTarget)
			cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation,
				Quaternion.LookRotation(targetScript.fireTarget.position - cachedTransform.position), 10 * Time.deltaTime);
		else cachedTransform.rotation = _targetTransform.rotation;
	}

	protected override void FixedRun()
	{
		if (IsDead) return;
		if (!rigidbody.isKinematic)
			rigidbody.position = Vector3.Lerp(cachedTransform.position, _targetTransform.position, speedMove);
	}

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.CompareTag("Food")) targetScript.AddCharacter(cachedTransform.position, 1, col);
		if (col.gameObject.CompareTag("FoodBox")) targetScript.AddCharacter(cachedTransform.position, 5, col);
	}

	public void LevelUp()
	{
		weapons.ChangeWeapon(targetScript.weapons.WeaponLevel);
		_thisWeapon.fireRate *= Random.Range(0.1f, 0.9f);
	}

	public void DeathPlay()
	{
		targetScript.characterList.Remove(this);
		IsDead = true;
		rigidbody.isKinematic = true;
		capsuleCollider.enabled = false;
		animator.SetBool(DeadAnim, true);
		StartCoroutine(DeadTimer());
	}

	private IEnumerator DeadTimer()
	{
		yield return new WaitForSeconds(5f);
		NightPool.Despawn(gameObject);
	}

	public void ChangeWeapon(WeaponController weapon) => _thisWeapon = weapon;
}
