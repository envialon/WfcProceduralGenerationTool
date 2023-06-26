using UnityEngine;
using WFC_Model;

[System.Serializable]
public class SerializableTilemap : ScriptableObject
{
    [SerializeField]
    public Tilemap tilemap;

    [SerializeField]
    public TileSet tileSet;

    public void SetFromTilemap(Tilemap tm, TileSet tileSet)
    {
        this.tilemap = new Tilemap(tm);
    }

    public Tilemap GetTilemap()
    {
        return new Tilemap(tilemap);
    }

    public TileSet GetTileSet()
    {
        return tileSet;
    }
}
