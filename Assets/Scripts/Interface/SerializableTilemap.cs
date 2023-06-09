using UnityEngine;
using WFC_Model;

[System.Serializable]
public class SerializableTilemap : ScriptableObject
{
    [SerializeField]
    public Tilemap tilemap;

    public void SetFromTilemap(Tilemap tm)
    {
        this.tilemap = new Tilemap(tm);
    }

    public Tilemap GetTilemap()
    {
        return new Tilemap(tilemap);
    }
}
