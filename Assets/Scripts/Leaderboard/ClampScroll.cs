using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClampScroll : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar;
    private int max;

    void Update() {
        if(transform.localPosition.y < 0)
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);

        if(transform.GetChild(2).childCount - 1 > 0) {
            max = 28;
            max += (transform.GetChild(2).childCount - 2) * 30;
        }

        if(transform.localPosition.y > max)
            transform.localPosition = new Vector3(transform.localPosition.x, max, transform.localPosition.z);
        
        scrollbar.size = 1 - ((transform.GetChild(2).childCount - 2) * 0.075f);
    }

    public void ValueChanged() {
        transform.localPosition = new Vector3(transform.localPosition.x, scrollbar.value * max, transform.localPosition.z);
    }

    public void Scrolling() {
        scrollbar.value = transform.localPosition.y / max;
    }
}
