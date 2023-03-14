using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace WFC_Procedural_Generator_Framework
{
    public enum Directions
    {
        north,
        south,
        west,
        east
    }

    public struct PatternInfo
    {
        public int frecuency;
        public float relativeFrecuency;
        public float relativeFrecuencyLog2;
        public int[,,] pattern;

        public Dictionary<Directions, List<int>> neigbourIndices;

        public PatternInfo(int[,,] pattern)
        {
            this.pattern = pattern;
            frecuency = 0;
            relativeFrecuency = 0;
            relativeFrecuencyLog2 = 0;
            neigbourIndices = new Dictionary<Directions, List<int>>
            {
                { Directions.north, new List<int>() },
                { Directions.south, new List<int>() },
                { Directions.west, new List<int>() },
                { Directions.east, new List<int>() }
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
        private int[,,] indexMap;
        private PatternInfo[] patterns;

        /// <summary>
        /// Deberemos transformar la información de tile y rotación a enteros puramente, acabaremos con 
        /// 4N índices únicos donde N es el tamaño de tileset.
        /// </summary>
        private void PopulateIndexMap()
        {
            Tile[,,] tilemap = inputTileMap.map;
            indexMap = new int[inputTileMap.mapSize, inputTileMap.height, inputTileMap.mapSize];

            for (int k = 0; k < height; k++)
            {
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < mapSize; j++)
                    {
                        indexMap[i, k, j] = tilemap[i, k, j].id * 4 + tilemap[i, k, j].rotation;
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
                    output[i, 0, j] = indexMap[i, 0, j];
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
                        patternFrecuency.Add(pattern, new PatternInfo(pattern));
                    }
                    totalPatterns++;
                    patternFrecuency[pattern]++;
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

            int[,,] currentGrid = current.pattern;
            int[,,] candidateGrid = candidate.pattern;

            for (int i = 0; i < patternSize; i++)
            {
                northNeighbour &= currentGrid[i, 0, patternSize] == candidateGrid[i, 0, 0];
                southNeighbour &= currentGrid[i, 0, 0] == candidateGrid[i, 0, patternSize];
                eastNeighbour &= currentGrid[patternSize, 0, i] == candidateGrid[0, 0, i];
                westNeighbour &= currentGrid[0, 0, i] == candidateGrid[patternSize, 0, i];
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

        private void PopulatePatternNeighbours()
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
            throw new NotImplementedException();
        }

        public InputReader(InputTileMapData inputTileMap)
        {
            this.inputTileMap = inputTileMap;
            this.mapSize = inputTileMap.mapSize;
            this.height = inputTileMap.height;
            PopulateIndexMap();
            PopulatePatternFrequency2D();
            PopulatePatternNeighbours();
        }

    }
}