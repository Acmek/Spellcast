using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Photon.Pun;

public class BallCode : MonoBehaviourPunCallbacks
{
    [SerializeField] private bool isPowerup;
    [SerializeField] private GameObject spell;
    [SerializeField] private SpriteRenderer ballSprite;
    [SerializeField] private Animator ballAnimator;
    [SerializeField] private GameObject dot;
    [SerializeField] private int bombAmount;
    [SerializeField] private AudioClip ballSelect;
    [SerializeField] private AudioClip ballDeselect;
    private InputCode input;
    private InputMultiplayer inputMultiplayer;
    private bool selected;
    private bool canDeselect;
    private bool bombSet;
    private AudioSource audioSource;
    private PhotonView view;
    private bool stop;

    void Start() {
        view = GetComponent<PhotonView>();
        if(GameObject.Find("Input") != null) {
            if(PhotonNetwork.InRoom) {
                transform.SetParent(GameObject.Find("BallHolder").transform);
                inputMultiplayer = GameObject.Find("Input").GetComponent<InputMultiplayer>();
            }
            else
                input = GameObject.Find("Input").GetComponent<InputCode>();
        }
        if(!isPowerup && (!PhotonNetwork.InRoom || (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)))
            ballSprite.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        if(Input.GetMouseButtonUp(0)) {
            selected = false;
            canDeselect = false;
        }
    }

    void OnMouseOver() { // Called When Mouse Hovers Over Ball
        if(!stop) {
            if(PhotonNetwork.InRoom) {
                if(!selected && inputMultiplayer.AddLetter(gameObject)) { // Tests If Ball Can Be Added To Input
                    selected = true;
                    dot.SetActive(true); // "dot" Is A Cosmetic Feature To Show Ball Is Selected
                    ballAnimator.SetTrigger("Select");

                    audioSource.clip = ballSelect;
                    audioSource.volume = 0.5f;
                    audioSource.Play();
                }
            }
            else {
                if(!selected && input.AddLetter(gameObject)) { // Tests If Ball Can Be Added To Input
                    selected = true;
                    dot.SetActive(true); // "dot" Is A Cosmetic Feature To Show Ball Is Selected
                    ballAnimator.SetTrigger("Select");

                    audioSource.clip = ballSelect;
                    audioSource.volume = 0.5f;
                    audioSource.Play();
                }
            }
            if(canDeselect) { // Tests If Ball Is Being Deselected And Removed From Input
                if(PhotonNetwork.InRoom)
                    inputMultiplayer.ClearBall(gameObject);
                else
                    input.ClearBall(gameObject);

                audioSource.clip = ballDeselect;
                audioSource.volume = 0.05f;
                audioSource.Play();
                
                canDeselect = false;
            }
        }
    }

    void OnMouseExit() {
        if(selected) {
            canDeselect = true;
        }
    }

    public void Deselect() {
        canDeselect = false;
        selected = false;
        dot.SetActive(false);
    }

    public void TurnPowerup(GameObject newSpell, int bombNum) {
        isPowerup = true;
        spell = newSpell;
        bombAmount = bombNum;
    }

    public bool IsPowerup() {
        return isPowerup;
    }

    public GameObject GetSpell() {
        return spell;
    }

    public int GetBombAmount() {
        if(bombAmount > 0 && GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("bomb") > 0)
            return bombAmount + (GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("bomb") - 1);
        return bombAmount;
    }

    public void SetBombBall() {
        bombSet = true;
    }

    public bool IsSet() {
        return bombSet;
    }

    public void BallDestroying(bool param) {
        stop = param;
    }

    public bool IsDestroying() {
        return stop;
    }

    public void ChangeBall(string letter, float r, float g, float b) {
        view.RPC("ChangeBallRPC", RpcTarget.Others, letter, r, g, b);
    }

    [PunRPC]
    private void ChangeBallRPC(string letter, float r, float g, float b) {
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = letter;
        if(letter.Equals("M") || letter.Equals("W")) {
            transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().fontStyle = FontStyles.Bold | FontStyles.Underline;
        }
        else {
            transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        }
        transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(r, g, b, 1);
    }
}
