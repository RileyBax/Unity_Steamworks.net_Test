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
    public GameObject spriteFace; // TEMP
    private Vector3 lookDir;

    void Start()
    {
        prevPos = transform.parent.forward * 0.05f;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        transform.LookAt(Camera.main.transform.position);
        spriteFace.transform.LookAt(Camera.main.transform.position);

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
        

        if(index == 0 || index == 4) spriteRenderer.sprite = spriteImport[0];
        else if(index == 6 || index == 2) spriteRenderer.sprite = spriteImport[1];
        else if(index == 5 || index == 3) spriteRenderer.sprite = spriteImport[2];
        else if(index == 7 || index == 1) spriteRenderer.sprite = spriteImport[3];


        if(index == 2 || index == 3 || index == 1) spriteRenderer.flipX = false;
        if(index == 6 || index == 5 || index == 7) spriteRenderer.flipX = true;

        if(index == 4) spriteFace.SetActive(false);
        else spriteFace.SetActive(true);

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
