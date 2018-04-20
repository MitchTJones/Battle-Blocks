using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour {
    private void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Manager.Instance.ToggleMenu();
        }
    }
}
