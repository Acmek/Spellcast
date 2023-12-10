using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject transition;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button rematchButton;
    [SerializeField] private GameObject rematchText;
    [SerializeField] private GameObject rematchBeginning;
    [SerializeField] private Button newGameButton;
    [SerializeField] private GameObject newGameSearch;
    private int rank;
    
    private PhotonView view;
    private bool sceneLoading;
    private bool rematchPressed;

    void Start() {
        view = GetComponent<PhotonView>();
    }

    public void UploadPlayerData(int wins, int losses, int letters, int words, string longest, string costume, int time) {
        StartCoroutine(InsertData(wins, losses, letters, words, longest, costume, time));
    }

    IEnumerator InsertData(int wins, int losses, int letters, int words, string longest, string costume, int time) {
        endGameScreen.SetActive(false);
        loading.SetActive(true);

        WWWForm form = new WWWForm();
        form.AddField("usernamePost", PlayerPrefs.GetString("username"));
        form.AddField("winsPost", wins);
        form.AddField("lossesPost", losses);
        form.AddField("lettersPost", letters);
        form.AddField("wordsPost", words);
        form.AddField("longestPost", longest);
        form.AddField("costumePost", costume);
        form.AddField("timePost", time);

        UnityWebRequest www = UnityWebRequest.Post("https://crabby-colon.000webhostapp.com/InsertData.php", form);
        yield return www.SendWebRequest();
        www.Dispose();

        loading.SetActive(false);
        endGameScreen.SetActive(true);
    }

    public void MultiplayerMenu() {
        rematchButton.interactable = false;
        mainMenuButton.interactable = false;
        newGameButton.interactable = false;

        PhotonNetwork.AutomaticallySyncScene = false;
        
        view.RPC("PlayerLeft", RpcTarget.Others, PlayerPrefs.GetString("username"));
        StartCoroutine(LoadReturn());
    }

    IEnumerator LoadReturn() {
        transition.SetActive(true);
        transition.GetComponent<AudioSource>().Play();
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);

        SceneManager.LoadScene("Loading");
    }

    [PunRPC]
    private void PlayerLeft(string playerName) {
        rematchText.SetActive(true);
        rematchText.GetComponent<TMP_Text>().text = playerName + " HAS LEFT";
        rematchButton.interactable = false;
    }

    public void Rematch() {
        rematchPressed = true;
        rematchButton.interactable = false;

        if(!rematchText.activeSelf) {
            rematchText.SetActive(true);
            rematchText.GetComponent<TMP_Text>().text = "REQUESTED A REMATCH!";
        }

        view.RPC("RematchPressed", RpcTarget.Others, PlayerPrefs.GetString("username"));
    }

    [PunRPC]
    private void RematchPressed(string playerName) {
        if(rematchPressed) {
            view.RPC("StartRematch", RpcTarget.All);
        }
        else {
            rematchText.SetActive(true);
            rematchText.GetComponent<TMP_Text>().text = playerName + " HAS REQUESTED A REMATCH!";
        }
    }

    [PunRPC]
    private void StartRematch() {
        rematchButton.interactable = false;
        mainMenuButton.interactable = false;
        newGameButton.interactable = false;

        rematchText.SetActive(false);
        rematchBeginning.SetActive(true);

        PhotonNetwork.AutomaticallySyncScene = true;
        StartCoroutine(StartMatch());
    }

    IEnumerator StartMatch() {
        yield return new WaitForSeconds(1f);

        transition.SetActive(true);
        transition.GetComponent<AudioSource>().Play();
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);
        
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("LoadRematch");
    }

    public void NewGame() {
        rematchButton.interactable = false;
        mainMenuButton.interactable = false;
        newGameButton.interactable = false;

        rematchText.SetActive(false);
        newGameSearch.SetActive(true);

        view.RPC("PlayerLeft", RpcTarget.Others, PlayerPrefs.GetString("username"));
        
        PhotonNetwork.LeaveRoom();
    }

    public override void OnConnectedToMaster() {
        StartCoroutine(SetRank());

        PhotonNetwork.JoinRandomRoom(new Hashtable { {"rank", rank} }, (byte)2);
    }

    IEnumerator SetRank() {
        WWWForm form = new WWWForm();
        form.AddField("usernamePost", PlayerPrefs.GetString("username"));

        UnityWebRequest www = UnityWebRequest.Post("https://crabby-colon.000webhostapp.com/GetRank.php", form);
        yield return www.SendWebRequest();
        string[] arr = www.downloadHandler.text.Split("\\", System.StringSplitOptions.RemoveEmptyEntries);
        
        www.Dispose();
        
        rank = int.Parse(arr[0]);
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

    public override void OnJoinedRoom() {
        PhotonNetwork.LoadLevel("Versus");
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        // set end game screen saying "oh!" and try again or return to main menu
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        //
    }
}
