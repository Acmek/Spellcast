using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCode : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float burnTime;
    [SerializeField] private float freezeTime;
    [SerializeField] private float shockChance;
    [SerializeField] private float healingFactor;
    [SerializeField] private EnemyCode enemy;

    public void Attack() {
        bool shocked = false;
        if(shockChance > 0) {
            if(Random.Range(0f, 1f) <= shockChance) {
                shocked = true;
            }
        }
        if(GameObject.Find("Player") != null)
            GameObject.Find("Player").GetComponent<PlayerCode>().Damage(damage, burnTime, freezeTime, shocked, healingFactor);
    }

    public void AfterAttack() {
        enemy.ResetAttack();
    }
}
