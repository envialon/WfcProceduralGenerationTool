using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace WFC_Procedural_Generator_Framework
{
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
                    output[i, 0, j] = offsettedIndexGrid[i+x, 0, j+y];
                }
            }
            return output;
        }


        private int hashPattern(int[,,] pattern)
        {
            string digits = "";
            foreach (int i in pattern)
            {
                digits += i;
            }
            int output = int.Parse(digits);
            return output;
        }

        private void PopulatePatternFrequency2D()
        {
            //usamos el diccionario para aprovechar el hasheo

            Dictionary<int, PatternInfo> patternFrecuency = new Dictionary<int, PatternInfo>();
            HashSet<PatternInfo> uniquePatterns = new HashSet<PatternInfo>();
            int totalPatterns = 0;

            for (int i = 0; i < mapSize - patternSize; i++)
            {
                for (int j = 0; j < mapSize - patternSize; j++)
                {
                    int[,,] pattern = Extract2DPatternAt(i, j);
                    int patternHash = hashPattern(pattern);
                    // PatternInfo candidate = new PatternInfo(pattern, uniquePatterns.Count);
                    if (!patternFrecuency.ContainsKey(hashPattern(pattern)))
                    {
                        //uniquePatterns.Add(candidate);
                        patternFrecuency.Add(patternHash, new PatternInfo(pattern, patternFrecuency.Count));
                        totalPatterns++;
                    }
                    patternFrecuency[patternHash]++;
                    patternGrid[i, 0, j] = patternFrecuency[patternHash].id;
                    //PatternInfo actualValue = new PatternInfo();
                    //uniquePatterns.TryGetValue(candidate, out actualValue);
                    //actualValue.frecuency++;
                    //patternGrid[i, 0, j] = actualValue.id;
                }
            }

            Debug.Log(totalPatterns);

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
                candidate.neigbourIndices[Direction.north].Add(currentIndex);
                current.neigbourIndices[Direction.north].Add(candidateIndex);
            }
            if (southNeighbour)
            {
                candidate.neigbourIndices[Direction.south].Add(currentIndex);
                current.neigbourIndices[Direction.south].Add(candidateIndex);
            }
            if (eastNeighbour)
            {
                candidate.neigbourIndices[Direction.east].Add(currentIndex);
                current.neigbourIndices[Direction.east].Add(candidateIndex);
            }
            if (westNeighbour)
            {
                candidate.neigbourIndices[Direction.west].Add(currentIndex);
                current.neigbourIndices[Direction.west].Add(candidateIndex);
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