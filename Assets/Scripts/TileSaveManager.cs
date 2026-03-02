using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using PurrNet;

public class TileSaveManager : NetworkBehaviour
{
    private string filePath;
    private bool wasServer;

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "tiles.json");
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        wasServer = isServer;
    }

    private void OnApplicationQuit()
    {

        if(!networkManager || wasServer) SaveTiles();

    }

    public void SaveTiles()
    {
        TileScript[] sceneTiles = FindObjectsByType<TileScript>(FindObjectsSortMode.None);

        TileDataList dataList = new TileDataList();

        foreach (var tile in sceneTiles)
        {
            TileData data = new TileData
            {
                id = tile.GetID(),
                position = GridUtil.SnapToGrid(tile.transform.position),
            };

            dataList.tiles.Add(data);
        }

        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Tiles saved to: " + filePath);
    }

    public List<TileData> LoadTiles()
    {
        if (!File.Exists(filePath)) return null;

        string json = File.ReadAllText(filePath);
        TileDataList dataList = JsonUtility.FromJson<TileDataList>(json);

        return dataList.tiles;
    }
}