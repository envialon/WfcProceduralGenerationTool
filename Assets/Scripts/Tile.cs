using System;
using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    [Serializable]
    public class Tile 
    {
        public GameObject prefab;
        public string xPos;
        public string yPos;
        public string zPos;
        public string xNeg;
        public string yNeg;
        public string zNeg;
    }
}