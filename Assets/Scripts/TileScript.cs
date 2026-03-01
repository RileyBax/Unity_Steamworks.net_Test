using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileScript : MonoBehaviour
{
    [SerializeField] private int id;
    private BoxCollider col;
    public bool isHeld;

    void Start()
    {
        col = GetComponent<BoxCollider>();
        isHeld = false;
    }

    public void SetTileData(int id)
    {
        this.id = id;
    }

    public int GetID()
    {
        return id;
    }
    
}