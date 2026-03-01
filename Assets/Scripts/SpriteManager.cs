using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{

    public List<Sprite> spriteImport;
    //private Sprite[,] spriteMap = new Sprite[4,4];
    Vector3 prevPos;
    public SpriteRenderer spriteRenderer;
    public GameObject spriteFace; // TEMP

    void Start()
    {
        prevPos = transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.LookAt(Camera.main.transform.position);
        spriteFace.transform.LookAt(Camera.main.transform.position);

        // check camera angle relative to player, change sprite
        Vector3 camDir = Camera.main.transform.position - transform.parent.position;

        // Ignore vertical difference
        camDir.y = 0f;
        camDir.Normalize();

        Vector3 forward = transform.parent.position - prevPos;

        forward.y = 0f;
        forward.Normalize();

        // Angle between forward and camera direction
        float angle = Vector3.SignedAngle(forward, camDir, Vector3.up);

        // Convert -180..180 → 0..360
        angle = (angle + 360f) % 360f;

        // Divide into 4 sectors (90° each)
        int index = Mathf.RoundToInt((angle - 15f) / 90f) % 4;
        

        if(index == 0 || index == 2) spriteRenderer.sprite = spriteImport[0];
        else if(index == 1 || index == 3) spriteRenderer.sprite = spriteImport[1];

        if(index == 1) spriteRenderer.flipX = false;
        if(index == 3) spriteRenderer.flipX = true;

        if(index == 2) spriteFace.SetActive(false);
        else spriteFace.SetActive(true);
        
        
        if(Vector3.Distance(prevPos, transform.parent.position) > 0.1f) prevPos = transform.parent.position;

    }
}
