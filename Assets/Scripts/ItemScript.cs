using PurrNet;
using UnityEngine;

public class ItemScript : HoldableObject
{

    public NetworkTransform savedPrefab;
    private bool isThrown;
    private Vector3 velocity;
    public LayerMask itemLayer;
    public LayerMask cubeLayer;
    private float throwForce = 10.0f;
    private bool clientIsHeld;

    void Awake()
    {
        col = GetComponent<MeshCollider>();
        isHeld = new(false);
        isHeld.onChanged += UpdateCollider;
        type = EInteractable.Type.Item;
        holdHeight = 3.0f;
        id = new();
        clientIsHeld = false;
    }

    void Update()
    {

        if(clientIsHeld) return;

        // Ground check
        bool isGrounded = Physics.CheckSphere(
            transform.position + new Vector3(0, 0.25f, 0),
            0.3f,
            ~itemLayer
        );

        if (!isThrown && isGrounded) return;

        // Apply gravity
        velocity += Physics.gravity * Time.deltaTime;

        // Move
        transform.position += velocity * Time.deltaTime;

        if (isGrounded)
        {
            isThrown = false;
            velocity = Vector3.zero;
        }

        if(transform.position.y < -100) Destroy(gameObject);
    }

    public override void OnPickup(GameObject player)
    {
        
        if(networkManager) SetHeldRPC(true);
        else isHeld.value = true;
        velocity = Vector3.zero;
        isThrown = false;

    }

    public override bool OnPlace(Ray ray)
    {

        clientIsHeld = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubeLayer))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            velocity = direction * throwForce;
        }
        else
        {
            velocity = ray.direction.normalized * throwForce;
        }

        isThrown = true;

        if(networkManager) SetHeldRPC(false);
        else isHeld.value = false;

        return true;
    }

    public void UpdateCollider(bool newValue)
    {
        
        clientIsHeld = newValue;
        col.enabled = !newValue;

    }

}
