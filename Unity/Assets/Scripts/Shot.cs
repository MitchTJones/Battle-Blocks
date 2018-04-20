using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Shot {
    public bool shot = false;
    public char t; //type
    public Vector2 s; //speed
    public Vector2 o; //origin
    public int f; //frame
    public Shot(Vector2 _s, Vector2 _o, int _f, bool _shot)
    {
        s = _s;
        o = _o;
        f = _f;
        shot = _shot;
    }
}
