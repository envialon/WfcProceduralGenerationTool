using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WFC_Model;

[System.Serializable]
public class SerializableTilemap : ScriptableObject
{
    public int width = 10;
    public int depth = 10;
    public int height = 1;

    [SerializeField]
    public Tile[,,] tiles;    
       
    public void SetFromTilemap(Tilemap tm)
    {
        width = tm.width;
        depth = tm.depth;
        height = tm.height;
        tiles = (Tile[,,])tm.map.Clone();
    }

    public  Tilemap GetTilemap()
    {
        Tilemap tm = new Tilemap(width, height, depth);
        tm.map = (Tile[])tiles.Clone();
        return tm;
    }

}
