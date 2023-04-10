namespace WFC_Procedural_Generator_Framework
{
    public class Tilemap
    {
        public int width = 10;
        public int depth = 10;
        public int height = 1;

        public Tile[,,] map;

        public Tilemap(int mapSize = 10, int height = 1)
        {
            this.width = mapSize;
            this.depth = mapSize;
            this.height = height;
            map = new Tile[mapSize, height, mapSize];
        }
        public Tilemap(int width = 10, int height = 1, int depth = 10)
        {
            this.width = width;
            this.depth = depth;
            this.height = height;
            map = new Tile[width, height, width];
        }

        public Tilemap(int[,,] indexMap, TileSet tileSet)
        {
            this.width = indexMap.GetLength(0);
            this.height = indexMap.GetLength(1);
            this.depth = indexMap.GetLength(2);
            throw new System.Exception();
        }

        public void SetTile(Tile tile, int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < width
                && z < depth && y < height)
            {
                map[x, y, z] = tile;
            }
        }

        public void RotateAt(int x, int y, int z)
        {
            map[x, y, z].RotateClockwise();
        }

        public Tile GetTile(int x, int y, int z)
        {
            return map[x, y, z];
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
                    output[i, j] = map[x + i, y, z + j].id;
                }
            }
            return output;
        }

        public void Clear()
        {
            map = new Tile[width, height, depth];
        }


    }
}