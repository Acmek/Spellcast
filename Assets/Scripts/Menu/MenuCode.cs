using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class MenuCode : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private Animator animator;
    [SerializeField] private MenuCode book;
    [SerializeField] private MenuCode wand;
    [SerializeField] private MenuCode costume;
    [SerializeField] private GameObject backDrop;
    [SerializeField] private bool isTitle;
    [SerializeField] private bool ignoreUpdate;
    private bool isOver;
    private bool isClicked;

    void Update() {
        if(!ignoreUpdate) {
            if(isOver) {
                animator.SetBool("Over", true);

                if(Input.GetMouseButtonDown(0)) {
                    animator.SetBool("Clicked", true);
                    isClicked = true;
                }
                if(Input.GetMouseButtonUp(0) && isClicked) {
                    animator.SetBool("Clicked", false);
                    isClicked = false;
                    book.enabled = false;
                    wand.enabled = false;
                    costume.enabled = false;
                    StartCoroutine(OpenMenu());
                }
            }
            else {
                animator.SetBool("Over", false);

                if(Input.GetMouseButtonUp(0)) {
                    animator.SetBool("Clicked", false);
                    isClicked = false;
                }
            }
        }
    }

    void OnMouseEnter() {
        isOver = true;
    }

    void OnMouseExit() {
        isOver = false;
    }

    public void ForceOpen() {
        if(!isTitle) {
            book.enabled = false;
            wand.enabled = false;
            costume.enabled = false;
        }
        StartCoroutine(OpenMenu());
    }

    IEnumerator OpenMenu() {
        yield return new WaitForSeconds(0.06f);

        GetComponent<AudioSource>().Play();

        backDrop.SetActive(true);
        backDrop.GetComponent<Animator>().SetTrigger("FadeIn");
        menu.SetActive(true);
        menu.GetComponent<Animator>().SetBool("Open", true);
    }

    public void ExitMenu() {
        StartCoroutine(CloseMenuAnimation());
    }

    IEnumerator CloseMenuAnimation() {
        GetComponent<AudioSource>().Play();
        
        backDrop.GetComponent<Animator>().SetBool("FadeOut", true);
        menu.GetComponent<Animator>().SetBool("Open", false);
        menu.GetComponent<Animator>().SetBool("Close", true);

        yield return new WaitForSeconds(0.35f);

        menu.GetComponent<Animator>().SetBool("Close", false);
        menu.SetActive(false);
        backDrop.GetComponent<Animator>().SetBool("FadeOut", false);
        backDrop.GetComponent<Image>().color = new Color(0, 0, 0, 150f / 255f);
        backDrop.SetActive(false);

        if(!isTitle) {
            book.enabled = true;
            wand.enabled = true;
            costume.enabled = true;
        }
    }

    public void ExitGame() {
        Application.Quit();
    }

    public void EscapePressed() {
        StartCoroutine(EscapeQuit());
    }

    IEnumerator EscapeQuit() {
        GetComponent<AudioSource>().Play();

        menu.GetComponent<Animator>().SetBool("Open", false);
        menu.GetComponent<Animator>().SetBool("Close", true);

        yield return new WaitForSeconds(0.35f);

        menu.GetComponent<Animator>().SetBool("Close", false);
        menu.SetActive(false);
    }
}
