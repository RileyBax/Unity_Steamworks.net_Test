using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectAssetData", menuName = "Scriptable Objects/ObjectAssetData")]
public class ObjectAssetData : ScriptableObject
{
    
    public List<Material> tileMatList;
    public List<Mesh> seedMeshList;
    public List<Mesh> produceMeshList;
    public List<GameObject> prefabList;
    public List<Sprite> plantSpriteList;

}
