using System.Collections.Generic;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{
    [CreateAssetMenu(fileName = "newTileSet", menuName = "ScriptableObjects/TileSet", order = 1)]
    [System.Serializable]
    public class TileSet : ScriptableObject
    {
        public List<Tile> tiles;
    }
}
