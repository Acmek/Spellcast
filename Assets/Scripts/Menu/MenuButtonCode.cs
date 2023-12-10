using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonCode : MonoBehaviour
{
    [SerializeField] private GameObject[] items;
    [SerializeField] private GameObject scrollLeftButton;
    [SerializeField] private GameObject scrollRightButton;
    [SerializeField] private bool isCostumeMenu;
    private int itemIndex;
    private bool canScroll = true;

    void Start() {
        if(isCostumeMenu) {
            itemIndex = GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeIndex();

            if(itemIndex == items.Length - 1) {
                scrollRightButton.SetActive(false);
                scrollLeftButton.SetActive(true);
            }
            else if(itemIndex == 0) {
                scrollRightButton.SetActive(true);
                scrollLeftButton.SetActive(false);
            }
            else {
                scrollRightButton.SetActive(true);
                scrollLeftButton.SetActive(true);
            }

            for(int i = 0; i < items.Length; i++)
                items[i].SetActive(false);
            items[itemIndex].SetActive(true);
        }
    }

    public void ScrollLeft() {
        if(canScroll) {
            canScroll = false;
            if(itemIndex == items.Length - 1) {
                scrollRightButton.SetActive(true);
            }
            if(itemIndex == 1) {
                scrollLeftButton.SetActive(false);
            }
            itemIndex--;
            items[itemIndex + 1].GetComponent<Animator>().SetBool("MenuShrinkScrollLeft", true);
            StartCoroutine(CloseScrollLeft());
            items[itemIndex].SetActive(true);
            items[itemIndex].GetComponent<Animator>().SetTrigger("MenuEnlargeScrollLeft");

            GetComponent<AudioSource>().time = 0.2f;
            GetComponent<AudioSource>().Play();
        }
    }

    IEnumerator CloseScrollLeft() {
        yield return new WaitForSeconds(0.25f);

        items[itemIndex + 1].GetComponent<Animator>().SetBool("MenuShrinkScrollLeft", false);
        items[itemIndex + 1].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 112);
        items[itemIndex + 1].GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        items[itemIndex + 1].SetActive(false);
        canScroll = true;
    }

    public void ScrollRight() {
        if(canScroll) {
            canScroll = false;
            if(itemIndex == 0) {
                scrollLeftButton.SetActive(true);
            }
            if(itemIndex == items.Length - 2) {
                scrollRightButton.SetActive(false);
            }
            itemIndex++;
            items[itemIndex - 1].GetComponent<Animator>().SetBool("MenuShrinkScrollRight", true);
            StartCoroutine(CloseScrollRight());
            items[itemIndex].SetActive(true);
            items[itemIndex].GetComponent<Animator>().SetTrigger("MenuEnlargeScrollRight");

            GetComponent<AudioSource>().time = 0.2f;
            GetComponent<AudioSource>().Play();
        }
    }

    IEnumerator CloseScrollRight() {
        yield return new WaitForSeconds(0.25f);

        items[itemIndex - 1].GetComponent<Animator>().SetBool("MenuShrinkScrollRight", false);
        items[itemIndex - 1].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 112);
        items[itemIndex - 1].GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        items[itemIndex - 1].SetActive(false);
        canScroll = true;
    }
}
