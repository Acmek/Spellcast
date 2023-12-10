using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ComboCode : MonoBehaviour
{
    [SerializeField] private float resetTime;
    [SerializeField] private PlayerCode player;
    private TMP_Text text;
    private float timer;
    private int combo;

    private int highestCombo;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer < resetTime)
            timer += Time.deltaTime;
        else {
            if(combo > 0) {
                combo = 0;
                text.text = "";
            }
        }
    }

    public void AddCombo() {
        combo += 1;

        if(combo > highestCombo) {
            player.SetHighestCombo(combo);
            highestCombo = combo;
        }
            
        text.text = "x" + combo;
        timer = 0;
    }

    public float GetCombo() {
        if(combo == 0) {
            return 0;
        }
        return combo - 1;
    }

    public void DelayCombo() {
        timer = 0;
    }

    public void ResetCombo() {
        timer = resetTime;
    }
}
