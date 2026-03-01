using Unity.VisualScripting;
using UnityEngine;

public class TileObject
{

    private GameObject obj;
    private int id;
    private BoxCollider col;
    public bool isHeld;

    public TileObject(GameObject obj, int id)
    {
        
        this.obj = obj;
        this.id = id;
        col = this.obj.GetComponent<BoxCollider>();
        isHeld = false;

    }

    public void SetObject(GameObject obj)
    {
        this.obj = obj;
        col = this.obj.GetComponent<BoxCollider>();
    }

    public GameObject GetObject() {return this.obj;}
    public int GetID() {return this.id;}
    

}
