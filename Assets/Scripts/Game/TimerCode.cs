using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class TimerCode : MonoBehaviour
{
    [SerializeField] private Image topSand;
    [SerializeField] private Image bottomSand;
    [SerializeField] private GameObject sandFalling;
    private Animator animator;
    private float maxTime;
    private float timer;
    private bool stopped;
    private bool frozen;

    // Start is called before the first frame update
    void Start()
    {
        stopped = true;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0) {
            if(!frozen) {
                topSand.color = new Color(0.945f, 0.882f, 0.576f, 1);
                bottomSand.color = new Color(0.945f, 0.882f, 0.576f, 1);
                sandFalling.GetComponent<Image>().color = new Color(0.945f, 0.882f, 0.576f, 1);

                timer -= Time.deltaTime;
            }
        }
        else {
            sandFalling.SetActive(false);
        }

        if(stopped) {
            animator.SetBool("Spin", false);
            transform.rotation = new Quaternion(0, 0, 0, 1);
            maxTime = 0;
            timer = 0;
            sandFalling.SetActive(false);
        }

        topSand.fillAmount = timer / maxTime;
        bottomSand.fillAmount = 1 - (timer / maxTime);
    }

    public IEnumerator ResetTimer(float num) {
        stopped = false;
        animator.SetBool("Spin", true);
        GetComponent<AudioSource>().time = 0;
        GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(0.35f);

        maxTime = num;
        timer = maxTime;
        sandFalling.SetActive(true);

        yield return new WaitForSeconds(0.75f);
        
        animator.SetBool("Spin", false);
        GetComponent<AudioSource>().Stop();
    }

    public void StopTimer() {
        stopped = true;
    }

    public bool CanAttack() {
        return timer <= 0;
    }

    public void Freeze() {
        topSand.color = new Color(0, 1, 1, 1);
        bottomSand.color = new Color(0, 1, 1, 1);
        sandFalling.GetComponent<Image>().color = new Color(0, 1, 1, 1);

        frozen = true;
    }

    public void Unfreeze() {
        frozen = false;
    }

    public void Shocked(float num) {
        if(timer > 0) {
            maxTime = num;
            timer = maxTime;
        }
    }
}
