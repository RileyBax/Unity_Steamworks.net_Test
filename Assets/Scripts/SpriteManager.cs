using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class SpriteManager : NetworkBehaviour
{

    public List<Sprite> spriteImport;
    //private Sprite[,] spriteMap = new Sprite[4,4];
    private Vector3 prevPos;
    public SpriteRenderer spriteRenderer;
    private Vector3 lookDir;

    void Start()
    {
        prevPos = transform.parent.forward * 0.05f;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        transform.LookAt(Camera.main.transform.position);

        // check camera angle relative to player, change sprite
        Vector3 camDir = Camera.main.transform.position - transform.parent.position;

        // Ignore vertical difference
        camDir.y = 0f;
        camDir.Normalize();

        Vector3 forward = lookDir;

        forward.y = 0f;
        forward.Normalize();

        // Angle between forward and camera direction
        float angle = Vector3.SignedAngle(forward, camDir, Vector3.up);

        // Convert -180..180 → 0..360
        angle = (angle + 360f) % 360f;

        // Divide into 4 sectors (90° each)
        int index = Mathf.RoundToInt(angle / 45f) % 8;
        

        spriteRenderer.sprite = spriteImport[index];

        if(networkManager) UpdateLookDirRPC();
        else UpdateLookDir();

    }

    [ObserversRpc(runLocally:true, bufferLast:true)]
    private void UpdateLookDirRPC()
    {
        
        Vector3 moveDir = transform.parent.position - prevPos;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            lookDir = moveDir.normalized;
            prevPos = transform.parent.position;
        }

    }

    private void UpdateLookDir()
    {
        
        Vector3 moveDir = transform.parent.position - prevPos;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            lookDir = moveDir.normalized;
            prevPos = transform.parent.position;
        }

    }

    public void SetLookDir(Vector3 dir)
    {
        
        lookDir = dir;

    }

    public Vector3 GetLookDir()
    {
        return lookDir;
    }
}
