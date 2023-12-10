using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class PlayerStatsCode : MonoBehaviour
{
    [SerializeField] private PlayerSave save;
    [SerializeField] private int[] attackCosts;
    [SerializeField] private int[] healthCosts;

    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text healthText;

    [SerializeField] private Image[] attackImage;
    [SerializeField] private Image[] healthImage;

    [SerializeField] private GameObject attackBuy;
    [SerializeField] private GameObject healthBuy;

    void Update() {
        if(save.GetDamageLevel() != 5)
            attackText.text = "" + attackCosts[save.GetDamageLevel()];
        else
            attackBuy.SetActive(false);
        if(save.GetHealthLevel() != 5)
            healthText.text = "" + healthCosts[save.GetHealthLevel()];
        else
            healthBuy.SetActive(false);

        for(int i = 0; i < save.GetDamageLevel(); i++) {
            attackImage[i].color = new Color(1, 0, 0, 1);
        }
        for(int i = 0; i < save.GetHealthLevel(); i++) {
            healthImage[i].color = new Color(1, 0, 0, 1);
        }
    }

    public void UpgradeAttack() {
        if(save.GetDamageLevel() < 5) {
            if(save.GetGold() >= attackCosts[save.GetDamageLevel()]) {
                save.SaveGold(-attackCosts[save.GetDamageLevel()]);
                save.AddDamageLevel();

                GetComponent<AudioSource>().Play();
            }
        }
    }

    public void UpgradeHealth() {
        if(save.GetHealthLevel() < 5) {
            if(save.GetGold() >= healthCosts[save.GetHealthLevel()]) {
                save.SaveGold(-healthCosts[save.GetHealthLevel()]);
                save.AddHealthLevel();

                GetComponent<AudioSource>().Play();
            }
        }
    }
}
