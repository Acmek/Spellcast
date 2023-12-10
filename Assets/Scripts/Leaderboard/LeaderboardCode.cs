using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class LeaderboardCode : MonoBehaviour
{
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject leaderboard;
    [SerializeField] private GameObject[] podium;
    [SerializeField] private string[] costumeNames;
    [SerializeField] private Sprite[] costumes;
    [SerializeField] private TMP_Text countdown;
    [SerializeField] private Transform ranks;
    [SerializeField] private GameObject rankPrefab;
    [SerializeField] private GameObject popUp;
    private List<string[]> players;
    private UnityWebRequest usernamesData;
    private int[] timer;
    private bool refreshing;

    void Start() {
        players = new List<string[]>();
    }

    void Update() {
        timer = new int[] {6 - (int)System.DateTime.UtcNow.DayOfWeek, 23 - (int)System.DateTime.UtcNow.Hour, 59 - (int)System.DateTime.UtcNow.Minute, 59 - (int)System.DateTime.UtcNow.Second};
        countdown.text = "Leaderboard Resets In " + timer[0] + ":";
        if(timer[1] < 10)
            countdown.text += "0" + timer[1] + ":";
        else
            countdown.text += timer[1] + ":";
        
        if(timer[2] < 10)
            countdown.text += "0" + timer[2] + ":";
        else
            countdown.text += timer[2] + ":";

        if(timer[3] < 10)
            countdown.text += "0" + timer[3];
        else
            countdown.text += timer[3];

        if(timer[0] == 6 && timer[1] == 23 && timer[2] == 59 && timer[3] == 59 && !refreshing) {
            popUp.SetActive(false);
            OnDisable();
            StartCoroutine(GetData());
            refreshing = true;
        }
        if(refreshing && timer[0] == 6 && timer[1] == 23 && timer[2] == 59 && timer[3] == 58)
            refreshing = false;
    }

    void OnEnable() {
        if(refreshing)
            refreshing = false;

        StartCoroutine(GetData());
    }

    IEnumerator GetData() { // Retrieve Leaderboard From SQL and Display
        leaderboard.SetActive(false);
        loading.SetActive(true); //Display Loading Screen and Hide Leaderboard

        usernamesData = UnityWebRequest.Get("https://crabby-colon.000webhostapp.com/GetData.php"); // Create URL to Access Website and SQL PHP File
        yield return usernamesData.SendWebRequest();
        string[] arr = usernamesData.downloadHandler.text.Split(" ", System.StringSplitOptions.RemoveEmptyEntries); // Convert Leaderboard Data Into Array of Each Player
        
        usernamesData.Dispose(); // Dispose of Data to Avoid Memory Leak

        for(int i = 0; i < arr.Length; i++) { // Runs through Array of Each Player
            players.Add(arr[i].Split("\\", System.StringSplitOptions.RemoveEmptyEntries)); // Sorts Players Data (Username, Wins, Losses) Into an Array and Adds to a List
            if(i >= 0 && i <= 2) { // Display Player on Podium
                podium[i].SetActive(true);

                if(System.Array.IndexOf(costumeNames, players[i][6]) != -1) // Display Player's Costume
                    podium[i].GetComponent<Image>().sprite = costumes[System.Array.IndexOf(costumeNames, players[i][6])];
                else
                    podium[i].GetComponent<Image>().sprite = costumes[0];

                podium[i].transform.GetChild(0).GetComponent<TMP_Text>().text = players[i][0]; // Display Number of Wins and Losses
                podium[i].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "W: " + players[i][1] + "   " + "L: " + players[i][2];

                if(players[i][0].Equals(PlayerPrefs.GetString("username"))) { // Change Text to Yellow if Current Player is You
                    podium[i].transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                    podium[i].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                }
            }
            else { // Display Player Underneath Podium
                GameObject rankObj = Instantiate(rankPrefab, new Vector3(0, 0, 0), Quaternion.identity, ranks);

                rankObj.GetComponent<TMP_Text>().text = (i + 1) + ". " + players[i][0] + " - " + "W: " + players[i][1] + "   " + "L: " + players[i][2]; // Display Wins and Losses
                rankObj.SetActive(true);

                if(players[i][0].Equals(PlayerPrefs.GetString("username"))) // Change Text to Yellow if Current Player is You
                    rankObj.GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
            }
            yield return null;
        }

        leaderboard.SetActive(true);
        loading.SetActive(false); // Removes Loading Screen and Refrshes New Leaderboard
    }

    void OnDisable() {
        usernamesData.Dispose();

        foreach(GameObject place in podium)
            place.SetActive(false);
        
        for(int i = 1; i < ranks.childCount; i++)
            Destroy(ranks.GetChild(i).gameObject);
    }

    public void SetPopup(int rankNum) {
        if(!popUp.activeSelf) {
            popUp.SetActive(true);

            GetComponent<AudioSource>().time = 0.2f;
            GetComponent<AudioSource>().Play();

            popUp.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = (rankNum + 1) + ". " + players[rankNum][0];
            if(System.Array.IndexOf(costumeNames, players[rankNum][6]) != -1)
                popUp.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>().sprite = costumes[System.Array.IndexOf(costumeNames, players[rankNum][6])];
            else
                popUp.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>().sprite = costumes[0];
            popUp.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = players[rankNum][1];
            popUp.transform.GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = players[rankNum][2];
            popUp.transform.GetChild(0).GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = players[rankNum][3];
            popUp.transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = players[rankNum][4];
            popUp.transform.GetChild(0).GetChild(0).GetChild(6).GetChild(0).GetComponent<TMP_Text>().text = players[rankNum][5];

            int timeNum = int.Parse(players[rankNum][7]);
            if(timeNum >= 60) {
                float timeMin = Mathf.Round((float)(timeNum / 60f) * 100.0f) * 0.01f;

                if(timeMin >= 60) {
                    float timeDiv = Mathf.Round((timeMin / 60f) * 100.0f) * 0.01f;

                    if(timeDiv == 1)
                        popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().text = timeDiv + " Hour";
                    else
                        popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().text = timeDiv + " Hours";
                }
                else {
                    if(timeMin == 1)
                        popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().text = timeMin + " Minute";
                    else
                        popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().text = timeMin + " Minutes";
                }
            }
            else {
                if(timeNum == 1)
                    popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().text = timeNum + " Second";
                else
                    popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().text = timeNum + " Seconds";
            }

            if(players[rankNum][0].Equals(PlayerPrefs.GetString("username"))) {
                popUp.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);

                popUp.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(4).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(5).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(6).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                
                popUp.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(6).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 0, 1);
            }
            else {
                popUp.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);

                popUp.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(4).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(5).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(6).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                
                popUp.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(4).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(6).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
                popUp.transform.GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1);
            }
        }
    }

    public void RemovePopup() {
        popUp.SetActive(false);

        GetComponent<AudioSource>().time = 0.2f;
        GetComponent<AudioSource>().Play();
    }
}
