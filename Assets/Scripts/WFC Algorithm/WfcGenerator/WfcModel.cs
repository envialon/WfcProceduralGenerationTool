using System.Collections;
using System.Collections.Generic;
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

        Dictionary<int, long> tileConfiguration;


        public WfcModel(TileSet tileSet = null, TileMapData tileMap = null, int width = 0, int height = 0, int depth = 0)
        {
            this.tileSet = tileSet;
            this.width = width;
            this.height = height;
            this.depth = depth;
            
            if (tileMap is not null)
            {
                Train(tileMap);
            }
        }
        public bool Train(TileMapData tileMap, TileSet tileSet)
        {
            this.tileSet = tileSet;
            return Train(tileMap);
        }

        public bool Train(TileMapData tileMap)
        {
            throw new System.NotImplementedException();
        }

        public TileMapData Generate()
        {
            if (!trained)
            {
                throw new System.Exception("Model not trained");
            }
            throw new System.NotImplementedException();
        }
    }
}
