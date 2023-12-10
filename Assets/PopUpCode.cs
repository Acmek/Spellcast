using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpCode : MonoBehaviour
{
    [SerializeField] private LeaderboardCode board;
    
    public void PopUpClicked() {
        if(gameObject.name == "1")
            board.SetPopup(0);
        else if(gameObject.name == "2")
            board.SetPopup(1);
        else if(gameObject.name == "3")
            board.SetPopup(2);
        else
            board.SetPopup(transform.GetSiblingIndex() + 2);
    }

    void OnMouseExit() {
        board.RemovePopup();
    }
}
