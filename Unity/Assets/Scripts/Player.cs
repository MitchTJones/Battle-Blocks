using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
public class Player : MonoBehaviour {

    public GameManager gameManager;

    public Sprite def;
    public Sprite near;
    public Sprite dead;

    float gravity = -18;
    float jumpVel = 11;
    float accTimeAirborne = .2f;
    float accTimeGrounded = .1f;
    float moveSpeed = 6;

    PlayerController controller;
    Vector3 velocity;
    float xVelSmoothing;

    Vector2 dirInput;

    public GameObject projectilePrefab;

    bool jump;

	void Start () {
        controller = GetComponent<PlayerController>();
	}

    public void SetDirInput(Vector2 input)
    {
        dirInput = input;
    }

    public void Jump()
    {
        if (controller.collisions.below)
            jump = true;
    }

    public void Shoot()
    {
        if (!Manager.Instance.gameManager.hasShot)
            Manager.Instance.gameManager.RecordShoot();
    }

	void Update () {
        CalcVel();
        controller.Move(velocity * Time.deltaTime);
    }

    void CalcVel()
    {
        if (controller.collisions.above || controller.collisions.below)
        {
            if (!controller.collisions.sliding)
            {
                velocity.y = 0;
            }
        }
        if (jump)
        {
            velocity.y = jumpVel;
            jump = false;
        }
        float targetXVel = dirInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetXVel, ref xVelSmoothing, (controller.collisions.below) ? accTimeGrounded : accTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
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
