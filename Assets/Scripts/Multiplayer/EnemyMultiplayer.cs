using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;

public class EnemyMultiplayer : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Animator playerAnim;
    [SerializeField] private GameObject ready;
    [SerializeField] private Transform floorBackgroundHolder;
    [SerializeField] private InputMultiplayer inputMultiplayer;
    [SerializeField] private ParticleSystem fire;
    [SerializeField] private GameObject ice;
    [SerializeField] private GameObject iceBreak;
    [SerializeField] private GameObject lightningBolt;
    [SerializeField] private GameObject spellHit;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject burst;
    [SerializeField] private GameObject splat;

    // Start is called before the first frame update
    void Start()
    {
        playerAnim.SetBool("Walking", true);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(transform.position.x > -2.6f) {
            transform.position = new Vector3(transform.position.x - 0.025f, transform.position.y, transform.position.z);
            floorBackgroundHolder.position = new Vector3(floorBackgroundHolder.position.x - 0.025f, floorBackgroundHolder.position.y, floorBackgroundHolder.position.z);

            if(transform.position.x < -2.6f) {
                if(ready.activeSelf) {
                    transform.position = new Vector3(-2.6f, transform.position.y, transform.position.z);
                    playerAnim.SetBool("Walking", false);
                    ready.GetComponent<TMP_Text>().text = "Go!";
                    ready.GetComponent<Animator>().SetTrigger("Go");
                    ready.GetComponent<AudioSource>().Play();

                    if(PhotonNetwork.IsMasterClient)
                        inputMultiplayer.EnableGame();
                }
            }
        }
    }

    public void UpdateHealthBar(float num) {
        healthBar.fillAmount = num;
    }

    public void StopBurning() {
        fire.Stop();
    }

    public void Unfreeze() {
        Instantiate(iceBreak, new Vector3(-2.24f, -3.23f, -5), new Quaternion(0, 0, 0, 1));
        ice.SetActive(false);
    }

    public void Die() {
        Instantiate(burst, new Vector3(-2.24f, -2.6f, 0), new Quaternion(0, 0, 0, 1));
        fire.Stop();
        if(ice.activeSelf)
            Instantiate(iceBreak, transform.position, new Quaternion(0, 0, 0, 1));
        ice.SetActive(false);
        Instantiate(splat, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other) {
        SpellCode spellCode = other.gameObject.GetComponent<SpellCode>();

        if(spellCode != null) {
            if(!spellCode.GetMultiplayerSpell()) {
                float damage = 0;
                float burnTime = 0;
                float freezeTime = 0;
                bool shocked = false;
                float healingFactor = 0;

                if(other.gameObject.tag == "Normal") {
                    damage = spellCode.GetDamage();
                }
                if(other.gameObject.tag == "Fire") {
                    damage = spellCode.GetDamage();
                    burnTime = spellCode.GetBurnDuration();
                    fire.Play();
                }
                if(other.gameObject.tag == "Ice") {
                    damage = spellCode.GetDamage();
                    freezeTime = spellCode.GetFreezeDuration();
                    ice.SetActive(true);
                    ice.GetComponent<Animator>().SetTrigger("MultiplayerFreeze");
                }
                if(other.gameObject.tag == "Lightning") {
                    damage = spellCode.GetDamage() * 2;
                    if(Random.Range(0f, 1f) <= spellCode.GetShockChance()) {
                        shocked = true;
                        GameObject bolt = Instantiate(lightningBolt, new Vector3(-2.24f, 0.85f, -6), new Quaternion(0, 0, 0, 1));
                        bolt.GetComponent<Animator>().SetTrigger("Multiplayer");
                        Destroy(bolt, 0.35f);
                    }
                }
                if(other.gameObject.tag == "Plant") {
                    damage = spellCode.GetDamage();
                    healingFactor = spellCode.GetHealingFactor();
                }
                if(other.gameObject.tag == "Bomb") {
                    damage = spellCode.GetDamage();

                    if(spellCode.GetBurnDuration() > 0) {
                        burnTime = spellCode.GetBurnDuration();
                        fire.Play();
                    }
                    if(spellCode.GetFreezeDuration() > 0) {
                        freezeTime = spellCode.GetFreezeDuration();
                        ice.SetActive(true);
                        ice.GetComponent<Animator>().SetTrigger("MultiplayerFreeze");
                    }
                    if(spellCode.GetShockChance() > 0) {
                        damage = spellCode.GetDamage() * 2;

                        if(Random.Range(0f, 1f) <= spellCode.GetShockChance()) {
                            shocked = true;
                            GameObject bolt = Instantiate(lightningBolt, new Vector3(-2.24f, 0.85f, -6), new Quaternion(0, 0, 0, 1));
                            bolt.GetComponent<Animator>().SetTrigger("Multiplayer");
                            Destroy(bolt, 0.35f);
                        }
                    }
                    if(spellCode.GetHealingFactor() > 0) {
                        healingFactor = spellCode.GetHealingFactor();
                    }
                }

                inputMultiplayer.SpellHit(damage, burnTime, freezeTime, shocked, healingFactor);
                anim.SetTrigger("Damaged");
                Instantiate(spellHit, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);
                Destroy(other.gameObject);
            }
        }
    }
}
