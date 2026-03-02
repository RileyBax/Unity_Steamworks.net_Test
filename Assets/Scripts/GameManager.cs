using System;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    private TextMeshProUGUI playerText;
    private LobbyManager lobbyManager;
    private int sq = 10;
    public NetworkTransform cubePrefab;
    public NetworkTransform itemPrefab;
    public GameObject playerPrefab;
    private GameObject playerObject;
    private Vector3 playerSpawnPos;
    public NetworkTransform playerHeldObjectRef;
    public bool hasLoadedOfflineMap = false;
    public bool hasLoadedMap = false;
    public TileSaveManager tileSaveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        LoadLocalGame();
        hasLoadedMap = false;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {

            if (networkManager) Instantiate(itemPrefab);
            else UnityProxy.InstantiateDirectly(itemPrefab);

        }

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

    [ObserversRpc(runLocally: true)]
    public void UpdatePlayerCount()
    {

        playerText.text = lobbyManager.GetPlayerCount().ToString();

    }

    public void SetPlayerText(string t)
    {

        playerText.text = t;

    }

    public void DrawFarm()
    {


        if (!hasLoadedOfflineMap)
        {

            List<TileData> tileDataList = tileSaveManager.LoadTiles();

            if (tileDataList != null)
            {

                foreach (TileData tileData in tileDataList)
                {

                    GameObject tile = UnityProxy.InstantiateDirectly(cubePrefab.gameObject, tileData.position, Quaternion.identity);
                    TileScript tileScript = tile.GetComponent<TileScript>();
                    tileScript.SetTileData(0);

                }

            }
            else
            {

                for (int x = -sq / 2; x < sq - sq / 2; x++)
                {

                    for (int z = -sq / 2; z < sq - sq / 2; z++)
                    {

                        GameObject tile = UnityProxy.InstantiateDirectly(cubePrefab.gameObject, GridUtil.SnapToGrid(new Vector3(x * 2, -2, z * 2)), Quaternion.identity);
                        TileScript tileScript = tile.GetComponent<TileScript>();
                        tileScript.SetTileData(0);

                    }

                }

            }

            hasLoadedOfflineMap = true;

        }
        else
        {

            PlayerController localPlayerScript = playerObject.GetComponent<PlayerController>();

            NetworkTransform[] mapArray = FindObjectsByType<NetworkTransform>(FindObjectsSortMode.None);

            for (int i = 0; i < mapArray.Length; i++)
            {

                if (mapArray[i].GetComponent<HoldableObject>() != null)
                {

                    HoldableObject objHoldable = mapArray[i].GetComponent<HoldableObject>();

                    NetworkTransform newObject = null; // there should be something better...

                    if(objHoldable.GetComponent<TileScript>()) newObject = Instantiate(cubePrefab, mapArray[i].gameObject.transform.position, Quaternion.identity);
                    else if(objHoldable.GetComponent<ItemScript>()) newObject = Instantiate(itemPrefab, mapArray[i].gameObject.transform.position, Quaternion.identity);

                    if (localPlayerScript.holdObj && localPlayerScript.holdObj.gameObject == mapArray[i].gameObject)
                    {
                        playerHeldObjectRef = newObject;
                        objHoldable.isHeld = false; // jank
                    }

                    Destroy(mapArray[i].gameObject);

                }

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

        NetworkTransform[] mapArray = FindObjectsByType<NetworkTransform>(FindObjectsSortMode.None);

        for (int i = 0; i < mapArray.Length; i++)
        {

            if (mapArray[i].GetComponent<HoldableObject>() != null && (mapArray[i].isOwner || !mapArray[i].isSpawned))
            {

                Destroy(mapArray[i]);

            }

        }

    }

    public void LoadLocalGame()
    {

        hasLoadedOfflineMap = false;

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

    protected override void OnObserverRemoved(PlayerID player)
    {
        base.OnObserverRemoved(player);

        NetworkTransform[] temp = FindObjectsByType<NetworkTransform>(FindObjectsSortMode.None);

        int count = 0;
        for (int i = 0; i < temp.Length; i++)
        {

            if (temp[i].owner == player && temp[i].GetComponent<HoldableObject>() != null)
            {

                // could store a reference to the prefab in the object script to access -> spawn new
                if (temp[i].GetComponent<TileScript>()) Instantiate(cubePrefab, GridUtil.SnapToGrid(temp[i].transform.position), temp[i].transform.rotation);
                if (temp[i].GetComponent<ItemScript>()) Instantiate(itemPrefab, temp[i].transform.position, temp[i].transform.rotation);

                count++;

            }

        }
        Debug.Log(count + " objects replaced");

    }

}
