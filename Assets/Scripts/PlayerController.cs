using PurrNet;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    private float speed = 10.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 moveDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

        transform.Translate(moveDir * Time.deltaTime * speed);

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

    }

}
