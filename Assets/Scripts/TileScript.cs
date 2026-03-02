using PurrNet;
using UnityEngine;

public class TileScript : HoldableObject
{
    [SerializeField] private int id;
    void Awake()
    {
        col = GetComponent<MeshCollider>();
        isHeld = false;
    }

    public void SetTileData(int id)
    {
        this.id = id;
    }

    public int GetID()
    {
        return id;
    }

    public override void OnPickup(GameObject player)
    {
        
        isHeld = true;

    }

    public override void OnPlace(RaycastHit hit)
    {
        
        isHeld = false;

        Vector3 placePos = GridUtil.SnapToGrid(hit.collider.transform.position) + hit.normal * 2.0f;
        transform.position = placePos;

    }
    
}