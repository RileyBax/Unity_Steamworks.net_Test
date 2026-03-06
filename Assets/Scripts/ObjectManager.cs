using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class ObjectManager : NetworkBehaviour
{

    public ObjectAssetData objectAssetData;

    public GameObject CreatePlant(PlantData plant, bool isOnline)
    {
        
        GameObject newObject;

        if(isOnline){

            newObject = Instantiate(objectAssetData.prefabList[(int)plant.type], plant.position, plant.rotation);
            SetPlantDataRPC(newObject, plant);

        }
        else
        {

            newObject = UnityProxy.InstantiateDirectly(objectAssetData.prefabList[(int)plant.type], plant.position, plant.rotation);
            SetPlantData(newObject, plant);

        }

        return newObject;

    }

    public GameObject CreateObject(ObjectData obj, bool isOnline)
    {
        
        GameObject newObject;

        if(isOnline){

            newObject = Instantiate(objectAssetData.prefabList[(int)obj.type], obj.position, obj.rotation);
            SetObjectTextureRPC(newObject, obj.id);

        }
        else
        {
            
            newObject = UnityProxy.InstantiateDirectly(objectAssetData.prefabList[(int)obj.type], obj.position, obj.rotation);
            SetObjectTexture(newObject, obj.id);

        }

        return newObject;

    }

    public GameObject CreateNewObject(EInteractable.Type EObjectType, int ETexture, Vector3 position, Quaternion rotation)
    {

        GameObject newObject = UnityProxy.InstantiateDirectly(objectAssetData.prefabList[(int) EObjectType], position, rotation);
        SetObjectTexture(newObject, ETexture);

        return newObject;

    }

    [ObserversRpc(bufferLast:true)]
    private void SetObjectTextureRPC(GameObject newObject, int ETexture)
    {
        
        newObject.GetComponent<HoldableObject>().SetDataRPC(ETexture);

    }

    private void SetObjectTexture(GameObject newObject, int ETexture)
    {
        
        newObject.GetComponent<HoldableObject>().SetData(ETexture);

    }
    
    [ObserversRpc(bufferLast:true)]
    private void SetPlantDataRPC(GameObject newObject, PlantData plantData)
    {
        
        newObject.GetComponent<PlantController>().SetDataRPC(plantData);

    }

    private void SetPlantData(GameObject newObject, PlantData plantData)
    {
        
        newObject.GetComponent<PlantController>().SetData(plantData);

    }

}
