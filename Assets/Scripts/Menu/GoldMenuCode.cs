using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GoldMenuCode : MonoBehaviour
{
    [SerializeField] private PlayerSave playerSaves;

    void Update() {
        GetComponent<TMP_Text>().text = playerSaves.gold + "";
    }
}
