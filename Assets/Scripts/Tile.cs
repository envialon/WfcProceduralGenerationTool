using System;
using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    public enum SymmetryTypes { 
        F, // no symmetry
        R, // radial symmetry
        I, // vertical axis symmetry
        H, // horizontal axis symmetry
        L, // counter-diagonal axis symmetry
        J, // main diagonal axis symmetry
        T, // horizonatal and vertical symmetry
        V, // double diagonal axis symmetry 
        X, // all transforms are identical
    }

    [Serializable]
    public class Tile 
    {
        public Mesh mesh;
        public string xPos;
        public string yPos;
        public string zPos;
        public string xNeg;
        public string yNeg;
        public string zNeg;
    }
}