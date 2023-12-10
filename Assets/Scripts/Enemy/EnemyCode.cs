using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class EnemyCode : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject healthBarGameObject;

    [Header("Attack (Damage In EnemySprite)")]
    [SerializeField] private float attackTime;

    [Header("Gold")]
    [SerializeField] private float minGoldAmount;
    [SerializeField] private float maxGoldAmount;

    [Header("Animation")]
    [SerializeField] private Animator enemySpriteAnimator;
    [SerializeField] private GameObject enemySprite;

    [Header("Effects")]
    [SerializeField] private GameObject particle;
    [SerializeField] private GameObject shadow;
    [SerializeField] private GameObject gold;
    [SerializeField] private ParticleSystem fire;
    [SerializeField] private GameObject ice;
    [SerializeField] private GameObject iceBreak;
    [SerializeField] private GameObject lightningBolt;

    [Header("Sound")]
    [SerializeField] private GameObject spellHit;
    [SerializeField] private GameObject splat;

    private Transform[] floorBackgrounds;
    private InputCode input;
    private ComboCode combo;
    private TimerCode timer;
    private PlayerCode player;
    private float health;
    private bool inPosition;
    private bool dead;
    private float burnTimer;
    private float freezeTimer;
    private bool playerAttacked;

    // Start is called before the first frame update
    void Start()
    {
        input = GameObject.Find("Input").GetComponent<InputCode>();
        combo = GameObject.Find("Combo").GetComponent<ComboCode>();
        health = maxHealth;
        GameObject.Find("PlayerSprite").GetComponent<Animator>().SetBool("Walking", true);
        timer = GameObject.Find("Timer").GetComponent<TimerCode>();
        player = GameObject.Find("Player").GetComponent<PlayerCode>();

        floorBackgrounds = new Transform[GameObject.Find("FloorBackgroundHolder").transform.childCount];
        for(int i = 0; i < floorBackgrounds.Length; i++)
            floorBackgrounds[i] = GameObject.Find("FloorBackgroundHolder").transform.GetChild(i).GetComponent<Transform>();
    }

    void FixedUpdate() {
        if(transform.position.x > -2.3f && health > 0) {
            if(!playerAttacked)
                combo.DelayCombo();
            if(health < maxHealth)
                playerAttacked = true;
            transform.position = new Vector3(transform.position.x - 0.025f, transform.position.y, transform.position.z);
            foreach(Transform floorBackground in floorBackgrounds)
                floorBackground.position = new Vector3(floorBackground.position.x - 0.025f, floorBackground.position.y, floorBackground.position.z);
            GameObject pastGold = GameObject.FindWithTag("Gold");
            if(pastGold != null)
                pastGold.transform.position = new Vector3(pastGold.transform.position.x - 0.025f, pastGold.transform.position.y, pastGold.transform.position.z);

            if(transform.position.x < -2.3f) {
                transform.position = new Vector3(-2.3f, transform.position.y, transform.position.z);
                GameObject.Find("PlayerSprite").GetComponent<Animator>().SetBool("Walking", false);
                if(health > 0) {
                    StartCoroutine(timer.ResetTimer(attackTime));
                    StartCoroutine(EnableAttacking());
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = health / maxHealth;

        if(inPosition && timer.CanAttack() && health > 0) {
            enemySpriteAnimator.SetTrigger("Attack");
            inPosition = false;
        }

        if(burnTimer > 0) {
            burnTimer -= Time.deltaTime;

            if(Random.Range(0, 3) == 0)
                health -= input.GetAttackLevel() / 100f;
            
            if(burnTimer <= 0)
                fire.Stop();
        }

        if(freezeTimer > 0) {
            freezeTimer -= Time.deltaTime;
            
            if(freezeTimer <= 0) {
                timer.Unfreeze();
                Instantiate(iceBreak, new Vector3(transform.position.x, transform.position.y, -5), new Quaternion(0, 0, 0, 1));
                ice.SetActive(false);
            }
        }

        if(health <= 0 && !dead) {
            dead = true;
            StartCoroutine(Die());
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(health > 0) {
            if(other.gameObject.tag == "Normal") {
                health -= other.GetComponent<SpellCode>().GetDamage();
            }
            if(other.gameObject.tag == "Fire") {
                health -= other.GetComponent<SpellCode>().GetDamage();
                burnTimer = other.GetComponent<SpellCode>().GetBurnDuration();
                fire.Play();
            }
            if(other.gameObject.tag == "Ice") {
                health -= other.GetComponent<SpellCode>().GetDamage();
                freezeTimer = other.GetComponent<SpellCode>().GetFreezeDuration();
                timer.Freeze();
                ice.SetActive(true);
                ice.GetComponent<Animator>().SetTrigger("Freeze");
            }
            if(other.gameObject.tag == "Lightning") {
                health -= other.GetComponent<SpellCode>().GetDamage() * 2;
                if(Random.Range(0f, 1f) <= other.GetComponent<SpellCode>().GetShockChance()) {
                    timer.Shocked(attackTime);
                    GameObject bolt = Instantiate(lightningBolt, new Vector3(transform.position.x, 0.85f, -6), new Quaternion(0, 0, 0, 1));
                    bolt.GetComponent<Animator>().SetTrigger("Strike");
                    Destroy(bolt, 0.35f);
                }
            }
            if(other.gameObject.tag == "Plant") {
                float damage = other.GetComponent<SpellCode>().GetDamage();
                if(damage > health)
                    player.Heal(health / maxHealth * other.GetComponent<SpellCode>().GetHealingFactor());
                else
                    player.Heal(damage / maxHealth * other.GetComponent<SpellCode>().GetHealingFactor());
                health -= damage;
            }
            if(other.gameObject.tag == "Bomb") {
                float damage = other.GetComponent<SpellCode>().GetDamage();

                if(other.GetComponent<SpellCode>().GetBurnDuration() > 0) {
                    burnTimer = other.GetComponent<SpellCode>().GetBurnDuration();
                    fire.Play();
                }
                if(other.GetComponent<SpellCode>().GetFreezeDuration() > 0) {
                    freezeTimer = other.GetComponent<SpellCode>().GetFreezeDuration();
                    timer.Freeze();
                    ice.SetActive(true);
                    ice.GetComponent<Animator>().SetTrigger("Freeze");
                }
                if(other.GetComponent<SpellCode>().GetShockChance() > 0) {
                    damage = other.GetComponent<SpellCode>().GetDamage() * 2;

                    if(Random.Range(0f, 1f) <= other.GetComponent<SpellCode>().GetShockChance()) {
                        timer.Shocked(attackTime);
                        GameObject bolt = Instantiate(lightningBolt, new Vector3(transform.position.x, 0.85f, -6), new Quaternion(0, 0, 0, 1));
                        bolt.GetComponent<Animator>().SetTrigger("Strike");
                        Destroy(bolt, 0.35f);
                    }
                }
                if(other.GetComponent<SpellCode>().GetHealingFactor() > 0) {
                    if(damage > health)
                        player.Heal(health / maxHealth * other.GetComponent<SpellCode>().GetHealingFactor());
                    else
                        player.Heal(damage / maxHealth * other.GetComponent<SpellCode>().GetHealingFactor());
                }
                health -= damage;
            }
        }

        if(other.GetComponent<SpellCode>() != null) {
            enemySpriteAnimator.SetTrigger("Damaged");
            Instantiate(spellHit, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);
            Destroy(other.gameObject);
        }
    }

    IEnumerator EnableAttacking() {
        inPosition = false;
        yield return new WaitForSeconds(0.5f);
        inPosition = true;
    }

    public void ResetAttack() {
        StartCoroutine(timer.ResetTimer(attackTime));
        StartCoroutine(EnableAttacking());
    }

    IEnumerator Die() {
        GameObject playerSpr = GameObject.Find("PlayerSprite");
        if(playerSpr != null)
            playerSpr.GetComponent<Animator>().SetBool("Walking", false);
        healthBarGameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        timer.StopTimer();
        enemySprite.SetActive(false);
        shadow.SetActive(false);
        fire.Stop();
        ice.SetActive(false);
        if(freezeTimer > 0)
            Instantiate(iceBreak, transform.position, new Quaternion(0, 0, 0, 1));
        particle.SetActive(true);
        Instantiate(splat, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);

        yield return new WaitForSeconds(1.5f);

        if(GameObject.Find("Player") != null) {
            timer.Unfreeze();
            int goldAmount = (int)Random.Range(minGoldAmount, maxGoldAmount);
            player.AddGold(goldAmount);
            Instantiate(gold, new Vector3(-2.3f, -3.83f, 0), Quaternion.Euler(270, 0, 0)).GetComponent<GoldCode>().SetGold(goldAmount);
            Destroy(gameObject);

            input.NextEnemy();
        }
    }

    public void PlayerAttacked() {
        playerAttacked = true;
    }
}
