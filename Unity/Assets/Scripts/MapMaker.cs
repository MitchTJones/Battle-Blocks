using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour {
    public List<TextAsset> maps;
    public int map = 0;
    public GameObject grassPrefab;
    public GameObject dirtPrefab;
    public GameObject stonePrefab;
    public GameObject tiltedDirtPrefab;
    public GameObject tiltedStonePrefab;
    public List<Vector2> spawnPoints;

    public int colNo = 51;
    public int rowNo = 28;

    void Start()
    {
        string[] rows = maps[map].text.Split('\n');
        if (rows.Length != rowNo)
        {
            Debug.LogError("Map Corrupted!");
            return;
        }
        for (int r = 0; r < rowNo; r++)
        {
            string[] row = rows[r].Split(' ');
            for (int c = 0; c < colNo; c++)
            {
                string item = row[c];
                Vector2 coords = new Vector2(c - (colNo / 2), (rowNo-r) - (rowNo / 2));
                switch (item)
                {
                    case "1":
                        PlaceTile(grassPrefab, coords);
                        break;
                    case "2":
                        PlaceTile(dirtPrefab, coords);
                        break;
                    case "3":
                        PlaceTile(stonePrefab, coords);
                        break;
                    case ">":
                        PlaceTiltedTile(tiltedDirtPrefab, coords, 1,1);
                        break;
                    case "<":
                        PlaceTiltedTile(tiltedDirtPrefab, coords, -1,1);
                        break;
                    case "[":
                        PlaceTiltedTile(tiltedStonePrefab, coords, -1, -1);
                        break;
                    case "]":
                        PlaceTiltedTile(tiltedStonePrefab, coords, 1, -1);
                        break;
                    case "*":
                        spawnPoints.Add(coords);
                        break;
                }
            }
        }
        if (spawnPoints.Count == 2)
        {
            Manager.Instance.gameManager.SetSpawnpoints(spawnPoints.ToArray());
        }
    }

    public GameObject PlaceTile(GameObject prefab, Vector2 loc)
    {
        GameObject tile = Instantiate(prefab, gameObject.transform);
        tile.transform.position = loc;
        return tile;
    }

    public void PlaceTiltedTile(GameObject prefab, Vector2 loc, float xS, float yS)
    {
        GameObject tile = PlaceTile(prefab, loc);
        tile.transform.localScale = new Vector3(xS, yS, tile.transform.localScale.z);
    }
}
