using WFC_Model;
using UnityEngine;
using UnityEditor;
using System;

public static class TilemapSerializer
{
    public static void SerializeTilemap(Tilemap tilemap, TileSet tileSet, string assetPath, string filename = "tilemap.asset")
    {
        SerializableTilemap stm = ScriptableObject.CreateInstance<SerializableTilemap>();
        stm.SetFromTilemap(tilemap, tileSet);
        AssetDatabase.CreateAsset(stm, assetPath + filename);
    }

    public static Tilemap DeserializeTilemap(string path)
    {
        SerializableTilemap stm = AssetDatabase.LoadAssetAtPath<SerializableTilemap>(path);
        if (!stm)
        {
            throw new System.Exception("Tilemap could not be loaded from path: " + path);
        }
        return stm.GetTilemap();
    }

    internal static TileSet DeserializeTileSet(string path)
    {
        SerializableTilemap stm = AssetDatabase.LoadAssetAtPath<SerializableTilemap>(path);
        if (!stm)
        {
            throw new System.Exception("Tilemap could not be loaded from path: " + path);
        }
        return stm.GetTileSet();
    }
}
