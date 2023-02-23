using System;


namespace WFC_Procedural_Generator_Framework
{
    [Serializable]
    public struct Tile
    {
        public int id;
        //public ulong configuration; // bitwise coding of the sockets of the tiles, 10 bits for each face, order still not determined
        public int rotation; // bitwise coding of the 4 possible states of rotation over the Y axis, no other rotation allowed for now

        public Tile(int id = 0, int rotation = 0)
        {            
            this.id = id;
            this.rotation = rotation;
        }

        public void Set(int id, int rotation)
        {            
            this.id = id;
            this.rotation = rotation;
        }

        public int RotateClockwise()
        {
            rotation = (rotation + 1) % 4;
            return rotation;
        }
    }
}