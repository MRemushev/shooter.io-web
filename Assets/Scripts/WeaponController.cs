using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject mainCharacter;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem shootFX;
    [SerializeField] private float damage;
    [SerializeField] private float fireRange;
    [SerializeField] private bool isRifle;
    
    private float _nextFire;
    private Animator _animator;

    public float fireRate;
    private static readonly int IsRifle = Animator.StringToHash("IsRifle");

    public float Damage => damage;
    public float DamagePerSecond => Mathf.Round(damage * fireRate);
    public float FireRange => fireRange;
    public bool IsShot { get; private set; }

    private void OnEnable()
    {
        switch (mainCharacter.tag)
        {
            case "Player":
                mainCharacter.GetComponent<PlayerController>().ChangeWeapon(this);
                break;
            case "Team":
                mainCharacter.GetComponent<TeamController>().ChangeWeapon(this);
                break;
            case "Enemy":
                mainCharacter.GetComponent<EnemyController>().ChangeWeapon(this);
                break;
        }
        mainCharacter.GetComponent<Animator>().SetBool(IsRifle, isRifle);
    }

    public void Shoot()
    {
        IsShot = false;
        if (Time.time < _nextFire) return;
        _nextFire = Time.time + 1f / fireRate;
        IsShot = true;
        shootFX.Play();
        audioSource.pitch = Random.Range(0.7f, 1.1f);
        audioSource.Play();
    }
}