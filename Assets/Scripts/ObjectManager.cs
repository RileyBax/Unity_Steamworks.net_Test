using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class ObjectManager : NetworkBehaviour
{

    public List<GameObject> objectPrefabList;

    public GameObject CreateObject(ObjectData obj, bool isOnline)
    {
        
        GameObject newObject;

        if(isOnline){

            newObject = Instantiate(objectPrefabList[(int)obj.type], obj.position, obj.rotation);
            SetObjectTextureRPC(newObject, obj.id);

        }
        else
        {
            
            newObject = UnityProxy.InstantiateDirectly(objectPrefabList[(int)obj.type], obj.position, obj.rotation);
            SetObjectTexture(newObject, obj.id);

        }

        return newObject;

    }

    public GameObject CreateNewObject(EInteractable.Type EObjectType, int ETexture, Vector3 position, Quaternion rotation)
    {
        
        GameObject newObject = UnityProxy.InstantiateDirectly(objectPrefabList[(int) EObjectType], position, rotation);
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

}
