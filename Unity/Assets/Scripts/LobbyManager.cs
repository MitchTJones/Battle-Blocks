using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour {
    public Canvas canvas;

    public GameObject localPlayerPanel;
    public GameObject enemyPlayerPanel;

    public Text countdownTimer;

    public bool localReady = false;
    public bool enemyReady = false;

    private void Start()
    {
        Manager.Instance.OnLobbyLoad(this);
        localPlayerPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = NetworkManager.Instance.username;
        localPlayerPanel.SetActive(true);
    }

    public void OnClick_ReadyUp()
    {
        localReady = true;
        localPlayerPanel.transform.GetChild(1).gameObject.SetActive(false);
        localPlayerPanel.transform.GetChild(2).gameObject.SetActive(true);
        NetworkManager.Instance.ReadyUp();
    }

    public void OnClick_Unready()
    {
        localReady = false;
        localPlayerPanel.transform.GetChild(1).gameObject.SetActive(true);
        localPlayerPanel.transform.GetChild(2).gameObject.SetActive(false);
        NetworkManager.Instance.Unready();
    }

    public void OnClick_Disconnect()
    {
        Manager.Instance.ReturnToMenu();
    }

    public void Countdown(float time)
    {
        countdownTimer.text = time.ToString();
    }

    public void CancelCountdown()
    {
        countdownTimer.text = "";
    }

    public void EnemyReady()
    {
        enemyReady = true;
        enemyPlayerPanel.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Ready";
    }

    public void EnemyUnready()
    {
        enemyReady = false;
        enemyPlayerPanel.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Not Ready";
    }

    public void AddEnemy(string username)
    {
        enemyPlayerPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = username;
        enemyPlayerPanel.SetActive(true);
    }

    public void RemoveEnemy()
    {
        enemyPlayerPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
        enemyPlayerPanel.SetActive(false);
    }
}
