using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectCode : MonoBehaviour
{
    [SerializeField] private int levels;
    [SerializeField] private GameObject scrollUpButton;
    [SerializeField] private GameObject scrollDownButton;
    [SerializeField] private GameObject mainButton;
    [SerializeField] private GameObject aboveLevel;
    [SerializeField] private GameObject aboveAboveLevel;
    [SerializeField] private GameObject belowLevel;
    [SerializeField] private GameObject belowBelowLevel;
    [SerializeField] private PlayerSave playerSaves;
    [SerializeField] private Animator transition;
    [SerializeField] private PlayerSave save;
    [SerializeField] private AudioClip transitionSound;
    [SerializeField] private Sprite[] backgroundSprites;
    [SerializeField] private SpriteRenderer[] backgrounds;
    private int levelIndex = 1;
    private bool canPress = true;
    private bool canStart;

    void Start() {
        levelIndex = playerSaves.GetLastLevel();

        if(levelIndex == 0)
            levelIndex = 1;

        if(levelIndex == levels) {
            scrollUpButton.SetActive(false);
            aboveLevel.SetActive(false);
        }
        else if(levelIndex == 1) {
            scrollDownButton.SetActive(false);
            belowLevel.SetActive(false);
        }

        mainButton.GetComponent<TMP_Text>().text = "Level " + levelIndex;
        aboveLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex + 1);
        belowLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex - 1);
        aboveAboveLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex + 2);
        belowBelowLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex - 2);

        int i = -1;
        if(levelIndex <= 5)
            i = 0;
        else if(levelIndex >=6 && levelIndex <= 10)
            i = 1;
        else if(levelIndex >= 11 && levelIndex <= 15)
            i = 2;
            
        foreach(SpriteRenderer background in backgrounds)
            background.sprite = backgroundSprites[i];
    }

    void Update() {
        UpdateLevels();
    }

    public void ScrollUp() {
        if(canPress) {
            canPress = false;
            
            if(levelIndex == 1) {
                scrollDownButton.SetActive(true);
                belowLevel.SetActive(true);
            }
            else {
                belowBelowLevel.GetComponent<Animator>().SetTrigger("BelowBelowLevelScrollUp");
            }
            if(levelIndex == levels - 1) {
                scrollUpButton.SetActive(false);
                aboveLevel.SetActive(false);
            }
            levelIndex++;

            mainButton.GetComponent<Animator>().SetTrigger("MainButtonScrollUp");
            aboveLevel.GetComponent<Animator>().SetTrigger("AboveLevelScrollUp");
            belowLevel.GetComponent<Animator>().SetTrigger("BelowLevelScrollUp");

            StartCoroutine(ResetCanPress());
        }
    }

    public void ScrollDown() {
        if(canPress) {
            canPress = false;

            if(levelIndex == levels) {
                scrollUpButton.SetActive(true);
                aboveLevel.SetActive(true);
            }
            else {
                aboveAboveLevel.GetComponent<Animator>().SetTrigger("AboveAboveLevelScrollDown");
            }
            if(levelIndex == 2) {
                scrollDownButton.SetActive(false);
                belowLevel.SetActive(false);
            }
            levelIndex--;

            mainButton.GetComponent<Animator>().SetTrigger("MainButtonScrollDown");
            aboveLevel.GetComponent<Animator>().SetTrigger("AboveLevelScrollDown");
            belowLevel.GetComponent<Animator>().SetTrigger("BelowLevelScrollDown");

            StartCoroutine(ResetCanPress());
        }
    }

    void UpdateLevels() {
        mainButton.GetComponent<TMP_Text>().text = "Level " + levelIndex;
        if(System.Array.IndexOf(save.GetLevels(), "" + (levelIndex - 1)) != -1) {
            transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
            canStart = true;
        }
        else {
            transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
            canStart = false;
        }

        aboveLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex + 1);
        if(System.Array.IndexOf(save.GetLevels(), "" + (levelIndex)) != -1 && levelIndex < 30)
            aboveLevel.transform.GetChild(0).gameObject.SetActive(false);
        else
            aboveLevel.transform.GetChild(0).gameObject.SetActive(true);

        belowLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex - 1);
        if(System.Array.IndexOf(save.GetLevels(), "" + (levelIndex - 2)) != -1 && levelIndex - 2 > -1)
            belowLevel.transform.GetChild(0).gameObject.SetActive(false);
        else
            belowLevel.transform.GetChild(0).gameObject.SetActive(true);

        aboveAboveLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex + 2);
        if(System.Array.IndexOf(save.GetLevels(), "" + (levelIndex + 1)) != -1 && levelIndex + 1 < 30)
            aboveAboveLevel.transform.GetChild(0).gameObject.SetActive(false);
        else
            aboveAboveLevel.transform.GetChild(0).gameObject.SetActive(true);

        belowBelowLevel.GetComponent<TMP_Text>().text = "Level " + (levelIndex - 2);
        if(System.Array.IndexOf(save.GetLevels(), "" + (levelIndex - 3)) != -1 && levelIndex - 3 > -1)
            belowBelowLevel.transform.GetChild(0).gameObject.SetActive(false);
        else
            belowBelowLevel.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void SelectLevel() {
        if(canStart)
            StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel() {
        transition.SetTrigger("Close");
        GetComponent<AudioSource>().clip = transitionSound;
        GetComponent<AudioSource>().volume = 1;
        GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(0.75f);

        playerSaves.SetLastLevel(levelIndex);
        SceneManager.LoadScene("Levels");
    }

    IEnumerator ResetCanPress() {
        int i = -1;
        if(levelIndex <= 5)
            i = 0;
        else if(levelIndex >=6 && levelIndex <= 10)
            i = 1;
        else if(levelIndex >= 11 && levelIndex <= 15)
            i = 2;
            
        foreach(SpriteRenderer background in backgrounds)
            background.sprite = backgroundSprites[i];
            
        GetComponent<AudioSource>().time = 0.2f;
        GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(0.25f);

        canPress = true;
    }
}
