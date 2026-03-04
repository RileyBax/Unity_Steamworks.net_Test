using System;
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
        isHeld = false;
        type = EInteractable.Type.Tile;
        holdHeight = 2.0f;
        mr = GetComponent<MeshRenderer>();
        SetTileData((EInteractable.TileTexture) id);
    }

    [ObserversRpc(bufferLast:true)]
    public void SetTileDataRPC(EInteractable.TileTexture ETexture)
    {
        
        id = (int) ETexture;
        mr.material = objectAssetData.tileMatList[(int) ETexture];

    }

    public void SetTileData(EInteractable.TileTexture ETexture)
    {
        
        id = (int) ETexture;
        mr.material = objectAssetData.tileMatList[(int) ETexture];

    }

    public int GetID()
    {
        return id;
    }

    public override void OnPickup(GameObject player)
    {
        
        isHeld = true;

    }

    public override bool OnPlace(Ray ray)
    {
        
        isHeld = false;

        if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, cubeLayer))
        {
                
            Vector3 placePos = GridUtil.SnapToGrid(hit.collider.transform.position) + hit.normal * 2.0f;
            transform.position = placePos;

            return true;

        }

        return false;

    }

}