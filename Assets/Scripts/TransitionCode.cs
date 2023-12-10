using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionCode : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool isTitleScreen;

    void Start() {
        if(!isTitleScreen) {
            GetComponent<AudioSource>().Play();
            animator.SetTrigger("Open");
        }
    }

    public void CloseScene() {
        GetComponent<AudioSource>().Play();
        animator.SetTrigger("Close");
    }
}
