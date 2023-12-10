using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave : MonoBehaviour
{
    public string[] costumeNameArray;
    public Sprite[] costumeMenuArray;
    public Sprite[] costumePlayerArray;
    public int gold;

    public int healthLevel;
    public int damageLevel;

    public int fireballLevel;
    public int snowballLevel;
    public int roseLevel;
    public int lightningLevel;
    public int bombLevel;

    public string costumes;
    public string costumeEquipped;

    public string levelsCompleted;
    public int lastLevelOpen;

    void Awake() {
        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.SetInt("gold", 100000);
        //PlayerPrefs.SetString("levelsCompleted", "0 1 2 3 4 5 6 7 8 9 10 11 12 13 14");
        gold = PlayerPrefs.GetInt("gold");

        healthLevel = PlayerPrefs.GetInt("healthLevel");
        damageLevel = PlayerPrefs.GetInt("damageLevel");

        costumes = PlayerPrefs.GetString("costumes");
        if(System.Array.IndexOf(costumes.Split(" "), "classic") == -1)
            costumes += "classic ";
        costumeEquipped = PlayerPrefs.GetString("costumeEquipped");
        if(costumeEquipped == "")
            EquipCostume("classic");

        levelsCompleted = PlayerPrefs.GetString("levelsCompleted");
        if(System.Array.IndexOf(levelsCompleted.Split(" "), "0") == -1)
            levelsCompleted += "0 ";
        lastLevelOpen = PlayerPrefs.GetInt("lastLevelOpen");
    }

    public void SaveGold(int num) {
        PlayerPrefs.SetInt("gold", gold += num);
        PlayerPrefs.Save();
        gold = PlayerPrefs.GetInt("gold");
    }

    public int GetGold() {
        return gold;
    }

    public void AddDamageLevel() {
        PlayerPrefs.SetInt("damageLevel", damageLevel += 1);
        PlayerPrefs.Save();
        damageLevel = PlayerPrefs.GetInt("damageLevel");
    }

    public int GetDamageLevel() {
        return damageLevel;
    }

    public void AddHealthLevel() {
        PlayerPrefs.SetInt("healthLevel", healthLevel += 1);
        PlayerPrefs.Save();
        healthLevel = PlayerPrefs.GetInt("healthLevel");
    }

    public int GetHealthLevel() {
        return healthLevel;
    }

    public void AddSpellLevel(string str) {
        PlayerPrefs.SetInt(str + "Level", PlayerPrefs.GetInt(str + "Level") + 1);
        PlayerPrefs.Save();
    }

    public int GetSpellLevel(string str) {
        return PlayerPrefs.GetInt(str + "Level");
    }

    public void SaveCostume(string str) {
        PlayerPrefs.SetString("costumes", costumes += str + " ");
        PlayerPrefs.Save();
        costumes = PlayerPrefs.GetString("costumes");
    }

    public string[] GetCostumes() {
        return costumes.Split(" ");
    }

    public void EquipCostume(string str) {
        PlayerPrefs.SetString("costumeEquipped", str);
        PlayerPrefs.Save();
        costumeEquipped = PlayerPrefs.GetString("costumeEquipped");
    }

    public string GetCostume() {
        return costumeEquipped;
    }

    public Sprite GetCostumeMenu() {
        return costumeMenuArray[System.Array.IndexOf(costumeNameArray, costumeEquipped)];
    }

    public Sprite GetCostumePlayer() {
        return costumePlayerArray[System.Array.IndexOf(costumeNameArray, costumeEquipped)];
    }

    public int GetCostumeIndex() {
        return System.Array.IndexOf(costumeNameArray, costumeEquipped);
    }

    public Sprite[] GetCostumeArr() {
        return costumePlayerArray;
    }

    public void SetLastLevel(int num) {
        PlayerPrefs.SetInt("lastLevelOpen", num);
        PlayerPrefs.Save();
        lastLevelOpen = PlayerPrefs.GetInt("lastLevelOpen");
    }

    public int GetLastLevel() {
        return lastLevelOpen;
    }

    public void AddLevel(int num) {
        if(System.Array.IndexOf(GetLevels(), num + "") == -1) {
            PlayerPrefs.SetString("levelsCompleted", levelsCompleted += num + " ");
            PlayerPrefs.Save();
            levelsCompleted = PlayerPrefs.GetString("levelsCompleted");
        }
    }

    public string[] GetLevels() {
        return levelsCompleted.Split(" ");
    }
}
