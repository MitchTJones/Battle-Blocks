using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public Transform origin;
    public Canvas canvas;
    public GameObject cam;

    public int playerNo;
    public int enemyNo;
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject projectilePrefab;
    [Header("Local Player")]
    public GameObject player;
    public Player playerScript;
    public float playerFacing;
    public List<Vector2> localTurn;
    public Shot localShot = new Shot(new Vector2(0,0), new Vector2(0,0), 10000, false);
    [Header("Enemy Player")]
    public GameObject enemy;
    public Enemy enemyScript;
    public float enemyFacing;
    public List<Vector2> enemyTurn;
    public Shot enemyShot;
    [Header("UI")]
    public Text countdownTimer;
    public Text turnTimer;
    public Text playTimer;
    public Button retry;
    public Button endTurn;
    public Text p1score;
    public Text p2score;
    public GameObject turnBar;
    public GameObject turnBarTime;
    [Header("")]
    public int playLength = 120;
    public bool recording;
    public bool playerControl;
    public bool turnDone;
    public bool replaying;
    public bool hasShot;
    public bool shoot;
    [Header("Projectiles")]
    public GameObject recordProjectile;
    public GameObject replayProjectile;
    public GameObject enemyProjectile;
    [Header("Spawnpoints")]
    public Vector2 localSpawn;
    public Vector2 enemySpawn;

    public bool localKillSequence;
    public bool enemyKillSequence;

    public bool death;

    public float killSequenceSpeed = 1.0f;
    public float killSequenceStartTime;

    public bool zoomingOut;

    public Vector3 camOrigin = new Vector3(0,0,-2);
    public float zoomOrigin = 14.75f;

    private void Awake()
    {
        Manager.Instance.OnGameLoad(this);
        camOrigin = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
        zoomOrigin = Camera.main.orthographicSize;
    }
    private void FixedUpdate()
    {
        if (recording)
        {
            if (localTurn.Count > playLength - 2)
                playerControl = false;
            Vector2 pos = player.transform.position;
            if (localTurn.Count > 0)
            {
                bool moved = !(Equal(pos, localTurn[localTurn.Count - 1]));
                if (moved || shoot)
                {
                    if (moved)
                        localTurn.Add(player.transform.position);
                    if (shoot)
                    {
                        RecordFire();
                        shoot = false;
                    }
                } else
                {
                    if (turnDone)
                    {
                        recording = false;
                        TurnOver();
                    }
                }

            } else
            {
                localTurn.Add(player.transform.position);
            }
            int framesLeft = (playLength - localTurn.Count);
            float size = (float)framesLeft / (float)playLength;
            turnBar.transform.localScale = new Vector2(size, turnBar.transform.localScale.y);
            turnTimer.text = (framesLeft < 0) ? "0" : framesLeft.ToString();
            turnTimer.text = (playLength - localTurn.Count).ToString();
        } else if (replaying && !death)
        {
            if (localShot.shot)
            {
                if (localShot.f < 1)
                {
                    LocalShoot();
                    localShot.shot = false;
                }
                localShot.f--;
            }
            if (enemyShot.shot)
            {
                if (enemyShot.f < 1)
                {
                    EnemyShoot();
                    enemyShot.shot = false;
                }
                enemyShot.f--;
            }
            if (localTurn.Count > 0)
            {
                if (localTurn[0].x < player.transform.position.x)
                    playerScript.FlipFace(-1);
                else
                    playerScript.FlipFace(1);
                player.transform.position = localTurn[0];
                localTurn.RemoveAt(0);
            }
            if (enemyTurn.Count > 0)
            {
                if (enemyTurn[0].x < enemy.transform.position.x)
                    enemyScript.FlipFace(-1);
                else
                    enemyScript.FlipFace(1);
                enemy.transform.position = enemyTurn[0];
                enemyTurn.RemoveAt(0);
            }
            if (localTurn.Count < 1 && enemyTurn.Count < 1)
            {
                if (replayProjectile == null || replayProjectile.Equals(null))
                {
                    if (enemyProjectile == null || enemyProjectile.Equals(null))
                    {
                        localTurn = new List<Vector2>();
                        enemyTurn = new List<Vector2>();
                        localShot = new Shot(new Vector2(0, 0), new Vector2(0, 0), 10000, false);
                        enemyShot = new Shot(new Vector2(0, 0), new Vector2(0, 0), 10000, false);
                        replaying = false;
                        NetworkManager.Instance.RequestNextTurn();
                    }
                }
            }
        }
    }

    public void KillPlayer()
    {
        Log("KillPlayer");
        death = true;
        Time.timeScale = 1.0f;
        playerScript.SetMatDead();
        StartCoroutine(DeathAnim(player, 1.5f, "Player"));
    }
    public void KillEnemy()
    {
        Log("KillEnemy");
        death = true;
        Time.timeScale = 1.0f;
        enemyScript.SetMatDead();
        StartCoroutine(DeathAnim(enemy, 1.5f, "Enemy"));
    }
    public void PlayerHitSequence()
    {
        playerScript.SetMatNear();
        localKillSequence = true;
        if (enemyKillSequence)
        {
            EndHitSequence();
        }
        StartCoroutine(Zoom(player.transform.position, 1.0f, 0.15f, 0.1f));
    }
    public void EnemyHitSequence()
    {
        enemyScript.SetMatNear();
        enemyKillSequence = true;
        if (localKillSequence)
        {
            EndHitSequence();
        }
        StartCoroutine(Zoom(enemy.transform.position, 1.0f, 0.075f, 0.025f));
    }
    public void EndHitSequence()
    {
        playerScript.SetMatDef();
        enemyScript.SetMatDef();
        localKillSequence = false;
        enemyKillSequence = false;
        StopAllCoroutines();
        StartCoroutine(Zoom(camOrigin, zoomOrigin, 1.0f, 0.1f));
    }

    private IEnumerator Zoom(Vector3 end, float zoomEnd, float ts, float time)
    {
        Time.timeScale = ts;
        float elapsedTime = 0;
        end.z = -2;
        Vector3 start = Camera.main.transform.position;
        float zoomStart = Camera.main.orthographicSize;

        while (elapsedTime < time)
        {
            Camera.main.transform.position = Vector3.Lerp(start, end, (elapsedTime / time));
            Camera.main.orthographicSize = Mathf.Lerp(zoomStart, zoomEnd, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator DeathAnim(GameObject target, float time, string p)
    {
        float elapsedTime = 0;
        playerScript.GetComponent<Player>().enabled = false;
        Vector3 start = target.transform.localScale;
        while (elapsedTime < time)
        {
            target.transform.localScale = Vector3.Lerp(start, new Vector3(0, 0, 0), (elapsedTime / time));
            //target.transform.Rotate(Vector3.right * (elapsedTime/time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(Zoom(camOrigin, zoomOrigin, 1.0f, 0.1f));
        if (p == "Enemy")
            NetworkManager.Instance.EnemyHit();
        if (p == "Player")
            NetworkManager.Instance.PlayerHit();
        playerScript.GetComponent<Player>().enabled = true;
        ResetRound();
    }

    public void StartGame()
    {
        turnTimer.text = playLength.ToString();
        StartTurn();
    }

    public void StartTurn()
    {
        replaying = false;
        playerControl = true;
        playerScript.GetComponent<Player>().enabled = true;
        recording = true;
        endTurn.gameObject.SetActive(true);
        retry.gameObject.SetActive(true);
    }

    public void SetScore(float p1, float p2)
    {
        p1score.text = p1.ToString();
        p2score.text = p2.ToString();
    }

    public void OnClick_ReRecord()
    {
        Destroy(recordProjectile);
        localShot = new Shot(new Vector2(0, 0), new Vector2(0, 0), 10000, false);
        hasShot = false;
        player.transform.position = localTurn[0];
        localTurn = new List<Vector2>();
        recording = true;
    }

    public void OnClick_EndTurn()
    {
        playerControl = false;
        turnDone = true;
        endTurn.gameObject.SetActive(false);
        retry.gameObject.SetActive(false);
    }

    public void TurnOver()
    {
        CancelTurnCountdown();
        NetworkManager.Instance.EndTurn(localTurn, localShot);
        ResetTurn();
    }

    public void ResetTurn()
    {
        replaying = false;
        playerControl = false;
        recording = false;
        turnDone = false;
        shoot = false;
        hasShot = false;
    }

    public void EnemyTurn(List<Vector2> turn, Shot shot)
    {
        Destroy(recordProjectile);
        replaying = true;
        playerScript.GetComponent<Player>().enabled = false;
        enemyTurn = turn;
        enemyShot = shot;
    }

    public void CreatePlayer()
    {
        player = Instantiate(playerPrefab, origin);
        playerScript = player.GetComponent<Player>();
    }

    public void CreateEnemy()
    {
        enemy = Instantiate(enemyPrefab, origin);
        enemyScript = enemy.GetComponent<Enemy>();
    }
    
    public void SetSpawnpoints(Vector2[] points)
    {
        Debug.Log(NetworkManager.Instance.playerNo);
        Debug.Log(NetworkManager.Instance.enemyNo);
        localSpawn = points[NetworkManager.Instance.playerNo-1];
        enemySpawn = points[NetworkManager.Instance.enemyNo-1 ];
    }

    public void ResetRound()
    {
        localTurn = new List<Vector2>();
        enemyTurn = new List<Vector2>();
        playerScript.SetMatDef();
        enemyScript.SetMatDef();
        localKillSequence = false;
        enemyKillSequence = false;
        death = false;
        player.transform.localScale = new Vector3(1, 1, 1);
        enemy.transform.localScale = new Vector3(1, 1, 1);
        MoveLocalPlayer(localSpawn);
        MoveEnemyPlayer(enemySpawn);
        if (localSpawn.x < 0)
            playerScript.FlipFace(1);
        else
            playerScript.FlipFace(-1);
        if (enemySpawn.x < 0)
            enemyScript.FlipFace(1);
        else
            enemyScript.FlipFace(-1);
        Time.timeScale = 1.0f;
    }

    public void MoveLocalPlayer(Vector2 newpos)
    {
        player.transform.position = newpos;
    }

    public void MoveEnemyPlayer(Vector2 newpos)
    {
        enemy.transform.position = newpos;
    }

    public void LocalShoot()
    {
        replayProjectile = ReplayFire(localShot, "Player");
    }

    public void EnemyShoot()
    {
        enemyProjectile = ReplayFire(enemyShot, "Enemy");
    }

    public void RecordShoot()
    {
        shoot = true;
    }

    public void RecordFire()
    {
        Shot shot = new Shot(new Vector2(player.GetComponent<PlayerController>().facing * 10, 0), player.transform.position, localTurn.Count - 1, true);
        GameObject projectile;
        projectile = Instantiate(projectilePrefab);
        Projectile p = projectile.GetComponent<Projectile>();
        p.SetPos(shot.o);
        p.SetSpeed(shot.s);
        p.SetSource("Player");
        hasShot = true;
        recordProjectile = projectile;
        localShot = shot;
    }

    public GameObject ReplayFire(Shot shot, string source)
    {
        GameObject projectile = Instantiate(projectilePrefab);
        Projectile p = projectile.GetComponent<Projectile>();
        p.SetPos(shot.o);
        p.SetSpeed(shot.s);
        p.SetSource(source);
        return projectile;
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

    public void TurnCountdown(float time)
    {
        float size = time / 30;
        turnBarTime.transform.localScale = new Vector2(size, turnBarTime.transform.localScale.y);
        playTimer.text = time.ToString();
    }

    public void CancelTurnCountdown()
    {
        playTimer.text = "";
    }

    public void Log(object message)
    {
        Debug.Log("[Manager] " + message);
    }

    public bool Equal(Vector2 v1, Vector2 v2)
    {
        if (diff(v1.x, v2.x) < 0.025)
            if (diff(v1.y, v2.y) < 0.025)
                return true;
        return false;
    }

    public float diff(float n1, float n2)
    {
        return Mathf.Abs(n1 - n2);
    }
}