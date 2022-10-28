﻿using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;
using NTC.Global.Pool;

[RequireComponent(typeof(AIPath))]
public class EnemyController : MainCharacter, IPoolItem
{
    [Header("Enemy components")]
    [SerializeField] private AIPath agent;
    [SerializeField] private Transform point;
    [SerializeField] private CapsuleCollider capsuleCollider;

    private FoodMovement[] _foods;
    private PointerIcon _levelText;
    private GameObject _playerPrefab;
    private PlayerController _playerScript;
    private float _previousHeath;
    public bool IsDead { get; private set; }

    private static readonly int DeadAnim = Animator.StringToHash("IsDead");

    public void OnSpawn()
    {
        PointerManager.instance.AddToList(point);
        _levelText = PointerManager.instance.dictionary[point].GetComponent<PointerIcon>();
        _levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
        characterName = NameRandomizer.GetRandomName();
        skinObject.material.mainTexture = skinArray.textureList[Random.Range(0, skinArray.textureList.Length)];
        scoreKills = Random.Range(0, PlayerPrefs.GetInt("WeaponLevel"));
        _playerPrefab = GameObject.Find("Player");
        _playerScript = _playerPrefab.GetComponent<PlayerController>();
        AddCharacter(cachedTransform.position, Random.Range(0, _playerScript.CharacterCount));
        weapons.ChangeWeapon(scoreKills);
        shootingArea.size = new Vector3(characterWeapon.FireRange * 6, 1, characterWeapon.FireRange * 6);
        health = previousHealth = 100 + _playerScript.CountKills * 10;
    }

    public void OnDespawn() => enemySpawner.SpawnObject();


    private void Start()
    {
        _foods = Finds<FoodMovement>();
        rankManager.charactersData.Add(this);
    }

    protected override void Run()
    {
        if (IsDead) return;
        // Movement enemy
        relativeVector = Vector3.ClampMagnitude(transform.InverseTransformDirection(agent.velocity), 1);
        animator.SetFloat(Horizontal, relativeVector.x);
        animator.SetFloat(Vertical, relativeVector.z);
        isStop = relativeVector.magnitude < 0.1f;
        // If the agent is stuck, then we try to find a new target
        if (isStop && !agent.isStopped) FindClosestFood();
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
        agent.isStopped = false;
        fireTarget = null;
    }

    private void EnemyShooting(Component col)
    {
        isFire = true;
        fireTarget = col.transform;
        agent.isStopped = fireTarget;
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
            col.GetComponent<EnemyController>().TakeDamage(TotalDamage,this);
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
        var weaponLevel = PlayerPrefs.GetInt("WeaponLevel") + CountKills;
        // Updating weapons to the main man
        weapons.ChangeWeapon(weaponLevel);
        shootingArea.size = new Vector3(characterWeapon.FireRange * 6, 1, characterWeapon.FireRange * 6);
        // Updating weapons to all the player's teammates
        _levelText.countText.text = (weapons.WeaponLevel + 1).ToString();
        foreach (var people in characterList) people.LevelUp();
        rankManager.ChangeRating();
    }

    private void DeathPlay(EnemyController enemyController = null)
    {
        IsDead = true;
        agent.isStopped = true;
        rigidbody.isKinematic = true;
        capsuleCollider.enabled = false;
        animator.SetBool(DeadAnim, true);
        PointerManager.instance.RemoveFromList(point);
        rankManager.charactersData.Remove(this);
        NightPool.Despawn(gameObject, 5f);
        if (enemyController) enemyController.AddKill();
        else _playerScript.AddKill();

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
        agent.destination = closestPeople!.position;
    }
}