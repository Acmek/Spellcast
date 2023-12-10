using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class ButtonCode : MonoBehaviour
{
    public void NextLevel() {
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel() {
        GetComponent<AudioSource>().Play();
        
        GameObject.Find("Transition").GetComponent<TransitionCode>().CloseScene();

        yield return new WaitForSeconds(0.75f);

        GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().SetLastLevel(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() + 1);
        SceneManager.LoadScene("Levels");
    }

    public void Retry() {
        StartCoroutine(LoadRetry());
    }

    IEnumerator LoadRetry() {
        GetComponent<AudioSource>().Play();

        GameObject.Find("Transition").GetComponent<TransitionCode>().CloseScene();

        yield return new WaitForSeconds(0.75f);

        SceneManager.LoadScene("Levels");    
    }

    public void MainMenu() {
        StartCoroutine(LoadMainMenu());
    }

    IEnumerator LoadMainMenu() {
        GetComponent<AudioSource>().Play();
        
        GameObject.Find("Transition").GetComponent<TransitionCode>().CloseScene();

        yield return new WaitForSeconds(0.75f);

        Destroy(GameObject.Find("PlayerSaves"));
        SceneManager.LoadScene("MainMenu");
    }
}
