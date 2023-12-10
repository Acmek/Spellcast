using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class SpellMenuCode : MonoBehaviour
{
    [SerializeField] private PlayerSave save;
    [SerializeField] private string spellName;
    [SerializeField] private int[] prices;
    [SerializeField] private TMP_Text text;
    [SerializeField] private string[] descriptions;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image[] images;
    [SerializeField] private GameObject buyButton;

    // Update is called once per frame
    void Update() {
        if(save.GetSpellLevel(spellName) != 5)
            text.text = "" + prices[save.GetSpellLevel(spellName)];
        else
            buyButton.SetActive(false);

        for(int i = 0; i < save.GetSpellLevel(spellName); i++) {
            images[i].color = new Color(1, 0, 0, 1);
            descriptionText.text = descriptions[save.GetSpellLevel(spellName) - 1];
        }
    }

    public void UpgradeSpell() {
        if(save.GetSpellLevel(spellName) < 5) {
            if(save.GetGold() >= prices[save.GetSpellLevel(spellName)]) {
                save.SaveGold(-prices[save.GetSpellLevel(spellName)]);
                save.AddSpellLevel(spellName);

                GetComponent<AudioSource>().Play();
            }
        }
    }
}
