using System.Collections.Generic;
using System.Data.SqlTypes;
using PurrNet;
using PurrNet.Modules;
using Steamworks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    private TextMeshProUGUI playerText;
    private LobbyManager lobbyManager;
    private int sq = 10;
    public NetworkTransform cubePrefab;
    public GameObject playerPrefab;
    private List<TileObject> mapList = new List<TileObject>();
    private GameObject playerObject;
    private Vector3 playerSpawnPos;
    public NetworkTransform playerHeldObjectRef;
    public bool hasLoadedMap = false;

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

            for (int x = 0; x < sq; x++)
            {

                for(int y = 0; y < sq; y++)
                {

                    GameObject tile = UnityProxy.InstantiateDirectly(cubePrefab.gameObject, new Vector3(x * 2 - sq/2 - 1, -2, y * 2 - sq/2 - 1), Quaternion.identity);

                    mapList.Add(new TileObject(tile, 0));

                }
                    
            }

        }
        else
        {

            PlayerController localPlayerScript = playerObject.GetComponent<PlayerController>();

            foreach(TileObject tile in mapList) {
                NetworkTransform newTile = Instantiate(cubePrefab, tile.GetObject().transform.position, Quaternion.identity);

                if(localPlayerScript.holdObj && localPlayerScript.holdObj.gameObject == tile.GetObject()) {
                    playerHeldObjectRef = newTile;
                    tile.isHeld = false; // jank
                }

                Destroy(tile.GetObject());
                tile.SetObject(newTile.gameObject);
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
        
        foreach(TileObject tile in mapList) {

            Destroy(tile.GetObject());

        }

    }

    public void LoadLocalGame()
    {
        
        // this should call on startup and game leave
        // needs a json save file of the world?
        mapList = new List<TileObject>();

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

    public bool IsTileHeld(GameObject searchTile)
    {
        
        foreach(TileObject tile in mapList)
        {
            
            if(tile.GetObject() == searchTile) return tile.isHeld;

        }

        Debug.Log("Could not find tile");

        return false;

    }

    public void SetTileHeld(GameObject searchTile, bool option)
    {
        
        foreach(TileObject tile in mapList)
        {
            
            if(tile.GetObject() == searchTile) tile.isHeld = option;

        }

    }

}
