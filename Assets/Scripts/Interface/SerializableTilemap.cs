using UnityEngine;
using System.Collections.Generic;
using WFC_Model;

[System.Serializable]
public class SerializableTilemap : ScriptableObject
{
    [SerializeField]
    public Tilemap tilemap;

    [SerializeReference]
    public TileSet tileSet;

    public void SetFromTilemap(Tilemap tm, TileSet tileSet)
    {
        this.tilemap = new Tilemap(tm, tileSet.GetSymmetryDictionary());
    }

    public Tilemap GetTilemap()
    {
        for(int i = 0; i < tilemap.map.Length; i++)
        {
            tilemap.map[i].Set(tilemap.map[i].id, tilemap.map[i].rotation);
        }
        return new Tilemap(tilemap, tileSet.GetSymmetryDictionary());
    }

    public TileSet GetTileSet()
    {
        return tileSet;
    }

}
