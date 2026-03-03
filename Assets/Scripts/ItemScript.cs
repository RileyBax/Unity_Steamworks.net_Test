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

    void Awake()
    {
        col = GetComponent<MeshCollider>();
        isHeld = false;
        type = EInteractable.Type.Item;
        holdHeight = 3.0f;
    }

    void Update()
    {

        if(isHeld) return;

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
        
        isHeld = true;
        velocity = Vector3.zero;
        isThrown = false;

    }

    public override bool OnPlace(Ray ray)
    {
        isHeld = false;

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
        return true;
    }

}
