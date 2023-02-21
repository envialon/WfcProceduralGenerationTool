using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{
    public class WfcModel
    {
        TileSet tileSet;
        int width;
        int height;
        int depth;
        bool trained = false;
        Dictionary<int, List<HashSet<int>>> adyacencyInfo;

        public WfcModel(TileSet tileSet = null, TileMap tileMap = null, int width = 0, int height = 0, int depth = 0)
        {
            this.tileSet = tileSet;
            this.width = width;
            this.height = height;
            this.depth = depth;
            if (tileMap is not null)
            {
                TrainModel(tileMap);
            }
        }

        public bool TrainModel(TileMap tileMap)
        {
            adyacencyInfo = new Dictionary<int, List<HashSet<int>>>();

            foreach (KeyValuePair<int[], Tile> keyValuePair in tileMap.tileMap)
            {
                int[] position = keyValuePair.Key;
                Tile candidate = keyValuePair.Value;
                Tile[] neighbours = GetNeighbours(tileMap, position);

                if (!adyacencyInfo.ContainsKey(candidate.id))
                {
                    adyacencyInfo[candidate.id] = new List<HashSet<int>>();
                }

                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (neighbours[i] is not null)
                    {
                        adyacencyInfo[candidate.id][(i + candidate.rotation) % 4].Add(neighbours[i].id);
                    }
                }
            }
            return true;
        }

        private Tile[] GetNeighbours(TileMap tileMap, int[] position)
        {
            Tile[] output = new Tile[4];

            output[0] = (position[0] + 1 > tileMap.maxY) ? null : tileMap.tileMap[new int[] { position[0], position[1] + 1, position[2] }];
            output[1] = (position[1] + 1 > tileMap.maxY) ? null : tileMap.tileMap[new int[] { position[0] + 1, position[1], position[2] }];

            output[2] = (position[0] - 1 < 0) ? null : tileMap.tileMap[new int[] { position[0], position[1] - 1, position[2] }];
            output[3] = (position[1] - 1 < 0) ? null : tileMap.tileMap[new int[] { position[0] - 1, position[1], position[2] }];

            return output;
        }

        public TileMap Generate()
        {
            if (!trained)
            {
                throw new System.Exception("Model not trained");
            }
            throw new System.NotImplementedException();
        }

    }
}
