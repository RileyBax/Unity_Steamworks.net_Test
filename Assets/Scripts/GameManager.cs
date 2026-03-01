using System;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    private TextMeshProUGUI playerText;
    private LobbyManager lobbyManager;
    private int sq = 10;
    public NetworkTransform cubePrefab;
    public GameObject playerPrefab;
    private List<TileScript> mapList = new List<TileScript>();
    private GameObject playerObject;
    private Vector3 playerSpawnPos;
    public NetworkTransform playerHeldObjectRef;
    public bool hasLoadedMap = false;
    public TileSaveManager tileSaveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        LoadLocalGame();
        hasLoadedMap = false;

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        UpdatePlayerCount();



        // i dont like this
        if (!isServer)
        {
            
            DestroyFarm();

        }

    }

    [ObserversRpc(runLocally:true)]
    public void UpdatePlayerCount()
    {

        playerText.text = lobbyManager.GetPlayerCount().ToString();

    }

    public void SetPlayerText(string t)
    {
        
        playerText.text= t;

    }

    public void DrawFarm()
    {
    

        if(mapList.Count == 0){

            List<TileData> tileDataList = tileSaveManager.LoadTiles();

            if(tileDataList != null){

                foreach(TileData tileData in tileDataList)
                {
                    
                    GameObject tile = UnityProxy.InstantiateDirectly(cubePrefab.gameObject, tileData.position, Quaternion.identity);
                    TileScript tileScript = tile.GetComponent<TileScript>();
                    tileScript.SetTileData(0);
                    mapList.Add(tileScript);

                }

            }
            else
            {

                for (int x = -sq/2; x < sq-sq/2; x++)
                {

                    for (int z = -sq/2; z < sq-sq/2; z++)
                    {
                        
                        GameObject tile = UnityProxy.InstantiateDirectly(cubePrefab.gameObject, GridUtil.SnapToGrid(new Vector3(x*2, -2, z*2)), Quaternion.identity);
                        TileScript tileScript = tile.GetComponent<TileScript>();
                        tileScript.SetTileData(0);
                        mapList.Add(tileScript);

                    }

                }

            }

        }
        else
        {

            PlayerController localPlayerScript = playerObject.GetComponent<PlayerController>();

            for (int i = 0; i < mapList.Count; i++) {
                NetworkTransform newTile = Instantiate(cubePrefab, mapList[i].gameObject.transform.position, Quaternion.identity);

                if(localPlayerScript.holdObj && localPlayerScript.holdObj.gameObject == mapList[i].gameObject) {
                    playerHeldObjectRef = newTile;
                    mapList[i].isHeld = false; // jank
                }

                Destroy(mapList[i].gameObject);
                mapList[i] = newTile.GetComponent<TileScript>();
            }

            hasLoadedMap = true;
            Debug.Log("Map Instantiated");

        }

    }

    public Vector3 GetPlayerSpawnPos()
    {
        return playerSpawnPos;
    }

    private void DestroyFarm()
    {
        
        foreach(TileScript tile in mapList) {

            Destroy(tile.gameObject);

        }

    }

    public void LoadLocalGame()
    {
        
        // this should call on startup and game leave
        // needs a json save file of the world?
        mapList = new List<TileScript>();

        playerText = GameObject.Find("Players").GetComponent<TextMeshProUGUI>();

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();

        playerObject = UnityProxy.InstantiateDirectly(playerPrefab);
        playerObject.name = "LocalPlayer";

        

        DrawFarm();

    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
        // add a uiscript with a flip button vis
        hasLoadedMap = false;
        LoadLocalGame();
    }

}
