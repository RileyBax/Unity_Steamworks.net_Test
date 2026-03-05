using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Transports;
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
    public GameObject playerHeldObjectRef;
    public bool hasLoadedMap = false;
    public TileSaveManager tileSaveManager;
    private bool wasServer;
    public SaveData gameSave;
    public ObjectManager objectManager;

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

            if (networkManager) objectManager.CreateObject(new ObjectData
            {
                id = (int) EInteractable.ItemTexture.SeedWheat,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                type = EInteractable.Type.Item,
                isHeld = false,
            }, true);
            else objectManager.CreateNewObject(EInteractable.Type.Item, (int) EInteractable.ItemTexture.SeedWheat, Vector3.zero, Quaternion.identity);

        }

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        UpdatePlayerCount();

        wasServer = isServer;

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

    public void DrawFarm(bool isOnline)
    {

        DestroyFarm();

        gameSave = tileSaveManager.LoadGame(); // I PUT THIS HERE FOR A REASON! 

        // Load save file -> if null DrawNewFarm() -> else draw all objects from save, INCLUDING PLAYER

        if (gameSave != null)
        {

            PlayerController pc = GetLocalPlayerObject(isOnline);
            List<GameObject> heldObjects = new List<GameObject>();

            if(pc != null){

                pc.transform.position = gameSave.player.position;
                pc.transform.rotation = gameSave.player.rotation;

            }
    
            if(isOnline) {
                hasLoadedMap = true;
            }

            foreach (ObjectData obj in gameSave.objects)
            {

                GameObject newObject = objectManager.CreateObject(obj, isOnline);

                if (obj.isHeld) heldObjects.Add(newObject);

            }
            if(pc != null) pc.LoadGrabObject(heldObjects);

        }
        else CreateFarm();

    }

    private PlayerController GetLocalPlayerObject(bool isOnline)
    {
        
        PlayerController[] sceneObjects = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach(PlayerController player in sceneObjects)
        {

            if(player.isServer || (player.name == "LocalPlayer" && !isOnline)) return player;

        }

        return null;
    }

    private void CreateFarm()
    {

        // create new farm
        for (int x = -sq / 2; x < sq - sq / 2; x++)
        {

            for (int z = -sq / 2; z < sq - sq / 2; z++)
            {

                if(x != 0 || z != 0) {
                    objectManager.CreateNewObject(
                        EInteractable.Type.Tile, 
                        (int) EInteractable.TileTexture.Dirt, 
                        GridUtil.SnapToGrid(new Vector3(x * 2, -2, z * 2)), 
                        Quaternion.identity);
                }
                else {
                    objectManager.CreateNewObject(EInteractable.Type.Tile, 
                    (int) EInteractable.TileTexture.Bedrock, 
                    GridUtil.SnapToGrid(new Vector3(x * 2, -2, z * 2)), 
                    Quaternion.identity);
                }

            }

        }
    }

    private void DestroyFarm()
    {

        NetworkTransform[] mapArray = FindObjectsByType<NetworkTransform>(FindObjectsSortMode.None);

        int count = 0;
        for (int i = 0; i < mapArray.Length; i++)
        {

            if (mapArray[i].GetComponent<HoldableObject>() != null && (mapArray[i].isOwner || !mapArray[i].isSpawned))
            {

                Destroy(mapArray[i].gameObject);
                count++;

            }

        }
        Debug.Log("Destroyed: " + count);

    }

    public void LoadLocalGame()
    {

        playerText = GameObject.Find("Players").GetComponent<TextMeshProUGUI>();

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>(); // why?

        playerObject = UnityProxy.InstantiateDirectly(playerPrefab);
        playerObject.name = "LocalPlayer";

        DrawFarm(false);

    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
        // add a uiscript with a flip button vis
        hasLoadedMap = false;

        if (wasServer) tileSaveManager.SaveGame();
        LoadLocalGame();
    }

    protected override void OnObserverRemoved(PlayerID player)
    {
        base.OnObserverRemoved(player);

        NetworkTransform[] leaverObject = FindObjectsByType<NetworkTransform>(FindObjectsSortMode.None);
        // dumbass
        int count = 0;
        for (int i = 0; i < leaverObject.Length; i++)
        {

            HoldableObject holdableObject = leaverObject[i].GetComponent<HoldableObject>();

            if (leaverObject[i].owner == player && holdableObject != null && isHost)
            {

                Vector3 setPos = leaverObject[i].transform.position;
                if(holdableObject.type == EInteractable.Type.Tile) setPos = GridUtil.SnapToGrid(leaverObject[i].transform.position);

                ObjectData objectData = new ObjectData{
                    id = holdableObject.id, 
                    position = setPos, 
                    rotation = leaverObject[i].transform.rotation, 
                    type = holdableObject.type,
                    isHeld = false
                    };

                objectManager.CreateObject(objectData, true);

                count++;

            }

        }
        Debug.Log(count + " objects replaced");

    }

    public string GetSteamName()
    {
        return lobbyManager.GetSteamName();
    }

}
