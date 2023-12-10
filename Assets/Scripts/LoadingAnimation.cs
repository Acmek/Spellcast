using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] private bool searchingEnemy;
    [SerializeField] private string str;
    private Image img;
    private TMP_Text text;

    void OnEnable() {
        if(searchingEnemy) {
            img = GetComponent<Image>();
            StartCoroutine(SearchingAnim());
        }
        else {
            text = GetComponent<TMP_Text>();
            StartCoroutine(LoadingAnim());
        }
    }

    IEnumerator LoadingAnim() {
        for(int i = 0; i < 4; i++) {
            string dots = "";
            for(int j = 0; j < i; j++) {
                dots += ".";
            }
            text.text = str + dots;
            
            yield return new WaitForSeconds(0.5f);
        }

        StartCoroutine(LoadingAnim());
    }

    IEnumerator SearchingAnim() {
        foreach(Sprite costume in GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeArr()) {
            img.sprite = costume;

            yield return new WaitForSeconds(0.15f);
        }

        StartCoroutine(SearchingAnim());
    }

    public void StopAnim() {
        StopAllCoroutines();
        this.enabled = false;
    }
}
