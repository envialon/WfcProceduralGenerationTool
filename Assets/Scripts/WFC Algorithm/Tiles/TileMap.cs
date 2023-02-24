using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{
    public class TileMap 
    {
        public int mapSize = 10;
        public Tile[,] map;

        public TileMap(int mapSize)
        {
            this.mapSize = mapSize;
            map = new Tile[mapSize, mapSize];
        }
    }
}