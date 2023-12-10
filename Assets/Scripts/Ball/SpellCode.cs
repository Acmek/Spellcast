using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpellCode : MonoBehaviour
{
    [SerializeField] private float burnDuration;
    [SerializeField] private float freezeDuration;
    [SerializeField] private float shockChance;
    [SerializeField] private float healingFactor;
    private float damage;
    private bool multiplayerSpell;
    private GameObject playerObj;
    private GameObject enemyObj;

    void Start() {
        playerObj = GameObject.Find("Player");
        enemyObj = GameObject.FindWithTag("Enemy");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(multiplayerSpell)
            transform.position = new Vector3(transform.position.x - 0.05f, transform.position.y, transform.position.z);
        else
            transform.position = new Vector3(transform.position.x + 0.05f, transform.position.y, transform.position.z);
    }

    public bool GetMultiplayerSpell() {
        return multiplayerSpell;
    }

    public void SetMultiplayerSpell() {
        multiplayerSpell = true;

        if(gameObject.tag == "Normal") {
            GetComponent<SpriteRenderer>().color = new Color(1, 0, 1, 1);

            ParticleSystem ps = transform.GetChild(0).GetComponent<ParticleSystem>();
            ps.startColor = (Resources.Load("Particles/MultiplayerParticle") as GameObject).GetComponent<ParticleSystem>().startColor;
            var col = ps.colorOverLifetime;
            col.color = (Resources.Load("Particles/MultiplayerParticle") as GameObject).GetComponent<ParticleSystem>().colorOverLifetime.color;
        }
    }

    void Update() {
        if(!multiplayerSpell)
            enemyObj = GameObject.FindWithTag("Enemy");

        if(enemyObj != null)
            if(enemyObj.transform.position.x < transform.position.x)
                transform.position = new Vector3(enemyObj.transform.position.x, transform.position.y, transform.position.z);
        if(playerObj.GetComponent<PlayerCode>().GetHealth() > 0 && playerObj.transform.position.x > transform.position.x)
                transform.position = new Vector3(playerObj.transform.position.x, transform.position.y, transform.position.z);

        if(transform.position.x > 1.4f || transform.position.x < -10.92f)
            Destroy(gameObject);
    }

    public void SetDamage(float num) {
        damage = num;
    }

    public float GetDamage() {
        GameObject inputObject = GameObject.Find("Input");

        if(inputObject != null) {
            if(PhotonNetwork.InRoom)
                return inputObject.GetComponent<InputMultiplayer>().GetAttackLevel() * damage;
            else
                return inputObject.GetComponent<InputCode>().GetAttackLevel() * damage;
        }
        return 0;
    }

    public float GetBurnDuration() {
        if(burnDuration > 0 && GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("fireball") > 0)
            return burnDuration + (2.5f * (GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("fireball") - 1));
        return burnDuration;
    }

    public float GetFreezeDuration() {
        if(freezeDuration > 0 && GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("snowball") > 0)
            return freezeDuration + (1.25f * (GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("snowball") - 1));
        return freezeDuration;
    }

    public float GetHealingFactor() {
        if(healingFactor > 0 && GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("rose") > 0)
            return healingFactor + (0.1025f * (GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("rose") - 1));
        return healingFactor;
    }

    public float GetShockChance() {
        if(shockChance > 0 && GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("lightning") > 0)
            return shockChance + (0.1875f * (GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("lightning") - 1));
        return shockChance;
    }

    public void SetBurnDuration(float num) {
        burnDuration = num;
    }

    public void SetFreezeDuration(float num) {
        freezeDuration = num;    
    }

    public void SetShockChance(float num) {
        shockChance = num;
    }

    public void SetHealingFactor(float num) {
        healingFactor = num;
    }
}
