using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class VersusCode : MonoBehaviourPunCallbacks, IConnectionCallbacks
{
    [SerializeField] private Image playerImage;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private GameObject roomCode;
    [SerializeField] private GameObject transition;
    [SerializeField] private GameObject find;
    [SerializeField] private TitleScreenCode exit;
    [SerializeField] private GameObject exitBut;
    [SerializeField] private GameObject enemyIcon;
    [SerializeField] private GameObject canvasObj;
    [SerializeField] private GameObject backMusic;
    private PhotonView view;
    private Coroutine findAny;
    private bool starting;

    void Start() {
        view = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;
        playerName.text = PlayerPrefs.GetString("username");
        playerImage.sprite = GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumePlayer();

        if(PhotonNetwork.CurrentRoom.IsVisible)
            roomCode.SetActive(false);
        else
            roomCode.transform.GetChild(0).GetComponent<TMP_Text>().text = PhotonNetwork.CurrentRoom.Name;

        findAny = StartCoroutine(FindAny());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        view.RPC("PlayerJoined", RpcTarget.Others, GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeIndex(), PlayerPrefs.GetString("username"));
    }

    [PunRPC]
    private void PlayerJoined(int playerCostumeInput, string playerNameInput) {
        if(!starting) {
            starting = true;
            view.RPC("PlayerJoined", RpcTarget.Others, GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeIndex(), PlayerPrefs.GetString("username"));
        
            StopCoroutine(findAny);
            find.SetActive(false);

            exitBut.SetActive(false);

            if(!PhotonNetwork.CurrentRoom.IsVisible)
                roomCode.SetActive(false);

            if(enemyIcon.transform.GetChild(0).GetComponent<LoadingAnimation>().enabled) {
                enemyIcon.transform.GetChild(0).GetComponent<LoadingAnimation>().StopAnim();
                enemyIcon.transform.GetChild(0).GetComponent<LoadingAnimation>().enabled = false;
            }
            enemyIcon.transform.GetChild(0).GetComponent<TMP_Text>().text = playerNameInput;

            if(enemyIcon.GetComponent<LoadingAnimation>().enabled) {
                enemyIcon.GetComponent<LoadingAnimation>().StopAnim();
                enemyIcon.GetComponent<LoadingAnimation>().enabled = false;
            }
            enemyIcon.GetComponent<Image>().sprite = GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeArr()[playerCostumeInput];
            enemyIcon.GetComponent<Image>().color = new Color(1, 1, 1, 1);

            canvasObj.GetComponent<Animator>().SetBool("Found", true);

            backMusic.GetComponent<AudioSource>().Stop();

            StartCoroutine(StartMatch());
        }
    }
    
    IEnumerator StartMatch() {
        yield return new WaitForSeconds(1);
        
        GameObject.Find("Canvas").GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(3f);

        transition.SetActive(true);
        transition.GetComponent<AudioSource>().Play();
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);
        
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Multiplayer");
    }

    IEnumerator FindAny() {
        yield return new WaitForSeconds(10f);

        if(!exit.GetStarted())
            if((int)PhotonNetwork.CurrentRoom.CustomProperties["rank"] != -1) {
                find.SetActive(true);
            }
            else {
                find.SetActive(true);
                find.GetComponent<TMP_Text>().text = "Looks like no one is playing right now!";
                find.transform.GetChild(0).gameObject.SetActive(false);
            }
    }

    public void FindAnyPlayer() {
        if(!exit.GetStarted()) {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;

            exit.SetStarted();
            find.GetComponent<LoadingAnimation>().enabled = true;

            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinRandomRoom(new Hashtable { {"rank", -1} }, (byte)2);
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        RoomOptions roomOptions = new RoomOptions() {
            MaxPlayers = (byte)2,
            IsVisible = true,
            CustomRoomProperties = new Hashtable { {"rank", -1} }
        };
        EnterRoomParams createRoomParams = new EnterRoomParams();
        createRoomParams.RoomOptions = roomOptions;

        roomOptions.CustomRoomPropertiesForLobby = new string[] {"rank"};

        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    public override void OnJoinedRoom() {
        //StartCoroutine(JoiningRoom()); doesn't work?
        PhotonNetwork.LoadLevel("Versus");
    }
}
