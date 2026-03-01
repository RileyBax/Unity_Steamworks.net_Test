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
    private GameObject cineCamObj;
    private CinemachineCamera cineCam;
    private CinemachineInputAxisController cineInput;
    private CharacterController characterController;
    public LayerMask playerLayer;
    public LayerMask cubeLayer;
    private float jumpHeight = 0.75f;
    private float ySpeed;
    public NetworkTransform holdObj;
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        cineCamObj = GameObject.Find("CinemachineCamera");
        cineCam = cineCamObj.GetComponent<CinemachineCamera>();
        cineInput = cineCamObj.GetComponent<CinemachineInputAxisController>();
            
        if (isOwner ||!networkManager) cineCam.Follow = transform;

        cineInput.enabled = false;

        characterController = GetComponent<CharacterController>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
        //if(!networkManager) return;

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

        
        if(!isGrounded) ySpeed += Physics.gravity.y / 7.5f * Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Space) && isGrounded) ySpeed = jumpHeight;

        moveDir.y = ySpeed;

        characterController.Move(moveDir * Time.deltaTime * speed);

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        GameObject localPlayerRef = GameObject.Find("LocalPlayer");
        if(localPlayerRef) {
            transform.position = localPlayerRef.transform.position;
            Destroy(localPlayerRef);
        }

        if(!gameManager) gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        

        if(isServer && !gameManager.hasLoadedMap){

            gameManager.DrawFarm();

            if(gameManager.playerHeldObjectRef) GrabObject(gameManager.playerHeldObjectRef);
        }
        
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

        if (Input.GetMouseButtonDown(0) && holdObj)
        {
            
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);

            if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, cubeLayer))
            {
                
                PlaceObject(hit);

            }

        }
        else if (Input.GetMouseButtonDown(1) && !holdObj)
        {
            
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);

            if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, cubeLayer)) GrabObject(hit.collider.GetComponent<NetworkTransform>());

        }

    }

    private void HoldObject()
    {

        if (holdObj)
        {
            
            holdObj.transform.position = transform.position + new Vector3(0, 2.0f, 0);

        }

    }

    private void GrabObject(NetworkTransform obj)
    {

        if(!gameManager.IsTileHeld(obj.gameObject)){

            holdObj = obj;

            holdObj.GiveOwnership(localPlayer);

            if(networkManager) UpdateComponentsRPC(holdObj, false);
            else holdObj.GetComponent<BoxCollider>().enabled = false;
            holdObj.gameObject.layer = 2;

            gameManager.SetTileHeld(obj.gameObject, true);

        }

    }

    private void PlaceObject(RaycastHit hit)
    {

        // snap to grid pos
        Vector3 placePos = SnapToGrid(hit.collider.transform.position, 2f) + hit.normal * 2.0f;
        holdObj.GetComponent<NetworkTransform>().transform.position = placePos;

        if(networkManager) UpdateComponentsRPC(holdObj, true);
        else holdObj.GetComponent<BoxCollider>().enabled = true;
        holdObj.gameObject.layer = 7;
    

        gameManager.SetTileHeld(holdObj.gameObject, false);

        holdObj = null;

    }

    [ObserversRpc(runLocally:true, bufferLast:true)]
    private void UpdateComponentsRPC(NetworkTransform obj, bool action)
    {
        
        obj.GetComponent<BoxCollider>().enabled = action;

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
