using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LoadRematch : MonoBehaviourPunCallbacks
{
    private PhotonView view;
    private bool loaded;

    void Start() {
        view = GetComponent<PhotonView>();
        loaded = true;
        view.RPC("SendLoaded", RpcTarget.Others);
    }

    [PunRPC]
    private void SendLoaded() {
        if(loaded)
            if(PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel("Multiplayer");
    }
}
