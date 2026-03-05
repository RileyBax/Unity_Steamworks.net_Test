using PurrNet;
using UnityEngine;

public class TileScript : HoldableObject
{

    public LayerMask cubeLayer;
    private MeshRenderer mr;
    public ObjectAssetData objectAssetData;

    void Awake()
    {
        col = GetComponent<MeshCollider>();
        isHeld = new(false);
        isHeld.onChanged += UpdateCollider;
        type = EInteractable.Type.Tile;
        holdHeight = 2.0f;
        mr = GetComponent<MeshRenderer>();
        id = new(0);
        id.onChanged += UpdateTexture;
        UpdateTexture(id.value);
    }

    [ServerRpc]
    public override void SetDataRPC(int ETexture)
    {
        
        id.value = ETexture;

    }

    public override void SetData(int ETexture)
    {
        
        id.value = ETexture;
        
    }

    public void UpdateTexture(int newValue)
    {
        
        mr.material = objectAssetData.tileMatList[newValue];

    }

    public int GetID()
    {
        return id;
    }

    public override void OnPickup(GameObject player)
    {
        
        if(networkManager) SetHeldRPC(true);
        else isHeld.value = true;

        transform.localScale *= 0.5f;

    }

    public override bool OnPlace(Ray ray)
    {
        
        if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, cubeLayer))
        {
                
            Vector3 placePos = GridUtil.SnapToGrid(hit.collider.transform.position) + hit.normal * 2.0f;
            transform.position = placePos;
            if(networkManager) SetHeldRPC(false);
            else isHeld.value = false;
            transform.localScale *= 2f;

            return true;

        }

        return false;

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        SetData(id.value); // we can access id but not edit it! should use listeners with syncvars!!!
    }

    public void UpdateCollider(bool newValue)
    {
        
        col.enabled = !newValue;

    }

}