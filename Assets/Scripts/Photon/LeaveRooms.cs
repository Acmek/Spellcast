using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LeaveRooms : MonoBehaviourPunCallbacks
{
    private void Start() {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom() {
        SceneManager.LoadScene("MultiplayerMenu");
    }
}
