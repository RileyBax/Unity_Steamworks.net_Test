using PurrNet;
using UnityEngine;

public abstract class HoldableObject : MonoBehaviour
{
    public bool isHeld { get; set; }
    public MeshCollider col {get; set;}

    public virtual void OnPickup(GameObject player)
    {
        isHeld = true;
    }

    public virtual void OnPlace(RaycastHit hit)
    {
        isHeld = false;
    }
}
