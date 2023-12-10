using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject transition;
    [SerializeField] private TMP_InputField createInput;
    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private GameObject rankMenu;
    [SerializeField] private GameObject rankLoading;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text rankName;
    [SerializeField] private Image rankBar;
    private int rank;
    private bool pressed;

    private LoadBalancingClient loadBalancingClient;

    void Start() {
        StartCoroutine(SetRank());
    }

    IEnumerator SetRank() {
        rankMenu.SetActive(false);
        rankLoading.SetActive(true);

        playerName.text = PlayerPrefs.GetString("username");
        
        WWWForm form = new WWWForm();
        form.AddField("usernamePost", PlayerPrefs.GetString("username"));

        UnityWebRequest www = UnityWebRequest.Post("https://crabby-colon.000webhostapp.com/GetRank.php", form);
        yield return www.SendWebRequest();
        string[] arr = www.downloadHandler.text.Split("\\", System.StringSplitOptions.RemoveEmptyEntries);
        
        www.Dispose();
        
        rank = int.Parse(arr[0]);
        rankName.text = arr[1];
        rankBar.fillAmount = float.Parse(arr[2]);

        if(int.Parse(arr[3]) == 1)
            rankBar.color = new Color(0.88f, 0, 0, 1);

        rankMenu.SetActive(true);
        rankLoading.SetActive(false);
    }

    public void RankedMatch() {
        if(!pressed) {
            pressed = true;

            PhotonNetwork.JoinRandomRoom(new Hashtable { {"rank", rank} }, (byte)2);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        RoomOptions roomOptions = new RoomOptions() {
            MaxPlayers = (byte)2,
            IsVisible = true,
            CustomRoomProperties = new Hashtable { {"rank", rank} }
        };
        EnterRoomParams createRoomParams = new EnterRoomParams();
        createRoomParams.RoomOptions = roomOptions;

        roomOptions.CustomRoomPropertiesForLobby = new string[] {"rank"};

        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    public void CreateRoomPressed() {
        if(!string.IsNullOrWhiteSpace(createInput.text) && !pressed) {
            pressed = true;
            RoomOptions roomOptions = new RoomOptions() {
                MaxPlayers = (byte)2,
                IsVisible = false
            };
            PhotonNetwork.CreateRoom(createInput.text.ToUpper(), roomOptions, null);
        }
    }

    public void JoinRoom() {
        if(!string.IsNullOrWhiteSpace(joinInput.text) && !pressed) {
            pressed = true;
            PhotonNetwork.JoinRoom(joinInput.text.ToUpper());
        }
    }

    public override void OnJoinedRoom() {
        //StartCoroutine(JoiningRoom()); doesn't work?
        PhotonNetwork.LoadLevel("Versus");
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        pressed = false;
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        pressed = false;
    }

    /*IEnumerator JoiningRoom() {
        transition.SetActive(true);
        transition.GetComponent<AudioSource>().Play();
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);

        PhotonNetwork.LoadLevel("Versus");
    }*/
}
