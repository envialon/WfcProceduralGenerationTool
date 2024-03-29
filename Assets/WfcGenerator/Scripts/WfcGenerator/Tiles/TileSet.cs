using System.Collections.Generic;
using UnityEngine;
using WFC_Model;

[CreateAssetMenu(fileName = "newTileSet", menuName = "ScriptableObjects/TileSet", order = 1)]
public class TileSet : ScriptableObject
{
    public List<TileAttributes> tiles;
        
    public void EnforceFirstTileIsEmpty()
    {
        if (tiles is null)
        {
            tiles = new List<TileAttributes>
            {
                new TileAttributes() //white space tile must have id 0
            };
        }
        else if (tiles.Count > 0 && tiles[0].mesh != null)
        {
            tiles.Insert(0, new TileAttributes()); //white space tile must have id 0
        }
    }

    public Mesh GetMesh(int id)
    {
        return tiles[id].mesh;
    }

    public Material GetMaterial(int id)
    {
        return tiles[id].material;
    }

    public SymmetryType GetSymmetry(int id)
    {
        return tiles[id].symmetry;
    }

    public Dictionary<int, SymmetryType> GetSymmetryDictionary()
    {
        Dictionary<int, SymmetryType> symmetryDictionary = new();
        for (int i = 0; i < tiles.Count; i++)
        {
            symmetryDictionary.Add(i, tiles[i].symmetry);
        }
        return symmetryDictionary;
    }


}
