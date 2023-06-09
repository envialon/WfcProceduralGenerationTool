using WFC_Model;
using UnityEngine;
using UnityEditor;

public static class TilemapSerializer
{
    public static void SerializeTilemap(Tilemap tilemap, string assetPath, string filename = "tilemap.asset")
    {
        SerializableTilemap stm = ScriptableObject.CreateInstance<SerializableTilemap>();
        stm.SetFromTilemap(tilemap);
        AssetDatabase.CreateAsset(stm, assetPath + filename);
    }

    public static Tilemap DeserializeTilemap(string path)
    {
        SerializableTilemap stm = AssetDatabase.LoadAssetAtPath<SerializableTilemap>(path);
        return stm.GetTilemap();
    }
}
