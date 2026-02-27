using System.Timers;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : NetworkBehaviour
{

    private float speed = 10.0f;
    private float fallSpeed = 50.0f;
    private GameObject cineCamObj;
    private CinemachineCamera cineCam;
    private CinemachineInputAxisController cineInput;
    private CharacterController characterController;
    public LayerMask playerLayer;
    public LayerMask cubeLayer;
    private float jumpHeight = 2.0f;
    private float ySpeed;
    private GameObject holdObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        cineCamObj = GameObject.Find("CinemachineCamera");
        cineCam = cineCamObj.GetComponent<CinemachineCamera>();
        cineInput = cineCamObj.GetComponent<CinemachineInputAxisController>();
            
        if (isOwner) cineCam.Follow = transform;

        cineInput.enabled = false;

        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!networkManager) return;

        PlayerMove();
        CamUpdate();
        ClickUpdate();

        HoldObject();

    }

    private void PlayerMove()
    {
        
        Vector3 moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDir = -transform.right * moveDir.x + -transform.forward * moveDir.z;

        bool isGrounded = Physics.CheckSphere(transform.position + new Vector3(0, 0.25f, 0), 0.30f, ~playerLayer);

        
        ySpeed += Physics.gravity.y * Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Space) && isGrounded) ySpeed = jumpHeight;

        moveDir.y = ySpeed;

        characterController.Move(moveDir * Time.deltaTime * speed);

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
        
    }

    private void CamUpdate()
    {
        
        transform.LookAt(Camera.main.transform.position);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);

        if(Input.GetMouseButton(2)) cineInput.enabled = true;
        else cineInput.enabled = false;

    }

    private void ClickUpdate()
    {

        if (Input.GetMouseButtonDown(0))
        {
            
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);

            if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, cubeLayer))
            {
                
                if(!holdObj) GrabObject(hit.collider.gameObject);
                else PlaceObject(hit);

                Debug.Log(hit.collider);

            }

        }

    }

    private void HoldObject()
    {

        if (holdObj)
        {
            
            holdObj.transform.position = transform.position + new Vector3(0, 2.0f, 0);

        }

    }

    private void GrabObject(GameObject obj)
    {
        
        holdObj = obj;
        holdObj.GetComponent<BoxCollider>().enabled = false;
        holdObj.layer = 2;

    }

    private void PlaceObject(RaycastHit hit)
    {
        
        holdObj.GetComponent<BoxCollider>().enabled = true;
        holdObj.layer = 7;

        // snap to grid pos
        Vector3 placePos = SnapToGrid(hit.collider.transform.position, 2f) + hit.normal * 2.0f;
        holdObj.transform.position = placePos;
    

        holdObj = null;

    }

    private Vector3 SnapToGrid(Vector3 position, float gridSize)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }

}
