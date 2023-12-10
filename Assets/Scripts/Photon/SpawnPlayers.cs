using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector2 spawnPos;

    private void Awake() {
        PhotonNetwork.Instantiate("Multiplayer/" + playerPrefab.name, spawnPos, Quaternion.identity);
    }
}
