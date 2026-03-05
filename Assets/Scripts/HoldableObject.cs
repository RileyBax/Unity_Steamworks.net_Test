using PurrNet;
using UnityEngine;

public abstract class HoldableObject : NetworkBehaviour
{
    public SyncVar<bool> isHeld { get; set; }
    public MeshCollider col { get; set; }
    public EInteractable.Type type { get; set; }
    public SyncVar<int> id {get; set;} // fix this
    public int itemID {get; set;}
    public float holdHeight {get; set;}

    public virtual void OnPickup(GameObject player)
    {
        isHeld.value = true;
    }

    public virtual bool OnPlace(Ray ray)
    {
        isHeld.value = false;
        
        return false;
    }

    [ServerRpc]
    public virtual void SetHeldRPC(bool action)
    {
        isHeld.value = action;
    }

    [ServerRpc]
    public virtual void SetDataRPC(int ETexture){}

    public virtual void SetData(int ETexture){}
    
}
