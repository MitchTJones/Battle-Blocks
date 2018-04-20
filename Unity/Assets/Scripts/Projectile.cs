using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    private Vector2 speed = new Vector2(0,0);
    private string source;
    private void Update()
    {
        transform.Translate(speed * Time.deltaTime);
    }

    public void SetSpeed(Vector2 input)
    {
        speed = input;
    }
    public void SetPos(Vector3 input)
    {
        transform.position = input;
    }
    public void SetSource(string input)
    {
        source = input;
    }
    private void OnTriggerEnter2D(Collider2D coll)
    {
        string target = coll.gameObject.tag;
        if (target == "Obstacle")
        {
            Destroy(gameObject);
        } else if (Manager.Instance.gameManager.replaying)
        {
            if (target.Contains("-")) {
                string primary = target.Split('-')[0];
                string secondary = target.Split('-')[1];
                if (primary != source)
                {
                    if (secondary == "InnerColl")
                    {
                        if (target == "Enemy-InnerColl")
                            Manager.Instance.gameManager.KillEnemy();
                        if (target == "Player-InnerColl")
                            Manager.Instance.gameManager.KillPlayer();
                        Destroy(gameObject);
                    }
                    if (target == "Enemy-OuterColl")
                        Manager.Instance.gameManager.EnemyHitSequence();
                    if (target == "Player-OuterColl")
                        Manager.Instance.gameManager.PlayerHitSequence();
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D coll)
    {
        string target = coll.gameObject.tag;
        if (Manager.Instance.gameManager.replaying)
        {
            if (target.Contains("-OuterColl"))
            {
                string primary = target.Split('-')[0];
                if (primary != source)
                {
                    Manager.Instance.gameManager.EndHitSequence();
                }
            }
        }
    }
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
