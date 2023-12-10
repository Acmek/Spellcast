using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class PauseButtonCode : MonoBehaviour
{
    [SerializeField] private Sprite pauseButton;
    [SerializeField] private Sprite playButton;
    [SerializeField] private Image image;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject backDrop;
    [SerializeField] private InputCode input;
    [SerializeField] private TMP_Text text;
    private bool canClick = true;
    private bool isOpen;

    public void Clicked() {
        if(canClick) {
            canClick = false;

            if(!isOpen) {
                isOpen = true;
                image.sprite = playButton;
                input.Paused();
                StartCoroutine(OpenMenu());
            }
            else {
                isOpen = false;
                image.sprite = pauseButton;
                input.Unpaused();
                StartCoroutine(CloseMenu());
            }
            GetComponent<AudioSource>().Play();
        }
    }

    public void PauseEscape() {
        GetComponent<AudioSource>().Play();
        
        isOpen = true;
        image.sprite = playButton;
        input.Paused();
        StartCoroutine(OpenMenu());
    }

    IEnumerator OpenMenu() {
        text.text = "LEVEL " + GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() + " PAUSED";
        backDrop.SetActive(true);
        backDrop.GetComponent<Animator>().SetTrigger("FadeIn");
        menu.SetActive(true);
        menu.GetComponent<Animator>().SetBool("Open", true);

        yield return new WaitForSeconds(0.35f);

        canClick = true;
    }

    IEnumerator CloseMenu() {
        backDrop.GetComponent<Animator>().SetBool("FadeOut", true);
        menu.GetComponent<Animator>().SetBool("Open", false);
        menu.GetComponent<Animator>().SetBool("Close", true);

        yield return new WaitForSeconds(0.35f);

        menu.GetComponent<Animator>().SetBool("Close", false);
        menu.SetActive(false);
        backDrop.GetComponent<Animator>().SetBool("FadeOut", false);
        backDrop.GetComponent<Image>().color = new Color(0, 0, 0, 150f / 255f);
        backDrop.SetActive(false);

        canClick = true;
    }

    public void EndPause() {
        if(isOpen) {
            StartCoroutine(EndingPause());
        }
        else {
            gameObject.SetActive(false);
        }
    }

    IEnumerator EndingPause() {
        canClick = false;
        image.sprite = pauseButton;

        backDrop.GetComponent<Animator>().SetBool("FadeOut", true);
        menu.GetComponent<Animator>().SetBool("Open", false);
        menu.GetComponent<Animator>().SetBool("Close", true);

        yield return new WaitForSeconds(0.35f);

        menu.GetComponent<Animator>().SetBool("Close", false);
        menu.SetActive(false);
        backDrop.GetComponent<Animator>().SetBool("FadeOut", false);
        backDrop.GetComponent<Image>().color = new Color(0, 0, 0, 150f / 255f);
        backDrop.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ResumeButton() {
        GetComponent<AudioSource>().Play();
        
        isOpen = false;
        image.sprite = pauseButton;
        input.Unpaused();
        StartCoroutine(CloseMenu());
    }
}
