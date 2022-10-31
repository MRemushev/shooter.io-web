using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class PlayerController : MainCharacter
{
    [Header("Player components")]
    [SerializeField] private LineRenderer laserBeam;
    [SerializeField] private Joystick walkJoystick;
    [SerializeField] private GameObject deadScreen;
    [SerializeField] private TextMeshProUGUI countTeamText;
    [SerializeField] private TextMeshProUGUI killsCountText;
    [SerializeField] private TextMeshProUGUI weaponLevelText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI weaponStatsText;
    [SerializeField] private float movementSpeed;

    private int _killPromotion = 2;
    private CameraController _cameraOffset;
    private Vector3 _movementVector;

    protected override void OnEnabled() => rankManager.charactersData.Add(this);
    protected override void OnDisabled() => rankManager.charactersData.Remove(this);
    
    private void Start()
    {
        skinObject.material.mainTexture = skinArray.textureList[PlayerPrefs.GetInt("PlayerSkin")];
        _cameraOffset = Find<CameraController>();
        characterName = PlayerPrefs.GetString("PlayerName");
        weapons.ChangeWeapon(PlayerPrefs.GetInt("WeaponLevel"));
        weaponLevelText.text = (weapons.WeaponLevel + 1).ToString();
        shootingArea.size = new Vector3(characterWeapon.FireRange * 6, 1, characterWeapon.FireRange * 6);
        laserBeam.SetPosition(1, new Vector3(0, 2.2f, characterWeapon.FireRange * 3));
        _cameraOffset.ChangeOffset(characterWeapon.FireRange * 10);
        previousHealth = 100 + PlayerPrefs.GetInt("PlayerHealth") * 10;
        ChangeWeaponStatsText();
        ChangeHpText();
        Renaissance();
    }

    protected override void Run()
    {
        // Movement player
        _movementVector = new Vector3(walkJoystick.Horizontal, 0, walkJoystick.Vertical).normalized;
        relativeVector = cachedTransform.InverseTransformDirection(_movementVector);
        animator.SetFloat(Horizontal, relativeVector.x);
        animator.SetFloat(Vertical, relativeVector.z);
        isStop = rigidbody.velocity == Vector3.zero;
    }

    protected override void FixedRun()
    {
        rigidbody.velocity = _movementVector * movementSpeed;
        // If the player does not shoot, then we perform a turn according to the player's movement
        if (_movementVector != Vector3.zero && !isFire) cachedTransform.rotation = Quaternion.LookRotation(_movementVector);
    }

    private void OnTriggerStay(Collider col) // Shooting area stay
    {
        if (Vector3.Distance(cachedTransform.position, col.transform.position) > characterWeapon.FireRange) return;
        if (col.CompareTag("Team") && !col.GetComponent<TeamController>().IsDead) AutoShooting(col);
        else if (col.CompareTag("Enemy") && !col.GetComponent<EnemyController>().IsDead) AutoShooting(col);
    }

    private void OnTriggerExit(Collider col)
    {
        isFire = false;
        fireTarget = null;
        laserBeam.enabled = false;
    }

    private void OnCollisionEnter(Collision col)
    {
        // If the player touched an object with the tag "Food", then we call the function of adding a team
        if (col.gameObject.CompareTag("Food"))
        {
            AddCharacter(cachedTransform.position, 1, col);
            ChangeStats();
        }
        else if (col.gameObject.CompareTag("FoodBox"))
        {
            AddCharacter(cachedTransform.position, 5, col);
            ChangeStats();
        }
    }

    private void ChangeHpText() =>
        hpText.text = "HP " + Mathf.Max(0,Mathf.Round(CharacterCount * previousHealth + health));

    private void ChangeWeaponStatsText() =>
        weaponStatsText.text = characterWeapon.gameObject.name + " - " + characterWeapon.DamagePerSecond;

    public void ChangeStats()
    {
        countTeamText.text = CharacterCount.ToString();
        var score = (int)((CharacterCount + 1) * characterWeapon.DamagePerSecond);
        if (PlayerPrefs.GetInt("HighScore") < score) PlayerPrefs.SetInt("HighScore", score);
        ChangeHpText();
        rankManager.ChangeRating();
    }

    private void AutoShooting(Component col)
    {
        isFire = true;
        laserBeam.enabled = true;
        // We turn in the direction of the shot
        fireTarget = col.transform;
        cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation, Quaternion.LookRotation(fireTarget.position - cachedTransform.position), 10 * Time.deltaTime);
        characterWeapon.Shoot(); // Starting the shooting effect
        if (!characterWeapon.IsShot) return;
        var isEnemy = col.GetComponent<EnemyController>();
        if (isEnemy) isEnemy.TakeDamage(TotalDamage);
        else col.GetComponent<TeamController>().targetScript.GetComponent<EnemyController>().TakeDamage(TotalDamage);
    }

    // Damage acceptance function
    public void TakeDamage(float damage)
    {
        if (damage < 1) return; // Check that the damage is not less than one
        health -= damage;
        ChangeHpText();
        // Check if the player has teammates
        if (CharacterCount == 0)
        {
            bloodFX.Play();
            if (health > 1) return; // Check how much health the player has
            var gameManager = Find<GameManager>();
            gameManager.UpdatePriceChance();
            deadScreen.SetActive(true); // Calling the screen of death
            gameManager.SetPause(true); // Putting the game on pause
        }
        else
        {
            characterList[Random.Range(0, CharacterCount)].bloodFX.Play();
            if (health > 1) return;
            while (health < 1)
            {
                if (CharacterCount > 0)
                {
                    var deadCharacter = Random.Range(0, CharacterCount);
                    characterList[deadCharacter].DeathPlay();
                    countTeamText.text = CharacterCount.ToString();
                    health += previousHealth;
                    rankManager.ChangeRating();
                }
                else
                {
                    deadScreen.SetActive(true); // Calling the screen of death
                    Find<GameManager>().SetPause(true);
                    break;
                }
            }
        }
    }

    public void AddKill()
    {
        scoreKills += 1;
        killsCountText.text = scoreKills.ToString();
        _killPromotion -= 1;
        if (_killPromotion != 0) return;
        var weaponLevel = PlayerPrefs.GetInt("WeaponLevel") + CountKills / 2;
        weapons.ChangeWeapon(weaponLevel); // Updating weapons to the main man
        weaponLevelText.text = (weaponLevel + 1).ToString();
        // Update fire area
        shootingArea.size = new Vector3(characterWeapon.FireRange * 6, 1, characterWeapon.FireRange * 6);
        laserBeam.SetPosition(1, new Vector3(0, 2.1f, characterWeapon.FireRange * 3));
        _cameraOffset.ChangeOffset(characterWeapon.FireRange * 10);
        ChangeWeaponStatsText();
        rankManager.ChangeRating(); // Update rating
        // Updating weapons to all the player's teammates
        foreach (var people in characterList) people.LevelUp();
        _killPromotion = 2;
    }

    public void Renaissance()
    {
        weapons.ChangeWeapon(PlayerPrefs.GetInt("WeaponLevel") + CountKills / 2);
        health = previousHealth;
        _cameraOffset.ChangeOffset(characterWeapon.FireRange * 10);
        if (PlayerPrefs.HasKey("PlayerPeople"))
            AddCharacter(cachedTransform.position, PlayerPrefs.GetInt("PlayerPeople"));
    }
}