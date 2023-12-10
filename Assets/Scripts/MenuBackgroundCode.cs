using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackgroundCode : MonoBehaviour
{
    [SerializeField] private Sprite middleLevel;
    [SerializeField] private Sprite topLevel;
    [SerializeField] private SpriteRenderer[] backgrounds;

    // Start is called before the first frame update
    void Start()
    {
        if(System.Array.IndexOf(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLevels(), "10") != -1)
            foreach(SpriteRenderer background in backgrounds)
                background.sprite = topLevel;
        else if(System.Array.IndexOf(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLevels(), "5") != -1)
            foreach(SpriteRenderer background in backgrounds)
                background.sprite = middleLevel;
    }
}
