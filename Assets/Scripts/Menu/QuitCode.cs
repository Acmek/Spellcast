using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitCode : MonoBehaviour
{
    [SerializeField] private bool isTitle;
    [SerializeField] private GameObject menu;
    [SerializeField] private MenuCode code;
    [SerializeField] private MenuCode[] menus;

    void Update() {
        if(!isTitle) {
            if(Input.GetKeyDown(KeyCode.Escape)) {
                if(menu.activeSelf)
                    Application.Quit();
                else {
                    for(int i = 0; i < menus.Length; i++)
                        menus[i].EscapePressed();

                    code.ForceOpen();
                }
            }
        }
        else {
            if(Input.GetKeyDown(KeyCode.Escape)) {
                Application.Quit();
            }
        }
    }
}
