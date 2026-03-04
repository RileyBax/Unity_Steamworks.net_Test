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
    public NetworkTransform holdObj;
    private GameManager gameManager;
    public SpriteManager spriteManager;

    // TODO:
    // holdObj change to List<NetworkTransform>
    // - can carry a list of items OR a list of tiles, cannot mix them
    // - - store a EInteractable.type of list variable, when list length is 0, type = null?
    // - make the tile smaller when isheld, do this inside tile script
    // Try remove middle mouse button, always have camera locked
    // - make sure left and right click register at center of screen
    // change player sprites to 3d cube object
    // add name tag to players with steam name
    // - add it to prefab, check if isOwner, hide prefab from self
    // stronger gravity, less floaty
    // items should be rigidbodies stronger throw should be fun
    // planted seeds dont need to be like minecraft
    // - throw seeds in a spread (holdObjList.Length % maxThrowAmount), one stalk grows out of each
    // - - oncollisionenter, physics cast area for tile layermask, if tile.type == dirt, plant
    // - - - plant() add seed to tile, instantiate produce object, start growing.
    // - - - must be certain distance from other planted seeds on that tile.
    // - left click harvest in area, would be better for frames to go directly to holdObj list
    // - - receive new seeds of same type on good harvest
    // - - should be satisfying
    // the world we start in is randomly generated
    // - messy world
    // - some types of produce is growing
    // - does not need to be perfect square or flat
    // - weeds that give random seeds


    // ISSUES:
    // how do we get the seeds?
    // - plants give new seeds on successful harvest
    // - - plant types: wheat, corn, potato, grass, weeds, tomato
    // - trader
    // how do we get more blocks?
    // - compost for dirt?
    // - - place harvest/weeds/failed harvest in a container?
    // trader? - big hand
    // - arrive once a day
    // - holds a bag that can be filled with produce
    // - gives back random items depending on value of items given
    // - - Items: Dirt, Stone, Seeds, Compost Bin, more tile types?
    // game play cycle?
    // - sandbox, no death/restart/game end, continuous, incremental, need certain types of produce for upgrades.
    // - roguelike, daily quota, game end on not met, incremental upgrades.
    // upgrades?
    // - yea

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
            // maybe a function that loads all previous player data?
            transform.position = localPlayerRef.transform.position;
            spriteManager.SetLookDir(localPlayerRef.GetComponent<PlayerController>().spriteManager.GetLookDir());
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

        TileScript tempScript = obj.GetComponent<TileScript>();

        if(!tempScript.isHeld){

            holdObj = obj;

            holdObj.GiveOwnership(localPlayer);

            if(networkManager) UpdateComponentsRPC(holdObj, false);
            else holdObj.GetComponent<BoxCollider>().enabled = false;
            holdObj.gameObject.layer = 2;

            tempScript.isHeld = true;

        }

    }

    private void PlaceObject(RaycastHit hit)
    {

        TileScript tempScript = holdObj.GetComponent<TileScript>();

        // snap to grid pos
        Vector3 placePos = GridUtil.SnapToGrid(hit.collider.transform.position) + hit.normal * 2.0f;
        holdObj.GetComponent<NetworkTransform>().transform.position = placePos;

        if(networkManager) UpdateComponentsRPC(holdObj, true);
        else holdObj.GetComponent<BoxCollider>().enabled = true;
        holdObj.gameObject.layer = 7;
    

        tempScript.isHeld = false;

        holdObj = null;

    }

    [ObserversRpc(runLocally:true, bufferLast:true)]
    private void UpdateComponentsRPC(NetworkTransform obj, bool action)
    {
        
        obj.GetComponent<BoxCollider>().enabled = action;

    }

}
