using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class PlayerController : NetworkBehaviour
{

    private float speed = 10.0f;
    private GameObject cineCamObj;
    private CinemachineCamera cineCam;
    private CinemachineInputAxisController cineInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        cineCamObj = GameObject.Find("CinemachineCamera");
        cineCam = cineCamObj.GetComponent<CinemachineCamera>();
        cineInput = cineCamObj.GetComponent<CinemachineInputAxisController>();
            
        if (isOwner) cineCam.Follow = transform;

        cineInput.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!networkManager) return;

        PlayerMove();
        CamUpdate();

    }

    private void PlayerMove()
    {
        
        Vector3 moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //moveDir = transform.right * moveDir.x + transform.forward * moveDir.z;

        if(moveDir.magnitude > 0) transform.Translate(-moveDir * Time.deltaTime * speed);

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

}
