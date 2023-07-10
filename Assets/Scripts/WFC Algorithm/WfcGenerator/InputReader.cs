using GluonGui.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace WFC_Model
{

    /// <summary>
    /// This class is responsible for infering all of the unique patterns and pattern adjacency
    /// information from a given input tilemap.
    /// </summary>
    public class InputReader
    {
        public int patternSize;
        public int patternHeight;
        public Tilemap inputTileMap;
        public Dictionary<int, SymmetryType> symmetryDictionary;

        private int mapHeight;
        private int mapSize;
        private int[] indexGrid;
        private PatternInfo[] patterns;
        private int totalPatterns = 0;


        private int tileCount;

        // yOffset and zOffset are the values we need to muliply by the y and z
        // coordinates respectively to get the correct index in a pattern
        private int yOffset;
        private int zOffset;

        private void CalculatePatternOffsets()
        {
            yOffset = patternSize;
            zOffset = patternSize * patternHeight;
        }

        private void Initialize(Tilemap inputTileMap, int patternSize = 2, int patternHeight = 1)
        {
            this.patternSize = patternSize;
            this.inputTileMap = inputTileMap;
            this.mapSize = inputTileMap.width;
            this.mapHeight = inputTileMap.height;
            this.patternHeight = patternHeight;

            yOffset = patternSize;
            zOffset = patternSize * patternHeight;

            this.symmetryDictionary = inputTileMap.GetSymmetryDictionary();
        }

        public InputReader(Tilemap inputTileMap, int patternSize = 2)
        {
            Initialize(inputTileMap, patternSize);
        }

        private void PopulateIndexGrid()
        {
            indexGrid = new int[inputTileMap.width * inputTileMap.height * inputTileMap.depth];

            HashSet<int> indexes = new HashSet<int>();

            for (int k = 0; k < mapHeight; k++)
            {
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < mapSize; j++)
                    {
                        //not using yOffset and zOffset because we're mapping the indexMap, not a pattern.
                        indexGrid[i + (k * mapSize) + (j * mapHeight * mapSize)] = Tile.EncodeTile(inputTileMap.GetTile(i, k, j), symmetryDictionary);
                        indexes.Add(indexGrid[i + (k * mapSize) + (j * mapHeight * mapSize)]);
                    }
                }
            }
            tileCount = indexes.Count;
        }

        private static int mod(int x, int y)
        {
            return x - y * (int)Math.Floor((double)x / y);
        }

        private int GetOffsetedIndexGridAt(int x, int y, int z)
        {
            //not using yOffset and zOffset because we're mapping the indexMap, not a pattern.
            return indexGrid[mod(x, mapSize) + mod(y, mapHeight) * mapSize + mod(z, mapSize) * mapHeight * mapSize];
        }

        private long HashPattern(int[] pattern)
        {
            long hash = 0;
            long power = 1;
            for (int i = 0; i < pattern.Length; i++)
            {
                hash += pattern[pattern.Length - 1 - i] * power;
                power *= tileCount;
            }
            return hash;
        }

        private void UpdateFrecuencies()
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                patterns[i].UpdateFrecuencies(totalPatterns);
            }
        }

        private bool NorthNeighbour3D(in PatternInfo current, in PatternInfo candidate)
        {
            int[] currentGrid = current.pattern;
            int[] candidateGrid = candidate.pattern;

            for (int x = 0; x < patternSize; x++)
            {
                for (int y = 0; y < patternHeight; y++)
                {
                    for (int z = 1; z < patternSize; z++)
                    {
                        int a = currentGrid[x + y * yOffset + z * zOffset];
                        int b = candidateGrid[x + y * yOffset + (z - 1) * zOffset];
                        if (a != b)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool WestNeighbour3D(in PatternInfo current, in PatternInfo candidate)
        {
            int[] currentGrid = current.pattern;
            int[] candidateGrid = candidate.pattern;

            for (int x = 1; x < patternSize; x++)
            {
                for (int y = 0; y < patternHeight; y++)
                {
                    for (int z = 0; z < patternSize; z++)
                    {
                        int a = currentGrid[x + y * yOffset + z * zOffset];
                        int b = candidateGrid[(x - 1) + y * yOffset + z * zOffset];
                        if (a != b)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool UpperNeighbour3D(in PatternInfo current, in PatternInfo candidate)
        {
            int[] currentGrid = current.pattern;
            int[] candidateGrid = candidate.pattern;

            for (int x = 0; x < patternSize; x++)
            {
                for (int y = 1; y < patternHeight; y++)
                {
                    for (int z = 0; z < patternSize; z++)
                    {
                        int a = currentGrid[x + y * yOffset + z * zOffset];
                        int b = candidateGrid[x + (y - 1) * yOffset + z * zOffset];
                        if (a != b)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void CheckForNeighbourhood3D(int currentIndex, PatternInfo current, int candidateIndex, PatternInfo candidate)
        {
            if (NorthNeighbour3D(current, candidate))
            {
                candidate.neigbourIndices[Direction.south].Add(currentIndex);
                current.neigbourIndices[Direction.north].Add(candidateIndex);
            }
            if (WestNeighbour3D(current, candidate))
            {
                candidate.neigbourIndices[Direction.east].Add(currentIndex);
                current.neigbourIndices[Direction.west].Add(candidateIndex);
            }
            if (UpperNeighbour3D(current, candidate))
            {
                candidate.neigbourIndices[Direction.down].Add(currentIndex);
                current.neigbourIndices[Direction.up].Add(candidateIndex);
            }
        }

        private void FindOverlappingNeighbours3D()
        {
            int numberOfPatterns = patterns.Length;
            for (int i = 0; i < numberOfPatterns; i++)
            {
                PatternInfo current = patterns[i];
                for (int j = 0; j < numberOfPatterns; j++)
                {
                    PatternInfo candidate = patterns[j];
                    CheckForNeighbourhood3D(i, current, j, candidate);
                }
            }
        }

        private int[] Extract3DPatternAt(int x, int y, int z)
        {
            int[] output = new int[patternSize * patternSize * patternHeight];
            for (int k = 0; k < patternHeight; k++)
            {
                for (int i = 0; i < patternSize; i++)
                {
                    for (int j = 0; j < patternSize; j++)
                    {
                        int value = GetOffsetedIndexGridAt(x + i, y + k, z + j);
                        output[i + k * yOffset + j * zOffset] = value;
                    }
                }
            }
            return output;
        }

        private int[] ReflectMatrix(in int[] pattern)
        {
            int[] output = new int[patternSize * patternSize * patternHeight];

            for (int x = 0; x < patternSize; x++)
            {
                for (int y = 0; y < patternHeight; y++)
                {
                    for (int z = 0; z < patternSize; z++)
                    {
                        output[x + y * yOffset + z * zOffset] = pattern[x + y * yOffset + (patternSize - z - 1) * zOffset];
                    }
                }
            }
            return output;
        }

        private int[] RotateMatrix(in int[] pattern)
        {
            int[] output = new int[patternSize * patternSize * patternHeight];
            for (int x = 0; x < patternSize; x++)
            {
                for (int y = 0; y < patternHeight; y++)
                {
                    for (int z = 0; z < patternSize; z++)
                    {
                        output[x + y * yOffset + z * zOffset] = pattern[z + y * yOffset + x * zOffset];
                    }
                }
            }

            return ReflectMatrix(in output);
        }

        private void RotateIndividualTiles(ref int[] pattern)
        {
            int patternLength = pattern.Length;
            for (int i = 0; i < patternLength; i++)
            {
                if (pattern[i] != 0)
                {
                    Tile newTile = Tile.DecodeTile(pattern[i], symmetryDictionary);
                    newTile.RotateClockwise();
                    pattern[i] = Tile.EncodeTile(newTile, symmetryDictionary);
                }
            }
        }

        private void RotatePatterns3D(Dictionary<long, PatternInfo> patternFrecuency)
        {
            PatternInfo[] patterns = patternFrecuency.Values.ToArray();
            foreach (PatternInfo pattern in patterns)
            {
                int[] rotatedPattern = pattern.pattern;

                for (int direction = 1; direction < 4; direction++)
                {
                    rotatedPattern = RotateMatrix(in rotatedPattern);
                    RotateIndividualTiles(ref rotatedPattern);
                    long rotatedPatternHash = HashPattern(rotatedPattern);
                    if (!patternFrecuency.ContainsKey(rotatedPatternHash))
                    {
                        patternFrecuency.Add(rotatedPatternHash, new PatternInfo(patternFrecuency.Count,
                                                                          rotatedPattern,
                                                                          patternSize,
                                                                          patternHeight,
                                                                          pattern.frecuency,
                                                                          direction));
                    }

                    totalPatterns++;
                    patternFrecuency[rotatedPatternHash]++;
                }
            }
        }

        private void ReflectIndividualTiles(ref int[] pattern)
        {
            int patternLength = pattern.Length;
            for (int i = 0; i < patternLength; i++)
            {
                if (pattern[i] != 0)
                {
                    Tile newTile = Tile.DecodeTile(pattern[i], symmetryDictionary);
                    newTile.Reflect();
                    pattern[i] = Tile.EncodeTile(newTile,  symmetryDictionary);
                }
            }
        }

        private void ReflectPatterns3D(Dictionary<long, PatternInfo> patternFrecuency)
        {
            PatternInfo[] patterns = patternFrecuency.Values.ToArray();
            foreach (PatternInfo pattern in patterns)
            {
                //reflect it and check if its already in patternFrecuency, if not, add it
                int[] reflectedPattern = ReflectMatrix(in pattern.pattern);
                ReflectIndividualTiles(ref reflectedPattern);
                long reflectedPatternHash = HashPattern(reflectedPattern);
                if (!patternFrecuency.ContainsKey(reflectedPatternHash))
                {
                    patternFrecuency.Add(reflectedPatternHash, new PatternInfo(patternFrecuency.Count,
                                                                               reflectedPattern,
                                                                               patternSize,
                                                                               patternHeight,
                                                                               pattern.frecuency));
                }

                totalPatterns++;
                patternFrecuency[reflectedPatternHash]++;
            }
        }

        private void ExtractUniquePatterns3D(bool enablePatternReflection, bool enablePatternRotation, bool horizontalPeriodic, bool verticalPeriodic)
        {
            //usamos el diccionario para aprovechar el hasheo
            Dictionary<long, PatternInfo> patternFrecuency = new Dictionary<long, PatternInfo>();
            totalPatterns = 0;


            int horizontalStart = (horizontalPeriodic) ? -patternSize : 0;
            int horizontalEnd = (horizontalPeriodic) ? mapSize + patternSize : mapSize - patternSize;

            int verticalStart = (verticalPeriodic) ? -patternHeight : 0;
            int verticalEnd = (verticalPeriodic) ? mapHeight + patternHeight : mapHeight - patternHeight;


            for (int i = horizontalStart; i <= horizontalEnd; i++)
            {
                for (int k = verticalStart; k <= verticalEnd; k++)
                {
                    for (int j = horizontalStart; j <= horizontalEnd; j++)
                    {
                        int[] pattern = Extract3DPatternAt(i, k, j);
                        long patternHash = HashPattern(pattern);
                        if (!patternFrecuency.ContainsKey(HashPattern(pattern)))
                        {
                            patternFrecuency.Add(patternHash, new PatternInfo(patternFrecuency.Count, pattern, patternSize, patternHeight));
                        }
                        totalPatterns++;
                        patternFrecuency[patternHash]++;
                    }
                }

            }

            if (enablePatternRotation)
            {
                //Debug.Log("Rotation");
                RotatePatterns3D(patternFrecuency);
            }
            if (enablePatternReflection)
            {
                //Debug.Log("Reflection");
                ReflectPatterns3D(patternFrecuency);
            }


            patterns = patternFrecuency.Values.ToArray();
        }

        public void ReadInput(int patternSize = 2, Tilemap inputTileMap = null, bool enableReflection = true,
                                                bool enableRotation = true, bool sandwitchPatterns = true,
                                                bool horizontalPeriodic = true, bool verticalPeriodic = true)
        {
            if (inputTileMap is not null)
            {
                Initialize(inputTileMap, patternSize);
            }
            if (this.inputTileMap is null)
            {
                throw new Exception("The InputReader doesn't have any data to read.");
            }

            patterns = new PatternInfo[0];

            this.patternSize = patternSize;
            patternHeight = patternSize;

            if (sandwitchPatterns)
            {
                this.patternHeight = 2;
            }
            if (inputTileMap.height == 1)
            {
                this.patternHeight = 1;
            }

            CalculatePatternOffsets();

            PopulateIndexGrid();
            ExtractUniquePatterns3D(enableReflection, enableRotation, horizontalPeriodic, verticalPeriodic);
            UpdateFrecuencies();
            FindOverlappingNeighbours3D();
        }



        public string GetMatrixVisualization(int[] mat, int maxX = 10, int maxY = 1, int maxZ = 10)
        {
            string[] converted = Array.ConvertAll(mat, x => x.ToString());


            for (int i = maxX - 1; i < maxX * maxZ + maxY; i += maxX)
            {
                converted[i] += "\n";
            }
            string output = "\t" + string.Join("\t", converted);
            return output;
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
            string message = "";
            message += "InputMap:\n" + GetMatrixVisualization(indexGrid, mapSize, mapHeight, mapSize) + spacer + spacer;
            message += "Number of unique patterns: " + patterns.Length + "\n";
            message += "Pattern Info:\n" + spacer;
            foreach (PatternInfo pattern in patterns)
            {
                string patternMessage = "Pattern " + pattern.id + ":\n";
                patternMessage += "Frecuency: " + pattern.frecuency + "\n";
                patternMessage += "RelativeFrecuency: " + pattern.relativeFrecuency + "\n";
                patternMessage += "Tile pattern:\n " + GetMatrixVisualization(pattern.pattern, patternSize, patternHeight, patternSize) + "\n";
                patternMessage += "Neigbours:\n" + GetNeighboursVisualization(pattern.neigbourIndices) + "\n";


                message += patternMessage + spacer;
            }

            return message;
        }
        public PatternInfo[] GetPatternInfo()
        {
            return patterns;
        }

    }
}