using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCostumeCode : MonoBehaviour
{
    private SpriteRenderer spriteRen;

    // Start is called before the first frame update
    void Start()
    {
        spriteRen = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(spriteRen.sprite != GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumePlayer()) {
            spriteRen.sprite = GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumePlayer();
        }
    }
}
