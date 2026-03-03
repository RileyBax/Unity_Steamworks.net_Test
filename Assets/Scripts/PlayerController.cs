using System.Timers;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Tilemaps;

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
    public GameObject holdObj;
    private GameManager gameManager;
    public SpriteManager spriteManager;
    public bool wasServer = false;
    private float holdObjHeight = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.gameSave = gameManager.tileSaveManager.LoadGame();

        cineCamObj = GameObject.Find("CinemachineCamera");
        cineCam = cineCamObj.GetComponent<CinemachineCamera>();
        cineInput = cineCamObj.GetComponent<CinemachineInputAxisController>();
            
        if (isOwner || !networkManager) cineCam.Follow = transform;

        cineInput.enabled = false;

        characterController = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {

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

        if(transform.position.y < -100) transform.position = new Vector3(0, 10.0f, 0);

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if(!gameManager) gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        GameObject localPlayerRef = GameObject.Find("LocalPlayer");
        if(localPlayerRef) Destroy(localPlayerRef);

        if(isServer && !gameManager.hasLoadedMap){
            wasServer = true;
            gameManager.DrawFarm(true);
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

            PlaceObject(ray);

        }
        else if (Input.GetMouseButtonDown(1) && !holdObj)
        {
            
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);

            if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, cubeLayer)) GrabObject(hit.collider.gameObject);

        }

    }

    private void HoldObject()
    {

        if (holdObj)
        {
            
            holdObj.transform.position = transform.position + new Vector3(0, holdObjHeight, 0);

        }

    }

    public void GrabObject(GameObject obj)
    {

        HoldableObject holdable = obj.GetComponent<HoldableObject>();

        if(!holdable.isHeld && (holdable.id != 0 || holdable.type == EInteractable.Type.Item)){

            holdObj = obj;
            holdObj.GetComponent<NetworkTransform>().GiveOwnership(localPlayer);

            if(networkManager) UpdateComponentsRPC(holdObj.GetComponent<NetworkTransform>(), false);
            else holdable.col.enabled = false;

            holdObjHeight = holdObj.GetComponent<HoldableObject>().holdHeight;
            holdable.OnPickup(gameObject);

        }

    }

    private void PlaceObject(Ray ray)
    {

        HoldableObject holdable = holdObj.GetComponent<HoldableObject>();
        
        if (holdable.OnPlace(ray))
        {

            if(networkManager) UpdateComponentsRPC(holdObj.GetComponent<NetworkTransform>(), true);
            else holdable.col.enabled = true;

            holdObj = null;
        }

        

    }

    [ObserversRpc(runLocally:true, bufferLast:true)]
    private void UpdateComponentsRPC(NetworkTransform obj, bool action)
    {

        if(obj) obj.GetComponent<HoldableObject>().col.enabled = action;

    }

}
