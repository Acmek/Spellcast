using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostumeMenuCode : MonoBehaviour
{
    [SerializeField] private PlayerSave save;
    private SpriteRenderer spriteRen;
    private PolygonCollider2D colliderCom;

    // Start is called before the first frame update
    void Start()
    {
        spriteRen = GetComponent<SpriteRenderer>();
        colliderCom = GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(spriteRen.sprite != save.GetCostumeMenu()) {
            spriteRen.sprite = save.GetCostumeMenu();
            UpdateCollider(spriteRen.sprite);
        }
    }
 
    void UpdateCollider(Sprite sprite) { 
        // update count
        colliderCom.pathCount = sprite.GetPhysicsShapeCount();
                
        // new paths variable
        List<Vector2> path = new List<Vector2>();

        // loop path count
        for(int i = 0; i < colliderCom.pathCount; i++) {
            // get shape
            sprite.GetPhysicsShape(i, path);
            // set path
            colliderCom.SetPath(i, path.ToArray());
        }
    }
}
