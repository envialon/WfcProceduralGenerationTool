using System.Collections.Generic;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{
    [CreateAssetMenu(fileName = "newTileSet", menuName = "ScriptableObjects/TileSet", order = 1)]
    public class TileSet : ScriptableObject
    {
        [SerializeField]
        public List<TileAttributes> tiles;

        private void Awake()
        {
            if (tiles is null)
            {
                tiles = new List<TileAttributes>();
            }
            if (tiles.Count > 0)
            {
                tiles.Insert(0, new TileAttributes()); //white space tile must have id 0
            }
            else
            {
                tiles.Add(new TileAttributes()); //white space tile must have id 0
            }
        }

    }
}
