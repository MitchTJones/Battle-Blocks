using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SocketIOComponent))]
public class NetworkManager : Singleton<NetworkManager> {

    protected NetworkManager() { } // guarantee this will be always a singleton only - can't use the constructor!

    public static NetworkManager instance;
    public SocketIOComponent socket;

    public string username;
    public int playerNo;
    public int enemyNo;

    public bool signedIn;

    private void Awake()
    {
        socket = GetComponent<SocketIOComponent>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        socket.On("verification", (SocketIOEvent e) => {
            //Log("Verified Connection to Server");
            Manager.Instance.LoadScene("Lobby");
            SignIn();
        });
        socket.On("signin-response", (SocketIOEvent e) => {
            //Log("Successfully Signed In");
            Manager.Instance.gameManager.CreatePlayer();
            signedIn = true;
        });
        socket.On("addplayer", (SocketIOEvent e) =>
        {
            //Log("Enemy Player has Joined the Server");
            Manager.Instance.lobbyManager.AddEnemy(e.data["name"].str);
        });
        socket.On("removeplayer", (SocketIOEvent e) =>
        {
            Manager.Instance.lobbyManager.RemoveEnemy();
        });
        socket.On("enemyready", (SocketIOEvent e) =>
        {
            Manager.Instance.lobbyManager.EnemyReady();
        });
        socket.On("enemyunready", (SocketIOEvent e) =>
        {
            Manager.Instance.lobbyManager.EnemyUnready();
        });
        socket.On("countdown", (SocketIOEvent e) =>
        {
            string name = e.data["name"].str;
            float time = e.data["time"].n;
            if (name == "lobby")
                Manager.Instance.lobbyManager.Countdown(time);
            if (name == "gamestart")
                Manager.Instance.gameManager.Countdown(time);
            if (name == "turn")
                Manager.Instance.gameManager.TurnCountdown(time);
        });
        socket.On("cancelcountdown", (SocketIOEvent e) =>
        {
            string name = e.data["n"].str;
            Log(name);
            if (name == "lobby")
                Manager.Instance.lobbyManager.CancelCountdown();
            if (name == "gamestart")
                Manager.Instance.gameManager.CancelCountdown();
        });
        socket.On("turnover", (SocketIOEvent e) =>
        {
            Manager.Instance.gameManager.TurnOver();
        });
        socket.On("loadgame", (SocketIOEvent e) =>
        {
            Manager.Instance.LoadScene("Scene1");
        });
        socket.On("startturn", (SocketIOEvent e) =>
        {
            //Log("Starting Turn");
            Manager.Instance.gameManager.CancelCountdown();
            Manager.Instance.gameManager.StartGame();
        });
        socket.On("enemyturn", (SocketIOEvent e) =>
        {
            //Log("Received Enemy Turn");
            List<Vector2> enemyTurn = new List<Vector2>();
            foreach (JSONObject vector in e.data["turn"]["turn"].list)
            {
                enemyTurn.Add(JSONTemplates.ToVector2(vector));
            }
            Shot enemyShot = JSONtoShot(e.data["turn"]["shot"]);
            Manager.Instance.gameManager.EnemyTurn(enemyTurn, enemyShot);
        });
        socket.On("player", (SocketIOEvent e) =>
        {
            playerNo = Convert.ToInt32(e.data["p"].n);
            enemyNo = Convert.ToInt32(e.data["e"].n);
        });
        socket.On("newround", (SocketIOEvent e) =>
        {
            Manager.Instance.gameManager.ResetRound();
        });
        socket.On("setscore", (SocketIOEvent e) =>
        {
            Manager.Instance.gameManager.SetScore(e.data["p1"].n, e.data["p2"].n);
        });
        socket.On("endturn", (SocketIOEvent e) =>
        {
            Manager.Instance.gameManager.OnClick_EndTurn();
        });
        socket.On("endgame", (SocketIOEvent e) =>
        {
            Debug.Log("Ending game: " + signedIn);
            if (Manager.Instance.currentScene == "Scene1")
            {
                Manager.Instance.LoadScene("Lobby");
                Manager.Instance.ErrorMessage(e.data["message"].str);
            }
        });
        socket.On("", (SocketIOEvent e) =>
        {

        });
    }
    public void Connect()
    {
        //Log("Connecting to Server");
        socket.Connect();
    }

    public void Disconnect()
    {
        Log("Disconnecting from Server");
        signedIn = false;
        socket.Close();
    }

    public void SignIn()
    {
        Debug.Log("Signing In: " + signedIn);
        if (!signedIn)
        {
            if (username == "")
                username = "Guest";
            socket.Emit("signin", JSONObject.CreateStringObject(username));
        }
    }

    public void ReadyUp()
    {
        socket.Emit("ready");
    }

    public void Unready()
    {
        socket.Emit("unready");
    }

    public void GameLoaded()
    {
        socket.Emit("gameloaded");
    }

    public void EnemyHit()
    {
        socket.Emit("enemyhit");
    }

    public void PlayerHit()
    {
        socket.Emit("localhit");
    }

    public void EndTurn(List<Vector2> turn, Shot shot)
    {
        TurnJSON turnJSON = new TurnJSON(turn, shot);
        socket.Emit("turn", turnJSON.ToJson());
    }

    public void RequestNextTurn()
    {
        socket.Emit("requestnextturn");
    }

    //public JSONObject Jsonify(JSON input)
    //{
    //    return new JSONObject(JsonUtility.ToJson(input));
    //}

    public Shot JSONtoShot(JSONObject j)
    {
        return new Shot(JSONTemplates.ToVector2(j["s"]), JSONTemplates.ToVector2(j["o"]), Convert.ToInt32(j["f"].n), (JSONTemplates.ToVector2(j["s"]).x != 0)); //shot boolean conversion is a quickfix
    }

    public class JSON
    {
        public JSONObject ToJson()
        {
            return new JSONObject(JsonUtility.ToJson(this));
        }
    }

    [Serializable]
    public class TurnJSON : JSON
    {
        public Vector2[] turn;
        public Shot shot;
        public TurnJSON(List<Vector2> t, Shot _shot)
        {
            turn = t.ToArray();
            shot = _shot;
        }
    }

    public Dictionary<string, object> emitObj(string name, object obj)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data[name] = obj;
        return data;
    }

    public void Log(object message)
    {
        Debug.Log("[NetworkManager] " + message);
    }
}
