using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public int id;
    public Vector3 position;
}

[Serializable]
public class TileDataList
{
    public List<TileData> tiles = new List<TileData>();
}
