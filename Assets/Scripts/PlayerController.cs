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

    // TODO:
    // hold to pickup tile? line renderer circle around mouse
    // holdObj change to List<NetworkTransform>
    // - can carry a list of items OR a list of tiles, cannot mix them
    // - - store a EInteractable.type of list variable, when list length is 0, type = null?
    // - make the tile smaller when isheld and rotate it?, do this inside tile script
    // - spiral helds items x and z instead of stack
    // texture type 0 shouldnt be unholdable
    // Try remove middle mouse button, always have camera locked?
    // - make sure left and right click register at center of screen
    // - - add crosshair?
    // change player sprites to 3d cube object
    // add name tag to players with steam name
    // - add it to prefab, check if isOwner, hide prefab from self
    // stronger gravity, less floaty
    // items should have stronger throw should be fun, maybe not rigidbodies, remove collision from player
    // planted seeds dont need to be like minecraft
    // - throw seeds in a spread (holdObjList.Length % maxThrowAmount), one sprite stalk grows out of each
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
    // - - plant types: wheat, corn, potato, grass, weeds, tomato, carrot
    // - trader
    // how do we get more blocks?
    // - compost for dirt?
    // - - place harvest/weeds/failed harvest in a container?
    // - trader
    // - - maybe miniture version of block?
    // how does the trader work? - big hand
    // - arrive once a day
    // - holds a bag that can be filled with produce
    // - gives back random items depending on value of items given
    // - - Items: Dirt, Stone, Seeds, Compost Bin, more tile types?
    // game play cycle?
    // - sandbox, no death/restart/game end, continuous, incremental, need certain types of produce for upgrades.
    // - roguelike, daily quota, game end on not met, incremental upgrades.
    // upgrades?
    // - throw amount, stack height, harvest quantity.
    // how do we store many items?
    // - just make a big pile of them?
    // how do we pick up many items?
    // - hold left click hoovers them up?
    // whats stopping the player from winning? we need some kind of antagonist/enemy/force.
    // - plant disease
    // - growth rate
    // - pests
    // - - rabbits and stuff?
    // - - can pick them up?
    // - weather effects?
    // - - strong wind, rain
    // - we need to have another mechanic, more things to do
    // - - building?
    // - - - why

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
