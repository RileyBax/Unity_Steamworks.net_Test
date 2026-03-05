using System.IO;
using UnityEngine;
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

        if(!networkManager || wasServer) SaveGame();

    }

    public void SaveGame()
    {
        NetworkTransform[] sceneObjects = FindObjectsByType<NetworkTransform>(FindObjectsSortMode.None);

        SaveData dataList = new SaveData();

        foreach (var IObject in sceneObjects)
        {
            if(IObject.GetComponent<HoldableObject>()){

                HoldableObject holdableObject = IObject.GetComponent<HoldableObject>();

                Vector3 setPos = IObject.transform.position;

                if(holdableObject.type == EInteractable.Type.Tile) setPos = GridUtil.SnapToGrid(IObject.transform.position);

                ObjectData data = new ObjectData
                {
                    id = holdableObject.id,
                    position = setPos,
                    rotation = holdableObject.transform.rotation,
                    type = holdableObject.type,
                    isHeld = holdableObject.isHeld,
                };

                dataList.objects.Add(data);

            }
            else if(IObject.GetComponent<PlayerController>() && IObject.GetComponent<PlayerController>().wasServer || IObject.gameObject.name == "LocalPlayer")
            {

                PlayerData playerData = new PlayerData
                {
                    position = IObject.transform.position,
                    rotation = IObject.transform.rotation,
                };
                
                Debug.Log("saved pos: " + IObject.transform.position + ", " + IObject.transform.rotation);

                dataList.player = playerData;

            }
        }

        string json = JsonUtility.ToJson(dataList, true);
        
        File.WriteAllText(filePath, json);

        Debug.Log("Tiles saved to: " + filePath);
    }

    public SaveData LoadGame()
    {
        if (!File.Exists(filePath)) return null;

        string json = File.ReadAllText(filePath);
        SaveData dataList = JsonUtility.FromJson<SaveData>(json);

        return dataList;
    }
}