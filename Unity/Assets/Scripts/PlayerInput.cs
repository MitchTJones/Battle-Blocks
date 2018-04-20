using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {
    Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        player.SetDirInput((Manager.Instance.gameManager.playerControl) ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) : new Vector2(0, 0));

        if (!Manager.Instance.gameManager.playerControl)
            return;

        if (Input.GetKeyDown(KeyCode.W))
            player.Jump();
        if (Input.GetKeyDown(KeyCode.Space))
            player.Shoot();
    }
}
