using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

using Photon.Pun;

public class PlayerCode : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private Animator playerSpriteAnimator;
    [SerializeField] private ComboCode combo;
    [SerializeField] private GameObject plus;
    [SerializeField] private GameObject burst;
    [SerializeField] private GameObject confetti;
    [SerializeField] private AudioClip gameWin;
    [SerializeField] private GameObject gameLose;
    [SerializeField] private GameObject playerDamage;

    [Header("Effects")]
    [SerializeField] private ParticleSystem fire;
    [SerializeField] private GameObject ice;
    [SerializeField] private GameObject iceBreak;
    [SerializeField] private GameObject lightningBolt;

    [Header("Input")]
    [SerializeField] private InputMultiplayer inputMultiplayer;

    [SerializeField] private Button rematchButton;
    [SerializeField] private GameObject rematchText;

    private float health;

    private bool hasWon;
    private float timeCompleted;
    private int goldGained;
    private int wordsSpelled;
    private string longestWord;
    private int highestCombo;
    private int lettersUsed;
    private string enemyName;

    private float burnTimer;
    private float freezeTimer;
    private bool burning;

    private bool hasLeft;
    
    // Start is called before the first frame update
    void Awake()
    {
        maxHealth = 100 + (PlayerPrefs.GetInt("healthLevel") * 50);
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(burnTimer > 0) {
            burning = true;
            burnTimer -= Time.deltaTime;

            if(Random.Range(0, 3) == 0) {
                if(inputMultiplayer != null) {
                    health -= inputMultiplayer.GetEnemyAttackLevel() / 100f;
                    inputMultiplayer.UpdateHealthBar(health / maxHealth);

                    if(health <= 0) {
                        Instantiate(burst, new Vector3(-7.28f, -2.6f, 0), new Quaternion(0, 0, 0, 1));
                        inputMultiplayer.MultiplayerEndGame();
                    }
                }
            }
            
            if(burnTimer <= 0) {
                if(burning)
                    if(inputMultiplayer != null)
                        inputMultiplayer.StopBurning();
                burning = false;
                fire.Stop();
            }
        }

        if(freezeTimer > 0) {
            freezeTimer -= Time.deltaTime;
            
            if(freezeTimer <= 0) {
                if(inputMultiplayer != null)
                    inputMultiplayer.Unfreeze();
                Instantiate(iceBreak, new Vector3(-7.28f, -3.23f, -5), new Quaternion(0, 0, 0, 1));
                ice.SetActive(false);
            }
        }

        healthBar.fillAmount = health / maxHealth;

        if(!hasWon)
            timeCompleted += Time.deltaTime;
    }

    void FixedUpdate() {
        if(hasWon) {
            if(transform.position.x < 2.5f) {
                transform.position = new Vector3(transform.position.x + 0.025f, transform.position.y, transform.position.z);

                if(transform.position.x > 2.5f) {
                    transform.position = new Vector3(2.5f, transform.position.y, transform.position.z);
                    playerSpriteAnimator.SetBool("Walking", false);
                }
            }
        }
    }

    public void Damage(float num, float burnTime, float freezeTime, bool shocked, float healingFactor) {
        health -= num;

        if(burnTime > 0) {
            burnTimer = burnTime;
            fire.Play();
        }
        if(freezeTime > 0) {
            if(inputMultiplayer != null)
                inputMultiplayer.Freeze();
            freezeTimer = freezeTime;
            ice.SetActive(true);
            ice.GetComponent<Animator>().SetTrigger("MultiplayerFreeze");
        }
        if(shocked) {
            if(inputMultiplayer != null)
                inputMultiplayer.Shock();
            GameObject bolt = Instantiate(lightningBolt, new Vector3(-7.28f, 0.85f, -6), new Quaternion(0, 0, 0, 1));
            bolt.GetComponent<Animator>().SetTrigger("Self");
            Destroy(bolt, 0.35f);
        }
        if(healingFactor > 0) {
            float number = 0;
            if(num > health)
                number = health;
            else
                number = num;
            
            if(inputMultiplayer != null) {
                inputMultiplayer.HealEnemy(number / maxHealth * healingFactor);
                Instantiate(plus, new Vector3(-2.24f, -3.6f, -5), Quaternion.Euler(-90, 0, 0));
            }
        }

        Instantiate(playerDamage, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);

        if(inputMultiplayer != null)
            inputMultiplayer.UpdateHealthBar(health / maxHealth);
            
        combo.ResetCombo();
        playerSpriteAnimator.SetTrigger("Damaged");

        if(health <= 0) {
            Instantiate(burst, new Vector3(-7.28f, -2.6f, 0), new Quaternion(0, 0, 0, 1));

            if(inputMultiplayer != null)
                inputMultiplayer.MultiplayerEndGame();
            else
                GameObject.Find("Input").GetComponent<InputCode>().EndGame();
        }
    }

    public void Heal(float num) {
        if(health > 0 && health < maxHealth) {
            health += num * maxHealth;

            if(health > maxHealth)
                health = maxHealth;

            Instantiate(plus, new Vector3(-7.28f, -3.6f, -5), Quaternion.Euler(-90, 0, 0));

            if(inputMultiplayer != null)
                inputMultiplayer.UpdateHealthBar(health / maxHealth);
        }
    }

    public void EndGame() {
        if(inputMultiplayer != null)
            GameObject.Find("GameUI").transform.GetChild(2).gameObject.SetActive(true);
        else
            GameObject.Find("GameUI").transform.GetChild(0).gameObject.SetActive(true);

        fire.Stop();
        ice.SetActive(false);
        if(freezeTimer > 0) {
            freezeTimer = 0;
            Instantiate(iceBreak, transform.position, new Quaternion(0, 0, 0, 1));
        }

        if(health > 0) { // Win
            GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().volume = 0.3f;
            GetComponent<AudioSource>().clip = gameWin;
            GetComponent<AudioSource>().Play();

            hasWon = true;
            confetti.SetActive(true);
            playerSpriteAnimator.SetBool("Walking", true);
            GameObject.Find("EndGameScreen").transform.GetChild(0).gameObject.SetActive(true);

            if(inputMultiplayer != null) { // Multiplayer
                //Kill Enemy
                GameObject.Find("Enemy").GetComponent<EnemyMultiplayer>().Die();

                GameObject.Find("EndGameScreen").transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = enemyName + " DEFEATED!";

                if(PhotonNetwork.CurrentRoom.IsVisible) { // ranked
                    GameObject.Find("EndGameScreen").transform.GetChild(7).gameObject.SetActive(true);
                }
                else { // custom
                    GameObject.Find("EndGameScreen").GetComponent<Animator>().SetTrigger("TwoButtons");
                }
            }
            else { // Offline Win
                if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() == 15) {
                    GameObject.Find("EndGameScreen").transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "GAME COMPLETED!";
                    GameObject.Find("EndGameScreen").GetComponent<Animator>().SetTrigger("TwoButtons");
                }
                else {
                    GameObject.Find("EndGameScreen").transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "LEVEL " + GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() + " COMPLETED!";
                    GameObject.Find("EndGameScreen").transform.GetChild(7).gameObject.SetActive(true);
                }
                GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().AddLevel(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel());
            }
        }
        else { // Lose
            GameObject.Find("EndGameScreen").transform.GetChild(1).gameObject.SetActive(true);
            if(inputMultiplayer != null) { // Multiplayer
                GameObject.Find("Enemy").transform.GetChild(1).gameObject.SetActive(false);
                if(PhotonNetwork.CurrentRoom.IsVisible) { // ranked
                    GameObject.Find("EndGameScreen").transform.GetChild(7).gameObject.SetActive(true);
                }
                else { // custom
                    GameObject.Find("EndGameScreen").GetComponent<Animator>().SetTrigger("TwoButtons");
                }
            }
            else { // Offline Lose
                GameObject.Find("EndGameScreen").GetComponent<Animator>().SetTrigger("TwoButtons");
            }
        }
        if(Mathf.Floor(timeCompleted % 60) < 10) {
            GameObject.Find("TimeCompleted").GetComponent<TMP_Text>().text = "TIME\n" + Mathf.Floor(timeCompleted / 60) + ":0" + Mathf.Floor(timeCompleted % 60);
        }
        else {
            GameObject.Find("TimeCompleted").GetComponent<TMP_Text>().text = "TIME\n" + Mathf.Floor(timeCompleted / 60) + ":" + Mathf.Floor(timeCompleted % 60);
        }
        if(inputMultiplayer != null) {
            GameObject.Find("LettersUsed").GetComponent<TMP_Text>().text = "LETTERS USED\n" + lettersUsed;
        }
        else {
            GameObject.Find("GoldGained").GetComponent<TMP_Text>().text = "GOLD\n" + goldGained;
            GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().SaveGold(goldGained);
        }
        GameObject.Find("WordsSpelled").GetComponent<TMP_Text>().text = "WORDS FOUND\n" + wordsSpelled;
        GameObject.Find("LongestWord").GetComponent<TMP_Text>().text = "LONGEST WORD\n" + longestWord;
        GameObject.Find("HighestCombo").GetComponent<TMP_Text>().text = "HIGHEST COMBO\n" + highestCombo;

        if(inputMultiplayer != null) {
            if(PhotonNetwork.CurrentRoom.IsVisible) {
                if(health > 0)
                    GameObject.Find("GameUI").GetComponent<EndGameManager>().UploadPlayerData(1, 0, lettersUsed, wordsSpelled, longestWord, GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostume(), (int)Mathf.Floor(timeCompleted));
                else
                    GameObject.Find("GameUI").GetComponent<EndGameManager>().UploadPlayerData(0, 1, lettersUsed, wordsSpelled, longestWord, GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostume(), (int)Mathf.Floor(timeCompleted));
            }
        }

        if(health <= 0) {
            Instantiate(gameLose, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);
            gameObject.SetActive(false);
        }

        if(hasLeft) {
            rematchButton.interactable = false;
            rematchText.SetActive(true);
            rematchText.GetComponent<TMP_Text>().text = "PLAYER HAS LEFT";
        }
    }

    public void AddGold(int num) {
        goldGained += num;
    }

    public void SetLettersUsed(int num) {
        lettersUsed = num;
    }

    public void SetWordsSpelled(int num) {
        wordsSpelled = num;
    }

    public void SetLongestWord(string wrd) {
        if(wrd != "")
            longestWord = wrd;
        else
            longestWord = "*NONE*";
    }

    public void SetHighestCombo(int num) {
        highestCombo = num;
    }

    public float GetHealth() {
        return health;
    }

    public void SetEnemyName(string str) {
        enemyName = str;
    }

    void OnTriggerEnter2D(Collider2D other) {
        SpellCode spellCode = other.gameObject.GetComponent<SpellCode>();
        if(spellCode != null) {
            if(spellCode.GetMultiplayerSpell()) {
                Destroy(other.gameObject);
            }
        }
    }

    public void HasLeft() {
        hasLeft = true;
    }
}
