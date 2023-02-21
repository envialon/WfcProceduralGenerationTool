using System;
using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    [Serializable]
    public class Tile 
    {
        public int id = 0;
        public ulong configuration; // bitwise coding of the sockets of the tiles, 10 bits for each face, order still not determined
        public short rotation = 0; // bitwise coding of the 4 possible states of rotation over the Y axis, no other rotation allowed for now
    }
}