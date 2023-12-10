using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class UsernameCode : MonoBehaviour
{
    [SerializeField] private GameObject multiplayerMenu;
    [SerializeField] private GameObject usernameMenu;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject saving;
    [SerializeField] private GameObject logging;
    [SerializeField] private GameObject tryAgain;
    [SerializeField] private TMP_Text warning;
    [SerializeField] private TMP_InputField input;
    private string[] curses = {"fuck", "shit", "bitch", "ass", "bastard", "nigger", "nigga", "dick", "damn", "whore", "hoe", "cock", "cunt", "cuck", "pussy", "vagina", "penis", "faggot", "hell"};

    void Awake() {
        input.text = "<noparse>";
        StartCoroutine(Login());
    }

    public void InsertNoParse() {
        input.text = input.text.Replace("</noparse>", "");
    }

    public void TryAgain() {
        StartCoroutine(Login());
    }

    public void SetUsername() {
        bool curseCheck = false;
        foreach(string curse in curses) {
            if(input.text.Contains(curse)) {
                curseCheck = true;
                break;
            }
        }

        string testInput = input.text;
        testInput = testInput.Replace("<noparse>", "");

        if(string.IsNullOrWhiteSpace(input.text) || testInput.Equals("")) {
            warning.text = "*please enter a username*";
        }
        else if(input.text.Contains(" ")) {
            warning.text = "*username cannot contain spaces*";
        }
        else if(input.text.Contains("<br>") || input.text.Contains("\n")) {
            warning.text = "*username cannot contain line breaks*";
        }
        else if(input.text.Contains("\\")) {
            warning.text = "*username cannot contain \\*";
        }
        else if(curseCheck) {
            warning.text = "*username must me appropriate*";
        }
        else {
            loading.SetActive(true);
            continueButton.SetActive(false);
            warning.text = "";
            StartCoroutine(GetUsernames());
        }
    }

    IEnumerator Login() {
        logging.GetComponent<TMP_Text>().text = "Loading...";
        tryAgain.SetActive(false);
        if(Application.internetReachability != NetworkReachability.NotReachable) {
            UnityWebRequest usernamesData = UnityWebRequest.Get("https://crabby-colon.000webhostapp.com/Names.php");
            
            yield return usernamesData.SendWebRequest();
            
            string[] arr = usernamesData.downloadHandler.text.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
            usernamesData.Dispose();

            if(System.Array.IndexOf(arr, PlayerPrefs.GetString("username")) != -1) {
                logging.SetActive(false);
                usernameMenu.SetActive(false);
                multiplayerMenu.SetActive(true);
            }
            else {
                logging.SetActive(false);
                usernameMenu.SetActive(true);
                multiplayerMenu.SetActive(false);
            }
        }
        else {
            tryAgain.SetActive(true);
            logging.GetComponent<TMP_Text>().text = "Please connect to the internet";
        }
    }

    IEnumerator GetUsernames() {
        UnityWebRequest usernamesData = UnityWebRequest.Get("https://crabby-colon.000webhostapp.com/Names.php");
        
        yield return usernamesData.SendWebRequest();
        
        string[] arr = usernamesData.downloadHandler.text.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
        usernamesData.Dispose();

        if(System.Array.IndexOf(arr, input.text) != -1) {
            warning.text = "*this username has already been taken*";
            loading.SetActive(false);
            continueButton.SetActive(true);
        }
        else {
            StartCoroutine(InsertUsername());
        }
    }

    IEnumerator InsertUsername() {
        usernameMenu.SetActive(false);
        saving.SetActive(true);

        WWWForm form = new WWWForm();
        form.AddField("usernamePost", input.text);

        UnityWebRequest www = UnityWebRequest.Post("https://crabby-colon.000webhostapp.com/Insert.php", form);
        yield return www.SendWebRequest();
        www.Dispose();

        PlayerPrefs.SetString("username", input.text);
        multiplayerMenu.SetActive(true);
    }
}