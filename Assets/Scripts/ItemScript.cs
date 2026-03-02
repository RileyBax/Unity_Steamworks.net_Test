using PurrNet;
using UnityEngine;

public class ItemScript : HoldableObject
{

    public NetworkTransform savedPrefab;

    void Awake()
    {
        col = GetComponent<MeshCollider>();
        isHeld = false;
    }

    public override void OnPickup(GameObject player)
    {
        
        isHeld = true;

    }

    public override void OnPlace(RaycastHit hit)
    {
        
        isHeld = false;

        transform.position = hit.point; // throw item here? give it rigid body?

    }

}
