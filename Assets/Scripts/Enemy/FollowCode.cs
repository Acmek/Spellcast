using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCode : MonoBehaviour
{
    [SerializeField] private Transform spriteObject;
    private float offset;

    void Start() {
        offset = transform.position.x - spriteObject.position.x;
    }

    void Update()
    {
        transform.position = new Vector3(spriteObject.position.x + offset, transform.position.y, transform.position.z);
    }
}
