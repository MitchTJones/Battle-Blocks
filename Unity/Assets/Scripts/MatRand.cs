using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatRand : MonoBehaviour {
    public List<Sprite> sprites;
    public int index;
	void Start () {
        index = (int)(Random.value * (sprites.Count));
        GetComponent<SpriteRenderer>().sprite = sprites[index];
    }
}
