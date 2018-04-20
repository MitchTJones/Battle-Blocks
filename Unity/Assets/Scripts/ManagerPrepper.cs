using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPrepper : MonoBehaviour
{
    public MenuManager menuManager;
    public Canvas canvas;
    public GameObject menuPrefab;
    public AudioClip audioClip;

    public NetworkManager nmCheck;
    public Manager mCheck;

    void Start()
    {
        nmCheck = NetworkManager.Instance;
        mCheck = Manager.Instance;
        Manager.Instance.canvas = canvas;
        Manager.Instance.menuPrefab = menuPrefab;
        Manager.Instance.musicClip = audioClip;
    }

    void Update()
    {

    }
}
