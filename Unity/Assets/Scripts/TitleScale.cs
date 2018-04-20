using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScale : MonoBehaviour {
    Text text;
    public float min = 0.4f;
    public float max = 0.405f;
    static float t = 0.0f;
	void Start () {
        text = gameObject.GetComponent<Text>();
        min = transform.localScale.x;
        max = min + 0.02f;
	}
	void Update () {
        float l = Mathf.Lerp(min, max, t);
        transform.localScale = new Vector2(l, l);

        t += 0.5f * Time.deltaTime;

        if (t > 1.0f)
        {
            float temp = max;
            max = min;
            min = temp;
            t = 0.0f;
        }
    }
}
