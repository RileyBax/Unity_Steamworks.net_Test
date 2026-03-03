using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ObjectData
{
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public EInteractable.Type type;
    public bool isHeld;
}

[Serializable]
public class SaveData
{
    public List<ObjectData> objects = new List<ObjectData>();
    public PlayerData player;
    
}

[Serializable]
public class PlayerData
{
    
    public Vector3 position;
    public Quaternion rotation;

}