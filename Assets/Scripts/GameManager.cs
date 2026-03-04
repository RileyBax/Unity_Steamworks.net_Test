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
    private Vector3 playerSpawnPos;
    public GameObject playerHeldObjectRef;
    public bool hasLoadedMap = false;
    public TileSaveManager tileSaveManager;
    private bool wasServer;
    public List<GameObject> objectPrefabList;
    public SaveData gameSave;
    private int objectCount = 0;
    public List<Material> tileTextures;
    public List<Material> itemTextures;

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

            if(pc != null){

                pc.transform.position = gameSave.player.position;
                pc.transform.rotation = gameSave.player.rotation;
                Debug.Log("loaded player pos: " + pc.transform.position + ", " + pc.transform.rotation);

            }

            objectCount = gameSave.objects.Count;
    
            if(isOnline) {
                Debug.Log("creating online map");
                hasLoadedMap = true;
            }

            foreach (ObjectData obj in gameSave.objects)
            {

                CreateObject(obj, isOnline);

            }

        }
        else CreateFarm();

    }

    private void CreateObject(ObjectData obj, bool isOnline)
    {

        GameObject newObject;

        if (isOnline) {
            newObject = Instantiate(objectPrefabList[(int)obj.type], obj.position, obj.rotation);
            if(obj.type == EInteractable.Type.Tile) SetObjectTextureRPC(newObject, (EInteractable.TileTexture) obj.id);
        }
        else {
            newObject = UnityProxy.InstantiateDirectly(objectPrefabList[(int)obj.type], obj.position, obj.rotation);
            if(obj.type == EInteractable.Type.Tile) SetObjectTexture(newObject, (EInteractable.TileTexture) obj.id); 
        }

        if (obj.isHeld)
        {
            // set player hold to this
            PlayerController pc = GetLocalPlayerObject(isOnline); 
            if(pc != null) pc.GrabObject(newObject);

        }

    }

    [ObserversRpc(bufferLast:true)]
    private void SetObjectTextureRPC(GameObject newObject, EInteractable.TileTexture texture)
    {
        
        newObject.GetComponent<TileScript>().SetTileDataRPC(texture); // edit holdable object lazy bones

    }

    private void SetObjectTexture(GameObject newObject, EInteractable.TileTexture texture)
    {
        
        newObject.GetComponent<TileScript>().SetTileData(texture); // edit holdable object lazy bones

    }

    private PlayerController GetLocalPlayerObject(bool isOnline)
    {
        
        PlayerController[] sceneObjects = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach(PlayerController player in sceneObjects)
        {
            
            Debug.Log(player.isServer + ", " + player.name + ", " + networkManager);

            if(player.isServer || (player.name == "LocalPlayer" && !isOnline)) return player;

        }
        Debug.Log(sceneObjects.Length);

        return null;
    }

    private void CreateFarm()
    {

        // create new farm
        for (int x = -sq / 2; x < sq - sq / 2; x++)
        {

            for (int z = -sq / 2; z < sq - sq / 2; z++)
            {

                GameObject newTile = UnityProxy.InstantiateDirectly(objectPrefabList[(int)EInteractable.Type.Tile], GridUtil.SnapToGrid(new Vector3(x * 2, -2, z * 2)), Quaternion.identity);
                if(x != 0 || z != 0) SetObjectTexture(newTile, EInteractable.TileTexture.Dirt);
                else SetObjectTexture(newTile, EInteractable.TileTexture.Default);

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

        NetworkTransform[] temp = FindObjectsByType<NetworkTransform>(FindObjectsSortMode.None);

        int count = 0;
        for (int i = 0; i < temp.Length; i++)
        {

            if (temp[i].owner == player && temp[i].GetComponent<HoldableObject>() != null && isHost)
            {

                Vector3 setPos = temp[i].transform.position;
                if(temp[i].GetComponent<HoldableObject>().type == EInteractable.Type.Tile) setPos = GridUtil.SnapToGrid(temp[i].transform.position);

                GameObject replaceObject = Instantiate(objectPrefabList[(int)temp[i].GetComponent<HoldableObject>().type], setPos, temp[i].transform.rotation);
                if(temp[i].GetComponent<HoldableObject>().type == EInteractable.Type.Tile) SetObjectTexture(replaceObject, (EInteractable.TileTexture) temp[i].GetComponent<HoldableObject>().id); 
                count++;

            }

        }
        Debug.Log(count + " objects replaced");

    }

    public int GetObjectID()
    {

        Debug.Log(objectCount);
        return objectCount++;

    }

}
