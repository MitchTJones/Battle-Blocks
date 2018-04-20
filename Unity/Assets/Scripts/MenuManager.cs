using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public Canvas canvas;
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField ipAddressInput;
    public GameObject serverMenu;
    public GameObject mainMenu;
    public GameObject settingsMenu;

    private void Awake()
    {
        Manager.Instance.OnMenuLoad(this);
    }
    public void OnIPChange()
    {
        NetworkManager.Instance.socket.url = "ws://" + ipAddressInput.text + "/socket.io/?EIO=4&transport=websocket";
    }
    public void OnUsernameChange()
    {
        NetworkManager.Instance.username = usernameInput.text;
    }
    public void OnClick_JoinServer()
    {
        Manager.Instance.menuManager.GetComponent<AudioSource>().Play();
        NetworkManager.Instance.socket.createSocket();
        NetworkManager.Instance.Connect();
    }
    public void OnClick_ServerMenu()
    {
        serverMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
    public void OnClick_SettingsMenu()
    {
        Manager.Instance.MenuOn();
        Manager.Instance.menu.GetComponent<Menu>().OnClick_Settings();
    }
    public void OnClick_ExitGame()
    {
        Application.Quit();
    }
    public void OnClick_BackFromServerMenu()
    {
        Manager.Instance.menuManager.GetComponent<AudioSource>().Play();
        Back();
    }
    public void Back()
    {
        if (serverMenu.activeSelf)
        {
            serverMenu.SetActive(false);
            mainMenu.SetActive(true);
        } else if (Manager.Instance.menuScript.settingsMenu.activeSelf)
        {
            Manager.Instance.MenuBack();
        }
    }
}
