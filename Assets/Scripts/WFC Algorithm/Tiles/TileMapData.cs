using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{
    public class InputTileMapData
    {
        public int mapSize = 10;
        public int height = 1;
        public Tile[,,] map;

        public InputTileMapData(int mapSize = 10, int height = 1)
        {
            this.mapSize = mapSize;
            this.height = height;
            map = new Tile[mapSize, height, mapSize];
        }

        public void SetTile(Tile tile, int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < mapSize
                && z < mapSize && y < height)
            {
                map[x, y, z] = tile;
            }
        }

        public void RotateAt(int x, int y, int z)
        {
            map[x, y, z].RotateClockwise();
        }

        public Tile GetTile(int x, int y, int z)
        {
            return map[x, y, z];
        }

        public int[,] Get2dPatternAt(int x, int y, int z, int patternSize)
        {
            int[,] output = new int[patternSize, patternSize];
            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    if (x + i >= mapSize || y + j >= mapSize)
                    {
                        output[i, j] = 0; continue;
                    }
                    output[i, j] = map[x + i, y, z + j].id;
                }
            }
            return output;
        }

        public void Clear()
        {
            map = new Tile[mapSize, height, mapSize];
        }


    }
}