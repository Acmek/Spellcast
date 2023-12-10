using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseCode : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private PauseButtonCode pause;
    [SerializeField] private ButtonCode button;

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(menu.activeSelf)
                button.MainMenu();
            else
                pause.PauseEscape();
        }
    }
}
