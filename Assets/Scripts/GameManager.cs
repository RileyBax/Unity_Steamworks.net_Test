using System.Collections.Generic;
using PurrNet;
using TMPro;
using Unity.Mathematics;
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
                id = (int) EInteractable.SeedTexture.SeedWheat,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                type = EInteractable.Type.Item,
                isHeld = false,
            }, true);
            else objectManager.CreateNewObject(EInteractable.Type.Item, (int) EInteractable.SeedTexture.SeedWheat, Vector3.zero, Quaternion.identity);

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

            foreach(PlantData plant in gameSave.plants)
            {
                
                objectManager.CreatePlant(plant, isOnline);

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

                if(x == 0 && z == 0) {
                    objectManager.CreateNewObject(EInteractable.Type.Tile, 
                    (int) EInteractable.TileTexture.Bedrock, 
                    GridUtil.SnapToGrid(new Vector3(x * 2, -2, z * 2)), 
                    Quaternion.identity);
                }
                else {
                    
                    int absX = math.abs(x);
                    int absZ = math.abs(z);

                    float tileChance = 1f;

                    tileChance -= (float) (absX * 0.05f + absZ * 0.05);

                    int texture = UnityEngine.Random.Range(2, 4);
                    int yOffset = 0;

                    if(UnityEngine.Random.Range(0f, 1f) < 1f - tileChance) yOffset = UnityEngine.Random.Range(-1, 2);

                    if(UnityEngine.Random.Range(0f, 1f) < tileChance)
                    {
                        
                        GameObject newTile = objectManager.CreateNewObject(EInteractable.Type.Tile, 
                        texture, 
                        GridUtil.SnapToGrid(new Vector3(x * 2, -2 + yOffset, z * 2)), 
                        Quaternion.identity);

                        if(texture == (int)EInteractable.TileTexture.Dirt && UnityEngine.Random.Range(1, 4) == 1)
                        {
                            
                            for(int i = 0; i < UnityEngine.Random.Range(1, 3); i++){

                                objectManager.CreatePlant(new PlantData
                                {
                                    id = (int) EInteractable.PlantObject.Wheat, // make random here
                                    position = newTile.transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 1.0f, UnityEngine.Random.Range(-1f, 1f)),
                                    rotation = Quaternion.identity,
                                    type = EInteractable.Type.Plant,
                                    isGrown = true,
                                    growTime = 10f,
                                }, false);

                            }

                        }

                    }

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

                if(holdableObject.type != EInteractable.Type.Plant){

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

                }
                else
                {
                    
                    PlantController plantController = leaverObject[i].GetComponent<PlantController>();

                    PlantData plantData = new PlantData
                    {
                        id = holdableObject.id,
                        position = holdableObject.transform.position,
                        rotation = holdableObject.transform.rotation,
                        type = holdableObject.type,
                        isGrown = plantController.isGrown,
                        growTime = plantController.growTime
                        
                    };

                    objectManager.CreatePlant(plantData, true);

                }

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
