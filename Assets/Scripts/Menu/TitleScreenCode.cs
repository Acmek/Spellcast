using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class TitleScreenCode : MonoBehaviour
{
    [SerializeField] private MenuCode menu;
    [SerializeField] private GameObject transition;
    [SerializeField] private GameObject[] objs;
    [SerializeField] private GameObject resetting;
    private bool gameStarted;

    public void StartGame() {
        if(!gameStarted) {
            GetComponent<AudioSource>().Play();
            
            gameStarted = true;
            StartCoroutine(LoadGame());
        }
    }

    IEnumerator LoadGame() {
        transition.SetActive(true);
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);

        SceneManager.LoadScene("MainMenu");
    }

    public void StartMultiplayer() {
        if(!gameStarted) {
            GetComponent<AudioSource>().Play();
            
            gameStarted = true;
            StartCoroutine(LoadMultiplayer());
        }
    }

    IEnumerator LoadMultiplayer() {
        transition.SetActive(true);
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);

        SceneManager.LoadScene("LoadingConnector");
    }

    public void StartSingleplayer() {
        if(!gameStarted) {
            GetComponent<AudioSource>().Play();
            
            gameStarted = true;
            StartCoroutine(LoadSingleplayer());
        }
    }

    IEnumerator LoadSingleplayer() {
        transition.SetActive(true);
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);

        if(PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public void LeaveVersus() {
        if(!gameStarted) {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            GetComponent<AudioSource>().Play();
            
            gameStarted = true;
            StartCoroutine(LoadReturn());
        }
    }

    IEnumerator LoadReturn() {
        transition.SetActive(true);
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);

        SceneManager.LoadScene("Loading");
    }

    public void ReturnToMainMenu() {
        if(!gameStarted) {
            gameStarted = true;
            
            if(PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void Quit() {
        if(!gameStarted) {
            Application.Quit();
        }
    }

    public void HowToPlay() {
        if(!gameStarted) {
            menu.ForceOpen();
        }
    }

    public void SetStarted() {
        gameStarted = true;
    }

    public bool GetStarted() {
        return gameStarted;
    }

    public void ResetSave() {
        StartCoroutine(LoadReset());
    }

    IEnumerator LoadReset() {
        foreach(GameObject arrObj in objs) {
            if(arrObj.name.Equals("ResetSave")) {
                arrObj.GetComponent<Image>().enabled = false;
                arrObj.transform.GetChild(0).gameObject.SetActive(false);
            }
            else {
                arrObj.SetActive(false);
            }
        }
        resetting.SetActive(true);

        if(!string.IsNullOrWhiteSpace(PlayerPrefs.GetString("username"))) {
            WWWForm form = new WWWForm();
            form.AddField("usernamePost", PlayerPrefs.GetString("username"));

            UnityWebRequest www = UnityWebRequest.Post("https://crabby-colon.000webhostapp.com/ResetSave.php", form);
            yield return www.SendWebRequest();
            www.Dispose();
        }

        PlayerPrefs.DeleteAll();


        transition.SetActive(true);
        transition.GetComponent<Animator>().SetTrigger("Close");

        yield return new WaitForSeconds(0.75f);

        SceneManager.LoadScene("MainMenu");
    }
}
