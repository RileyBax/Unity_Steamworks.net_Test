using PurrNet;
using UnityEngine;

public class PlantController : HoldableObject
{

    public GameObject spriteGroup;
    public SpriteRenderer sprite1;
    public SpriteRenderer sprite2;
    private float growHeight = 1f;
    private const float growMaxTime = 10.0f;
    public SyncVar<float> growTime = new(growMaxTime);
    private Vector3 growStartPos;
    public SyncVar<bool> isGrown = new(false);
    private EInteractable.ProduceTexture produceItem;
    public ObjectAssetData objectAssetData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteGroup.transform.position -= new Vector3(0, growHeight, 0);
        growStartPos = spriteGroup.transform.position;
        
        type = EInteractable.Type.Plant;

        id = new(0);
        id.onChanged += UpdateSprite;

    }

    // Update is called once per frame
    void Update()
    {
        
        if(growTime > 0 && !isGrown){
            float pos = (1f - growTime / growMaxTime) * growHeight;
            spriteGroup.transform.position = growStartPos + new Vector3(0, pos, 0);
            if(networkManager) UpdateGrowTime(Time.deltaTime);
            else growTime.value -= Time.deltaTime;
        }
        else {
            if(networkManager) UpdateIsGrown(true);
            else isGrown.value = true;

            spriteGroup.transform.position = growStartPos + new Vector3(0, growHeight, 0);
        }

    }

    [ServerRpc]
    public void SetDataRPC(PlantData plantData)
    {
        isGrown.value = plantData.isGrown;
        growTime.value = plantData.growTime;
        id.value = plantData.id;
    }

    public void SetData(PlantData plantData)
    {
        isGrown.value = plantData.isGrown;
        growTime.value = plantData.growTime;
        id.value = plantData.id;
    }

    public void UpdateSprite(int newValue)
    {

        // object asset data sprite id
        produceItem = (EInteractable.ProduceTexture) newValue;
        sprite1.sprite = objectAssetData.plantSpriteList[newValue];
        sprite2.sprite = objectAssetData.plantSpriteList[newValue];

    }

    [ServerRpc]
    private void UpdateGrowTime(float value)
    {
        
        growTime.value -= value;

    }

    [ServerRpc]
    private void UpdateIsGrown(bool value)
    {
        
        isGrown.value = value;

    }

    public void HarvestPlant()
    {

        if (isGrown)
        {

            ObjectManager objectManager = GameObject.Find("ObjectManager").GetComponent<ObjectManager>(); // dont like this

            bool isOnline = networkManager ? true : false; // this probably wont work

            GameObject temp = objectManager.CreateObject(new ObjectData
            {
                id = (int) produceItem,
                position = transform.position + new Vector3(0, 0.5f, 0),
                rotation = Quaternion.identity,
                type = EInteractable.Type.Produce,
            }, isOnline);

            temp.SendMessage("HarvestForce");

            objectManager.CreateObject(new ObjectData
            {
                id = id,
                position = transform.position + new Vector3(0, 0.5f, 0),
                rotation = Quaternion.identity,
                type = EInteractable.Type.Item,
                isHeld = false,
            }, isOnline);

            Destroy(gameObject);

        }

    }

}
