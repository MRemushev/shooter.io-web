using System.Collections;
using NTC.Global.Cache;
using NTC.Global.Pool;
using UnityEngine;
using Random = UnityEngine.Random;


public class TeamController : MonoCache
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
	private Vector3 _relativeVector;
	private bool IsDead { get; set; }

	private static readonly int
		Horizontal = Animator.StringToHash("Horizontal"), Vertical = Animator.StringToHash("Vertical"),
		DeadAnim = Animator.StringToHash("IsDead");

	private void Awake() => cachedTransform = transform;

	protected override void OnEnabled() => takeFX.Play();

	public void SetTarget(MainCharacter targetComponent, Material targetSkin)
	{
		targetScript = targetComponent;
		skinObject.material = targetSkin;
		weapons.ChangeWeapon(targetScript.weapons.WeaponLevel);
		_thisWeapon.fireRate *= Random.Range(0.1f, 0.9f);
		targetScript.characterList.Add(this);
		var isPlayer = targetScript.GetCached<PlayerController>();
		if (isPlayer) isPlayer.ChangeStats();
	}

	protected override void OnDisabled()
	{
		IsDead = false;
		targetScript = null;
		capsuleCollider.enabled = true;
	}

	protected override void FixedRun()
	{
		if (IsDead) return;
		rigidbody.isKinematic = targetScript.IsStop;
		rigidbody.MoveRotation(targetScript.cachedTransform.rotation);
		if (!targetScript.IsStop) 
			rigidbody.MovePosition(Vector3.Lerp(cachedTransform.position, targetScript.cachedTransform.position, speedMove));
		animator.SetFloat(Horizontal, targetScript.relativeVector.x);
		animator.SetFloat(Vertical, targetScript.relativeVector.z);
		if (targetScript.attackTarget) _thisWeapon.Shoot();
	}

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.CompareTag("Food")) StartCoroutine(targetScript.AddCharacter(cachedTransform.position, 1, col));
		if (col.gameObject.CompareTag("FoodBox")) StartCoroutine(targetScript.AddCharacter(cachedTransform.position, 5, col));
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
