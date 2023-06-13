
namespace WFC_Model
{
    [System.Serializable]
    public class Tilemap
    {
        public int width = 10;
        public int depth = 10;
        public int height = 1;
        public Tile[] map;

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

        public Tilemap(int mapSize = 10, int height = 1)
        {
            InitializeParams(mapSize, height, mapSize);
            Clear();
        }

        public Tilemap(int width = 10, int height = 1, int depth = 10)
        {
            InitializeParams(width, height, depth);
            Clear();
        }

        public Tilemap(Tilemap other)
        {
            InitializeParams(other.width, other.height, other.depth);
            this.map = (Tile[])other.map.Clone();
        }

        //Constructor from the output of the WFC algorithm
        public Tilemap(int[,,] indexMap)
        {
            InitializeParams(indexMap.GetLength(0), indexMap.GetLength(1), indexMap.GetLength(2));
            Clear();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        int tileId = indexMap[i, k, j] / 4;
                        int rotation = indexMap[i, k, j] - tileId * 4;
                        SetTile(new Tile(tileId, rotation), i, k, j);
                    }
                }
            }
        }

        public void SetTile(Tile tile, int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < width
                && z < depth && y < height)
            {
                map[x + (y * yOffset) + (z * zOffset)] = tile;
            }
        }

        public void RotateAt(int x, int y, int z)
        {
            map[x + (y * yOffset) + (z * zOffset)].RotateClockwise();
        }

        public Tile GetTile(int x, int y, int z)
        {
            return map[x + (y * yOffset) + (z * zOffset)];
        }

        public int[,] Get2dPatternAt(int x, int y, int z, int patternSize)
        {
            int[,] output = new int[patternSize, patternSize];
            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    if (x + i >= width || y + j >= depth)
                    {
                        output[i, j] = 0; continue;
                    }
                    output[i, j] = map[(x + i) + (y * yOffset) + (z + j * zOffset)].id;

                }
            }
            return output;
        }

        public void Clear()
        {
            map = new Tile[width * height * depth];
        }
    }
}