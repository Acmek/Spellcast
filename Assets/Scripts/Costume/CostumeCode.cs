using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class CostumeCode : MonoBehaviour
{
    [SerializeField] private PlayerSave save;
    [SerializeField] private int price;
    [SerializeField] private string costumeIndex;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private GameObject equipButton;
    [SerializeField] private AudioSource menuAudio;
    [SerializeField] private AudioClip buySFX;
    [SerializeField] private AudioClip equipSFX;

    void Update() {
        if(System.Array.IndexOf(save.GetCostumes(), costumeIndex) == -1 && costumeIndex != "classic") {
            buyButton.SetActive(true);
            equipButton.SetActive(false);

            buyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "" + price;
        }
        else {
            buyButton.SetActive(false);
            equipButton.SetActive(true);

            if(save.GetCostume() != costumeIndex) {
                equipButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "EQUIP";
            }
            else {
                equipButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "EQUIPPED";
            }
        }
    }

    public void BuyCostume() {
        if(System.Array.IndexOf(save.GetCostumes(), costumeIndex) == -1) {
            if(save.GetGold() >= price) {
                save.SaveCostume(costumeIndex);
                save.SaveGold(-price);
                menuAudio.clip = buySFX;
                menuAudio.Play();
            }
        }
    }

    public void EquipCostume() {
        save.EquipCostume(costumeIndex);
        menuAudio.clip = equipSFX;
        menuAudio.Play();
    }
}
