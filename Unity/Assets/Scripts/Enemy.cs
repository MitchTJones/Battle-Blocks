using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    public Sprite def;
    public Sprite near;
    public Sprite dead;
    public void FlipFace(float facing)
    {
        Vector3 currScale = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(facing, currScale.y, currScale.z);
    }
    public void SetMatDef()
    {
        GetComponent<SpriteRenderer>().sprite = def;
    }
    public void SetMatNear()
    {
        GetComponent<SpriteRenderer>().sprite = near;
    }
    public void SetMatDead()
    {
        GetComponent<SpriteRenderer>().sprite = dead;
    }
}
