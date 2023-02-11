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

        public WfcModel(TileSet tileSet = null, TileMap tileMap = null, int width = 0, int height = 0, int depth = 0)
        {
            this.tileSet = tileSet;
            this.width = width;
            this.height = height;
            this.depth = depth;
            if(tileMap is not null)
            {
                TrainModel(tileMap);
            }
        }
        
        public bool TrainModel(TileMap tileMap)
        {
            throw new System.NotImplementedException();
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
