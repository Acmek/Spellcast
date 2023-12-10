using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using Photon.Pun;

public class ErrorCode : MonoBehaviour
{
    [SerializeField] private Sprite wrongSprite;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private float duration;
    [SerializeField] private float magnitude;
    [SerializeField] private InputCode input;
    [SerializeField] private InputMultiplayer inputMultiplayer;
    private SpriteRenderer sprite;
    private Vector3 originalPos;

    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalPos = transform.position;
    }

    public void Wrong() {
        sprite.sprite = wrongSprite;
        StartCoroutine(Shake());
    }

    public void Used() {
        sprite.sprite = lockSprite;
        StartCoroutine(Shake());
    }

    public IEnumerator Shake() {
        float elapsedTime = 0f;

        while(elapsedTime < duration) {
            float xOffset = Random.Range(-0.5f, 0.5f) * magnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * magnitude;

            transform.position = new Vector3(originalPos.x + xOffset, originalPos.y + yOffset, originalPos.z);

            yield return null;

            elapsedTime += Time.deltaTime;
        }

        transform.position = originalPos;
        sprite.sprite = null;
        if(PhotonNetwork.InRoom)
            inputMultiplayer.ClearInput();
        else
            input.ClearInput(); 
    }

    public void StopError() {
        sprite.sprite = null;
    }
}
