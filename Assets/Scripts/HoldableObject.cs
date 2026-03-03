using PurrNet;
using UnityEngine;

public abstract class HoldableObject : NetworkBehaviour
{
    public bool isHeld { get; set; }
    public MeshCollider col { get; set; }
    public EInteractable.Type type { get; set; }
    public int id {get; set;} // fix this
    public int itemID {get; set;}
    public float holdHeight {get; set;}

    public virtual void OnPickup(GameObject player)
    {
        isHeld = true;
    }

    public virtual bool OnPlace(Ray ray)
    {
        isHeld = false;
        return false;
    }
    
}
