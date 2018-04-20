using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(MenuHandler))]
[RequireComponent(typeof(AudioSource))]
public class Manager : Singleton<Manager> {

    protected Manager() { }

    public string currentScene = "Menu";
    private ManagerPrepper managerPrepper;

    [Header("Managers")]
    public MenuManager menuManager;
    public LobbyManager lobbyManager;
    public GameManager gameManager;

    [Header("Music")]
    public AudioSource aus;
    public AudioClip musicClip;

    [Header("UI")]
    public Canvas canvas;
    public GameObject errorPrefab;
    public GameObject errorBox;
    public Text errorText;
    public string errorMessage = null;
    GameObject err;
    [Header("Menu")]
    public GameObject menuPrefab;
    public GameObject menu;
    public Menu menuScript;


    public void Awake()
    {
        //if (Manager.Instance != this)
        //{
        //    Destroy(gameObject);
        //}
        aus = GetComponent<AudioSource>();
        aus.clip = musicClip;
        DontDestroyOnLoad(gameObject);
        aus.playOnAwake = true;
        aus.loop = true;
        MenuOn();
        MenuOff();
    }

    private void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (currentScene == "Menu")
        {
            menuManager.Back();
        }
        else
        {
            if ((menu == null) ? true : !menu.activeSelf)
            {
                MenuOn();
            }
            else
            {
                MenuBack();
            }
        }
        //errorBox = Instantiate(errorPrefab);
        //errorBox.transform.SetParent(canvas.transform, false);
    }

    public void MenuBack()
    {
        if (currentScene == "Menu")
        {
            MenuOff();
            menuScript.settingsMenu.SetActive(false);
            menuScript.escMenu.SetActive(true);
        } else {
            if (!menuScript.escMenu.activeSelf && menuScript.settingsMenu.activeSelf)
            {
                menuScript.settingsMenu.SetActive(false);
                menuScript.escMenu.SetActive(true);
            }
            else
            {
                MenuOff();
            }
        }
    }

    public void MenuOn()
    {
        if (menu == null)
        {
            menu = Instantiate(menuPrefab);
            menu.transform.SetParent(canvas.transform, false);
            menuScript = menu.GetComponent<Menu>();
        }
        menu.SetActive(true);
        menuScript.escMenu.SetActive(true);
        menuScript.settingsMenu.SetActive(false);
    }

    public void MenuOff()
    {
        if (menu != null)
        {
            menu.SetActive(false);
        }
    }

    public void LoadScene(string scene)
    {
        //Debug.LogError("Loading New Scene: " + scene);
        currentScene = scene;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void OnMenuLoad(MenuManager manager)
    {
        menuManager = manager;
        canvas = menuManager.canvas;
    }

    public void OnLobbyLoad(LobbyManager manager)
    {
        Log("Scene Loaded: Lobby");
        lobbyManager = manager;
        canvas = lobbyManager.canvas;
        // NetworkManager.Instance.SignIn();
    }

    public void OnGameLoad(GameManager manager)
    {
        //Log("Scene Loaded: *Game Scene*");
        gameManager = manager;
        canvas = gameManager.canvas;
        gameManager.CreatePlayer();
        gameManager.CreateEnemy();
        NetworkManager.Instance.GameLoaded();
    }

    public void ReturnToMenu()
    {
        NetworkManager.Instance.Disconnect();
        LoadScene("Menu");
    }

    public void Error()
    {
        if (!errorMessage.Equals(null))
        {
            errorBox = Instantiate(errorPrefab);
            errorBox.transform.SetParent(canvas.transform, false);
            errorText = errorBox.transform.GetChild(0).GetComponent<Text>();
            errorText.text = errorMessage;
        }
    }

    public void ErrorMessage(string message)
    {
        errorMessage = message;
    }

    public void ErrorOK()
    {
        errorMessage = null;
        Destroy(errorBox);
    }

    public void Log(string message)
    {
        Debug.Log("[Manager] " + message);
    }
}
