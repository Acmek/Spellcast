using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideCode : MonoBehaviour
{
    [SerializeField] private float startTime;
    [SerializeField] private float lifeTime;
    
    // Start is called before the first frame update
    void Start()
    {
        if(GetComponent<AudioSource>() != null)
            GetComponent<AudioSource>().time = startTime;
        StartCoroutine(Die());
    }

    IEnumerator Die() {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
