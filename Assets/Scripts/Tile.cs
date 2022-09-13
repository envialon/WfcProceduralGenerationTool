using System;
using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    [Serializable]
    public class Tile 
    {
        public Mesh mesh;
        public List<string> faces;
    }
}