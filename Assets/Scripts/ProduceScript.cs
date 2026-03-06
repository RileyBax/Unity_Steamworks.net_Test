using PurrNet;
using UnityEngine;

public class ProduceScript : HoldableObject
{
public NetworkTransform savedPrefab;
    private bool isThrown;
    private Vector3 velocity;
    public LayerMask itemLayer;
    public LayerMask cubeLayer;
    private float throwForce = 20.0f;
    private bool clientIsHeld;
    private Rigidbody rb;
    public ObjectAssetData objectAssetData;
    private MeshFilter meshFilter;
    private bool isPlanted = false;

    void Awake()
    {
        col = GetComponent<MeshCollider>();
        isHeld = new(false);
        isHeld.onChanged += UpdateCollider;
        type = EInteractable.Type.Produce;
        holdHeight = 3.0f;
        id = new(0);
        id.onChanged += UpdateMesh;
        clientIsHeld = false;
        rb = GetComponent<Rigidbody>();
        meshFilter = GetComponent<MeshFilter>();
        col.convex = true;

    }

    public override void OnPickup(GameObject player)
    {
        
        if(networkManager) SetHeldRPC(true);
        else isHeld.value = true;

        velocity = Vector3.zero;
        isThrown = false;
        SetCollider(false);
        rb.constraints = RigidbodyConstraints.FreezeAll;

    }

    public override bool OnPlace(Ray ray)
    {

        clientIsHeld = false;
        SetCollider(true);
        rb.constraints = RigidbodyConstraints.None;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubeLayer))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            velocity = direction * throwForce;
            rb.AddForce(velocity, ForceMode.Impulse);
        }
        else
        {
            velocity = ray.direction.normalized * throwForce;
            rb.AddForce(velocity, ForceMode.Impulse);
        }

        isThrown = true;

        if(networkManager) SetHeldRPC(false);
        else isHeld.value = false;

        return true;
    }

    [ObserversRpc(runLocally:true, bufferLast:true)]
    public void SetCollider(bool active)
    {
        
        col.enabled = active;

    }

    public void UpdateCollider(bool newValue)
    {
        
        clientIsHeld = newValue;

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

    public void UpdateMesh(int newValue)
    {

        meshFilter.mesh = objectAssetData.produceMeshList[newValue];
        col.sharedMesh = meshFilter.mesh;

    }

    public void HarvestForce()
    {
        
        Vector3 dir = Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        Vector3 rot = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));

        rb.AddForce(dir * 4f, ForceMode.Impulse);
        rb.AddTorque(rot * 10f);

    }

}
