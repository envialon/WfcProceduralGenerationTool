
using System.Collections.Generic;

namespace WFC_Model
{
    [System.Serializable]
    public class Tilemap
    {
        public int width = 10;
        public int depth = 10;
        public int height = 1;
        public Tile[] map;
        public Dictionary<int, SymmetryType> symmetryDictionary;


        private int yOffset;
        private int zOffset;

        private void InitializeParams(int width = 10, int height = 1, int depth = 10)
        {
            this.width = width;
            this.depth = depth;
            this.height = height;
            yOffset = width;
            zOffset = width * height;
        }

        public Tilemap(Dictionary<int, SymmetryType> symmetryDictionary, int mapSize = 10, int height = 1)
        {
            InitializeParams(mapSize, height, mapSize);
            this.symmetryDictionary = symmetryDictionary;
            Clear();
        }

        public Tilemap(Dictionary<int, SymmetryType> symmetryDictionary, int width = 10, int height = 1, int depth = 10)
        {
            InitializeParams(width, height, depth);
            this.symmetryDictionary = symmetryDictionary;
            Clear();
        }

        public Tilemap(Tilemap other, Dictionary<int, SymmetryType> symmetryDictionary = null)
        {
            InitializeParams(other.width, other.height, other.depth);
            map = (Tile[])other.map.Clone();
            this.symmetryDictionary = symmetryDictionary is null ? new Dictionary<int, SymmetryType>(other.symmetryDictionary) : symmetryDictionary;
        }


        public void SetTile(Tile tile, int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < width
                && z < depth && y < height)
            {
                map[x + (y * yOffset) + (z * zOffset)] = tile;
            }
        }

        public Dictionary<int, SymmetryType> GetSymmetryDictionary()
        {
            return symmetryDictionary;
        }

        public void ReflectAt(int x, int y, int z)
        {
            map[x + (y * yOffset) + (z * zOffset)].Reflect();
        }

        public void RotateAt(int x, int y, int z)
        {
            map[x + (y * yOffset) + (z * zOffset)].RotateClockwise();
        }

        public int GetEncodedTileAt(int x, int y, int z)
        {
            return Tile.EncodeTile(map[x + (y * yOffset) + (z * zOffset)], symmetryDictionary);
        }

        public Tile GetTile(int x, int y, int z)
        {
            return map[x + (y * yOffset) + (z * zOffset)];
        }

        public void Clear()
        {
            map = new Tile[width * height * depth];
        }

        public override string ToString()
        {

            string output = "";
            for (int i = 0; i < height; i++)
            {
                output += "Layer " + i + "\n";
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        output += map[k + (i * yOffset) + (j * zOffset)].id + " ";
                    }
                    output += "\n";
                }
            }
            return output;
        }

    }
}