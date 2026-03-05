using System.Collections.Generic;
using System.Timers;
using PurrNet;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
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
    private List<GameObject> holdObj = new List<GameObject>();
    private GameManager gameManager;
    public bool wasServer = false;
    private float holdObjHeight = 2.0f;
    private LineRenderer lr;
    private const float grabTimerMax = 0.25f;
    private float grabTimer = grabTimerMax;
    private int segments = 60;
    private float radius = 0.05f;
    private EInteractable.Type holdListType = EInteractable.Type.Null;
    public PlayerObjectRenderer playerObjectRenderer;
    public TextMeshPro nameTag;
    private SyncVar<string> playerName = new(ownerAuth:true);

    void Awake()
    {
        
        playerName.onChanged += SetPlayerName;

    }

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

        lr = GetComponent<LineRenderer>();
        lr.enabled = false;

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


        if (!isGrounded) ySpeed += Physics.gravity.y / 7.5f * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) ySpeed = jumpHeight;

        moveDir.y = ySpeed;

        characterController.Move(moveDir * Time.deltaTime * speed);

        if (transform.position.y < -100) transform.position = new Vector3(0, 10.0f, 0);

        playerObjectRenderer.SetLookDir(moveDir);

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if (!gameManager) gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        GameObject localPlayerRef = GameObject.Find("LocalPlayer");
        if (localPlayerRef) Destroy(localPlayerRef);

        if(gameManager && isOwner) {
            playerName.value = gameManager.GetSteamName();
            nameTag.gameObject.SetActive(false);
        }

        if (isServer && !gameManager.hasLoadedMap)
        {
            wasServer = true;
            gameManager.DrawFarm(true);
        }

    }

    private void CamUpdate()
    {

        transform.LookAt(Camera.main.transform.position);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);

        if (Input.GetMouseButton(2)) cineInput.enabled = true;
        else cineInput.enabled = false;

    }

    private void ClickUpdate()
    {

        if (Input.GetMouseButtonDown(0) && holdObj.Count > 0)
        {

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);

            PlaceObject(ray);

        }
        else if (Input.GetMouseButton(1))
        {

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);

            if (Physics.Raycast(ray, out RaycastHit hit, 100.0f, cubeLayer)) GrabObject(hit);

        }

        if (Input.GetMouseButtonUp(1))
        {
            
            grabTimer = grabTimerMax;
            // reset linerenderer
            lr.positionCount = 0;

        }

    }

    private void HoldObject()
    {

        for (int i = 0; i < holdObj.Count; i++)
        {
                
            holdObj[i].transform.position = transform.position + new Vector3(0, holdObjHeight + i * holdObj[i].transform.localScale.y, 0);

        }

    }

    public void GrabObject(RaycastHit hit)
    {

        // this function is shocking
        
        HoldableObject holdable = hit.collider.gameObject.GetComponent<HoldableObject>();

        if(holdable.type == EInteractable.Type.Tile && grabTimer > 0 && holdable.id != (int) EInteractable.TileTexture.Bedrock && !holdable.isHeld && (holdListType == EInteractable.Type.Tile || holdListType == EInteractable.Type.Null))
        {
            grabTimer -= Time.deltaTime;
            // add point to line renderer every so frames
            float progress = 1f - (grabTimer / grabTimerMax);
            DrawTimerCircle(progress);
            lr.enabled = true;
        }
        else if (!holdable.isHeld && (holdable.id != (int) EInteractable.TileTexture.Bedrock || holdable.type == EInteractable.Type.Item) && (holdListType == holdable.type || holdListType == EInteractable.Type.Null))
        {

            holdObj.Add(hit.collider.gameObject);
            holdObj[holdObj.Count-1].GetComponent<NetworkTransform>().GiveOwnership(localPlayer);

            if (networkManager) UpdateComponentsRPC(holdObj[holdObj.Count-1].GetComponent<NetworkTransform>(), false);
            else holdable.col.enabled = false;

            holdObjHeight = holdObj[holdObj.Count-1].GetComponent<HoldableObject>().holdHeight;
            holdable.OnPickup(gameObject);

            grabTimer = grabTimerMax;
            lr.enabled = false;

            if(holdListType == EInteractable.Type.Null) holdListType = holdable.type;

        }

    }

    public void LoadGrabObject(List<GameObject> objList)
    {

        for(int i = 0; i < objList.Count; i++){

            HoldableObject holdable = objList[i].GetComponent<HoldableObject>();

            holdObj.Add(objList[i]);
            holdObj[holdObj.Count-1].GetComponent<NetworkTransform>().GiveOwnership(localPlayer);

            if (networkManager) UpdateComponentsRPC(holdObj[holdObj.Count-1].GetComponent<NetworkTransform>(), false);
            else holdable.col.enabled = false;

            holdObjHeight = holdObj[holdObj.Count-1].GetComponent<HoldableObject>().holdHeight;
            holdable.OnPickup(gameObject);

            if(holdListType == EInteractable.Type.Null) holdListType = holdable.type;

        }

    }

    private void PlaceObject(Ray ray)
    {

        HoldableObject holdable = holdObj[holdObj.Count-1].GetComponent<HoldableObject>();

        if (holdable.OnPlace(ray))
        {

            if (networkManager) UpdateComponentsRPC(holdObj[holdObj.Count-1].GetComponent<NetworkTransform>(), true);
            else holdable.col.enabled = true;

            holdObj.RemoveAt(holdObj.Count-1);
        }

        if(holdObj.Count == 0) holdListType = EInteractable.Type.Null;

    }

    [ObserversRpc(runLocally: true, bufferLast: true)]
    private void UpdateComponentsRPC(NetworkTransform obj, bool action)
    {

        if (obj) obj.GetComponent<HoldableObject>().col.enabled = action;

    }

    private void DrawTimerCircle(float progress) // progress 0 → 1
    {
        Vector3 mouseWorld = Camera.main.transform.position + Camera.main.ScreenPointToRay(Input.mousePosition).direction * 1.0f;

        int currentSegments = Mathf.CeilToInt(segments * progress);
        lr.positionCount = currentSegments + 1;

        float angleStep = 2f * Mathf.PI / segments;

        for (int i = 0; i <= currentSegments; i++)
        {
            float angle = i * angleStep;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            Vector3 offset = Camera.main.transform.right * x + Camera.main.transform.up * y;

            lr.SetPosition(i, mouseWorld + offset);
        }
    }

    private void SetPlayerName(string newValue)
    {
        
        nameTag.text = newValue;

    }

}
