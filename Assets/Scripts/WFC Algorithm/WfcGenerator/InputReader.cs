using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace WFC_Model
{
    public class InputReader
    {
        public int patternSize = 2; // 2x2x2
        public int patternHeight = 2;
        public Tilemap inputTileMap;

        private int height;
        private int mapSize;
        private int[,,] offsettedIndexGrid;
        private PatternInfo[] patterns;
        private int totalPatterns = 0;

        public bool enablePatternReflection;
        public bool enablePatternRotations;


        /// <summary>
        /// Deberemos transformar la información de tile y rotación a enteros puramente, acabaremos con 
        /// 4N índices únicos donde N es el tamaño de tileset.
        /// </summary>
        private void PopulateIndexGrid()
        {
            offsettedIndexGrid = new int[inputTileMap.width, inputTileMap.height, inputTileMap.depth];

            for (int k = 0; k < height; k++)
            {
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < mapSize; j++)
                    {
                        offsettedIndexGrid[i, k, j] = inputTileMap.GetTile(i, k, j).id * 4 + inputTileMap.GetTile(i, k, j).rotation;
                    }
                }
            }
        }

        private int mod(int x, int y)
        {
            return x - y * (int)Math.Floor((double)x / y);
        }

        private int[,,] Extract2DPatternAt(int x, int y)
        {
            int[,,] output = new int[patternSize, 1, patternSize];
            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    output[i, 0, j] = offsettedIndexGrid[mod((i + x), mapSize), 0, mod(j + y, mapSize)];
                }
            }
            return output;
        }


        private string HashPattern(int[,,] pattern)
        {
            string digits = "";
            foreach (int i in pattern)
            {
                digits += i + ".";
            }
            return digits;
        }

        private int[,,] ReflectMatrix2D(in int[,,] pattern)
        {
            int[,,] output = new int[patternSize, 1, patternSize];

            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    output[i, 0, j] = pattern[i, 0, patternSize - j - 1];
                }
            }
            return output;
        }

        private int[,,] RotateMatrix2D(in int[,,] pattern)
        {
            int[,,] output = new int[patternSize, 1, patternSize];
            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    output[i, 0, j] = pattern[j, 0, i];                   
                }
            }

            return ReflectMatrix2D(in output);

        }

        private void RotatePatterns(Dictionary<string, PatternInfo> patternFrecuency)
        {
            PatternInfo[] patterns = patternFrecuency.Values.ToArray();
            foreach (PatternInfo pattern in patterns)
            {
                int[,,] rotatedPattern;

                for (int direction = 0; direction < 3; direction++)
                {
                    rotatedPattern = RotateMatrix2D(in pattern.pattern);
                    string patternHash = HashPattern(rotatedPattern);
                    if (!patternFrecuency.ContainsKey(patternHash))
                    {
                        totalPatterns++;
                        patternFrecuency.Add(patternHash, new PatternInfo(rotatedPattern, patternFrecuency.Count, pattern.frecuency));
                    }
                }
            }
        }

        private void ReflectPatterns(Dictionary<string, PatternInfo> patternFrecuency)
        {
            PatternInfo[] patterns = patternFrecuency.Values.ToArray();
            foreach (PatternInfo pattern in patterns)
            {
                //reflect it and check if its already in patternFrecuency, if not, add it
                int[,,] reflectedPattern = ReflectMatrix2D(in pattern.pattern); 
                string reflectedPatternHash = HashPattern(reflectedPattern);
                if (!patternFrecuency.ContainsKey(reflectedPatternHash))
                {
                    totalPatterns++;
                    patternFrecuency.Add(reflectedPatternHash, new PatternInfo(reflectedPattern, patternFrecuency.Count, pattern.frecuency));
                }
            }
        }

        private void ExtractUniquePatterns()
        {
            //usamos el diccionario para aprovechar el hasheo
            Dictionary<string, PatternInfo> patternFrecuency = new Dictionary<string, PatternInfo>();
            totalPatterns = 0;

            for (int i = -patternSize; i <= mapSize + patternSize; i++)
            {
                for (int j = -patternSize; j <= mapSize; j++)
                {
                    int[,,] pattern = Extract2DPatternAt(i, j);
                    string patternHash = HashPattern(pattern);
                    if (!patternFrecuency.ContainsKey(HashPattern(pattern)))
                    {
                        patternFrecuency.Add(patternHash, new PatternInfo(pattern, patternFrecuency.Count));
                    }
                    totalPatterns++;
                    patternFrecuency[patternHash]++;
                }
            }

            if (enablePatternReflection)
            {
                //Debug.Log("Reflection");
                ReflectPatterns(patternFrecuency);
            }
            if (enablePatternRotations)
            {
                //Debug.Log("Rotation");
                RotatePatterns(patternFrecuency);
            }

            patterns = patternFrecuency.Values.ToArray();
        }

        private void UpdateFrecuencies()
        {

            for (int i = 0; i < patterns.Length; i++)
            {
                patterns[i].UpdateFrecuencies(totalPatterns);
            }
        }

        private bool EastNeighbour(PatternInfo current, PatternInfo candidate)
        {
            int[,,] currentGrid = current.pattern;
            int[,,] candidateGrid = candidate.pattern;

            for (int i = 1; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    int a = currentGrid[i, 0, j];
                    int b = candidateGrid[i - 1, 0, j];
                    if (a != b)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool NorthNeighbour(PatternInfo current, PatternInfo candidate)
        {
            int[,,] currentGrid = current.pattern;
            int[,,] candidateGrid = candidate.pattern;

            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 1; j < patternSize; j++)
                {
                    int a = currentGrid[i, 0, j];
                    int b = candidateGrid[i, 0, j - 1];
                    if (a != b)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void OldCheckForNeighborhood(int currentIndex, PatternInfo current, int candidateIndex, PatternInfo candidate)
        {
            if (NorthNeighbour(current, candidate))
            {
                candidate.neigbourIndices[Direction.south].Add(currentIndex);
                current.neigbourIndices[Direction.north].Add(candidateIndex);
            }
            if (EastNeighbour(current, candidate))
            {
                candidate.neigbourIndices[Direction.west].Add(currentIndex);
                current.neigbourIndices[Direction.east].Add(candidateIndex);
            }
        }

        private void FindOverlappingNeighbours()
        {
            int numberOfPatterns = patterns.Length;
            for (int i = 0; i < numberOfPatterns; i++)
            {
                PatternInfo current = patterns[i];
                for (int j = 0; j < numberOfPatterns; j++)
                {
                    PatternInfo candidate = patterns[j];
                    OldCheckForNeighborhood(i, current, j, candidate);
                }
            }
        }

        private void PopulatePatternNeighbours()
        {
            FindOverlappingNeighbours();
        }

        private void PlacePattern(ref int[,,] indexGrid, int patternId, int x, int y, int z)
        {
            for (int i = 0; i < patternSize; i++)
            {
                for (int j = 0; j < patternSize; j++)
                {
                    indexGrid[i + x, y, j + z] = patterns[patternId].pattern[i, 0, j];
                }
            }
        }

        public int[,,] GetIndexGridFromPatternIndexGrid(int[,,] patternIndexGrid)
        {
            int maxX = patternIndexGrid.GetLength(0);
            int maxY = patternIndexGrid.GetLength(1);
            int maxZ = patternIndexGrid.GetLength(2);

            int[,,] indexGrid = new int[maxX * patternSize, maxY * patternHeight, maxZ * patternSize];

            for (int i = 0; i < maxX; i++)
            {
                for (int j = 0; j < maxZ; j++)
                {
                    for (int k = 0; k < maxY; k++)
                    {
                        PlacePattern(ref indexGrid, patterns[patternIndexGrid[i, k, j]].id, i, k, j);
                    }
                }
            }


            return indexGrid;
        }

        public PatternInfo[] GetPatternInfo()
        {
            return patterns;
        }

        public void Train(int patternSize = 2, Tilemap inputTileMap = null, bool enableReflection = true, bool enableRotation = true)
        {
            this.enablePatternReflection = enableReflection;
            this.enablePatternRotations = enableRotation;
            if (inputTileMap is not null)
            {
                Initialize(inputTileMap, patternSize);
            }
            if (this.inputTileMap is null)
            {
                throw new Exception("The InputReader doesn't have any data to read.");
            }

            patterns = new PatternInfo[0];

            PopulateIndexGrid();
            ExtractUniquePatterns();
            UpdateFrecuencies();
            PopulatePatternNeighbours();
        }


        private void Initialize(Tilemap inputTileMap, int patternSize = 2)
        {
            this.patternSize = patternSize;
            this.inputTileMap = inputTileMap;
            this.mapSize = inputTileMap.width;
            this.height = inputTileMap.height;
        }

        public string GetMatrixVisualization(int[,,] mat)
        {
            string patternVisualization = "";
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(2); j++)
                {
                    patternVisualization += "\t" + mat[i, 0, j] + "\t";
                }
                patternVisualization += "\n";
            }
            return patternVisualization;
        }

        private string GetNeighboursVisualization(Dictionary<Direction, HashSet<int>> neighbours)
        {
            string str = "";

            foreach (KeyValuePair<Direction, HashSet<int>> entry in neighbours)
            {
                str += "\t" + entry.Key + ": ";
                foreach (int index in entry.Value)
                {
                    str += index + ", ";
                }
                str += "\n";
            }

            return str;
        }

        public string GetPatternSummary()
        {
            const string spacer = "\n/////////////////////\n";
            string messsage = "";
            messsage += "InputMap:\n" + GetMatrixVisualization(offsettedIndexGrid) + spacer + spacer;

            messsage += "Pattern Info:\n" + spacer;
            foreach (PatternInfo pattern in patterns)
            {
                string patternMessage = "Pattern " + pattern.id + ":\n";
                patternMessage += "Frecuency: " + pattern.frecuency + "\n";
                patternMessage += "RelativeFrecuency: " + pattern.relativeFrecuency + "\n";
                patternMessage += "Tile pattern:\n " + GetMatrixVisualization(pattern.pattern) + "\n";
                patternMessage += "Neigbours:\n" + GetNeighboursVisualization(pattern.neigbourIndices) + "\n";


                messsage += patternMessage + spacer;
            }

            return messsage;
        }

        public InputReader(Tilemap inputTileMap, int patternSize = 2)
        {
            Initialize(inputTileMap, patternSize);
        }
    }
}