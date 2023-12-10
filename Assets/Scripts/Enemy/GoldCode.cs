using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldCode : MonoBehaviour
{
    private float gold;

    // Start is called before the first frame update
    void Start()
    {
        var emission = GetComponent<ParticleSystem>().emission;
        emission.rateOverTime = gold / 4f;
        GetComponent<ParticleSystem>().Play();
    }

    public void SetGold(int num) {
        gold = num;
    }
}
