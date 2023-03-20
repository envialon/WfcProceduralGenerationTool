using System;
using System.Collections.Generic;
using System.Linq;

namespace WFC_Procedural_Generator_Framework
{
    public enum Directions
    {
        north,
        south,
        west,
        east
    }

    /// <summary>
    /// Contains all of the pattern information extracted from the input.
    /// </summary>
    public struct PatternInfo
    {
        public int id;
        public int frecuency;
        public float relativeFrecuency;
        public float relativeFrecuencyLog2;
        public int[,,] pattern;

        public Dictionary<Directions, HashSet<int>> neigbourIndices;

        public PatternInfo(int[,,] pattern, int patternId)
        {
            this.pattern = pattern;
            this.id = patternId;
            frecuency = 0;
            relativeFrecuency = 0;
            relativeFrecuencyLog2 = 0;
            neigbourIndices = new Dictionary<Directions, HashSet<int>>
            {
                { Directions.north, new HashSet<int>() },
                { Directions.south, new HashSet<int>() },
                { Directions.west, new HashSet<int>() },
                { Directions.east, new HashSet<int>() }
            };
        }

        public static PatternInfo operator ++(PatternInfo patternInfo)
        {
            patternInfo.frecuency++;
            return patternInfo;
        }
    }

    public class InputReader
    {
        public int patternSize = 2; // 2x2x2
        public TileSet tileSet;
        public InputTileMapData inputTileMap;

        private int height;
        private int mapSize;
        private int[,,] offsettedIndexGrid;
        private int[,,] patternGrid;
        private PatternInfo[] patterns;

        /// <summary>
        /// Deberemos transformar la información de tile y rotación a enteros puramente, acabaremos con 
        /// 4N índices únicos donde N es el tamaño de tileset.
        /// </summary>
        private void PopulateIndexGrid()
        {
            Tile[,,] tilemap = inputTileMap.map;
            offsettedIndexGrid = new int[inputTileMap.mapSize + patternSize, inputTileMap.height, inputTileMap.mapSize + patternSize];

            for (int k = 0; k < height; k++)
            {
                for (int i = 0; i < mapSize + patternSize; i++)
                {
                    for (int j = 0; j < mapSize + patternSize; j++)
                    {
                        offsettedIndexGrid[i, k, j] = tilemap[i % mapSize, k, j % mapSize].id * 4 + tilemap[i % mapSize, k, j % mapSize].rotation;
                    }
                }
            }
        }
        private int[,,] Extract2DPatternAt(int x, int y)
        {
            int[,,] output = new int[patternSize, 1, patternSize];
            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    output[i, 0, j] = offsettedIndexGrid[i, 0, j];
                }
            }
            return output;
        }


        private void PopulatePatternFrequency2D()
        {
            //usamos el diccionario para aprovechar el hasheo

            Dictionary<int[,,], PatternInfo> patternFrecuency = new Dictionary<int[,,], PatternInfo>();
            int totalPatterns = 0;

            for (int i = 0; i < mapSize - patternSize; i++)
            {
                for (int j = 0; j < mapSize - patternSize; j++)
                {
                    int[,,] pattern = Extract2DPatternAt(i, j);
                    if (!patternFrecuency.ContainsKey(pattern))
                    {
                        patternFrecuency.Add(pattern, new PatternInfo(pattern, totalPatterns));
                    }
                    totalPatterns++;
                    patternFrecuency[pattern]++;
                    patternGrid[i, 0, j] = patternFrecuency[pattern].id;
                }
            }

            patterns = patternFrecuency.Values.ToArray();
            int numberOfValues = patterns.Length;
            for (int i = 0; i < numberOfValues; i++)
            {
                patterns[i].relativeFrecuency = patterns[i].frecuency / totalPatterns;
                patterns[i].relativeFrecuencyLog2 = MathF.Log(patterns[i].relativeFrecuencyLog2, 2);
            }
        }

        private void CheckForNeighbourhood2D(int currentIndex, PatternInfo current, int candidateIndex, PatternInfo candidate)
        {
            bool northNeighbour = true;
            bool southNeighbour = true;
            bool eastNeighbour = true;
            bool westNeighbour = true;

            int lastIndex = patternSize - 1;
            int[,,] currentGrid = current.pattern;
            int[,,] candidateGrid = candidate.pattern;

            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 1; j < patternSize; j++)
                {
                    int mirrorJ = lastIndex - j;
                    northNeighbour &= currentGrid[i, 0, j] == candidateGrid[i, 0, mirrorJ];
                    southNeighbour &= currentGrid[i, 0, mirrorJ] == candidateGrid[i, 0, j];
                    eastNeighbour &= currentGrid[j, 0, i] == candidateGrid[mirrorJ, 0, i];
                    westNeighbour &= currentGrid[mirrorJ, 0, i] == candidateGrid[j, 0, i];
                }
            }
            if (northNeighbour)
            {
                candidate.neigbourIndices[Directions.north].Add(currentIndex);
                current.neigbourIndices[Directions.north].Add(candidateIndex);
            }
            if (southNeighbour)
            {
                candidate.neigbourIndices[Directions.south].Add(currentIndex);
                current.neigbourIndices[Directions.south].Add(candidateIndex);
            }
            if (eastNeighbour)
            {
                candidate.neigbourIndices[Directions.east].Add(currentIndex);
                current.neigbourIndices[Directions.east].Add(candidateIndex);
            }
            if (westNeighbour)
            {
                candidate.neigbourIndices[Directions.west].Add(currentIndex);
                current.neigbourIndices[Directions.west].Add(candidateIndex);
            }
        }

        private void FindOverlappingNeighbours()
        {
            int numberOfPatterns = patterns.Length;
            for (int i = 0; i < numberOfPatterns; i++)
            {
                PatternInfo current = patterns[i];
                for (int j = i; j < numberOfPatterns; j++)
                {
                    PatternInfo candidate = patterns[i];
                    CheckForNeighbourhood2D(i, current, j, candidate);
                }
            }
        }

        private void PopulatePatternNeighbours()
        {
            FindOverlappingNeighbours();
        }

        public PatternInfo[] GetPatternInfo()
        {
            return patterns;
        }

        public InputReader(InputTileMapData inputTileMap, int patternSize = 2)
        {
            this.patternSize = patternSize;
            this.inputTileMap = inputTileMap;
            this.mapSize = inputTileMap.mapSize;
            this.height = inputTileMap.height;
            this.patternGrid = new int[mapSize, 1, mapSize];
            PopulateIndexGrid();
            PopulatePatternFrequency2D();
            PopulatePatternNeighbours();
        }
    }
}