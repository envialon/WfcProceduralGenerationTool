using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace WFC_Model
{
    // depht or wide mode 
    public class WfcSolver
    {
        public int width;
        public int height;
        public int depth;

        public bool deepFirstPropagation;

        public Cell[] cellMap;

        public List<int> uncollapsedCellIndices;

        private readonly PatternInfo[] patternInfo;
        private readonly Dictionary<int, SymmetryType> symmetryDictionary;

        private readonly int numberOfPatterns;
        private int collapsedCount = 0;

        private int yOffset;
        private int zOffset;

        private readonly Random random = new();

        private readonly Dictionary<Position, HashSet<int>> removalDictionary;

        private int[,] InitialEnablerCount()
        {
            int numberOfDirections = Enum.GetValues(typeof(Direction)).Length;
            int[,] result = new int[numberOfPatterns, numberOfDirections];

            for (int patternIndex = 0; patternIndex < numberOfPatterns; patternIndex++)
            {
                for (int direction = 0; direction < numberOfDirections; direction++)
                {
                    HashSet<int> compatibles = patternInfo[patternIndex].GetCompatiblesInDirection((Direction)direction);
                    foreach (int compatible in compatibles)
                    {
                        result[compatible, direction] += 1;
                    }
                }
            }
            return result;
        }

        private void RemoveBasedOnEncodedTile(int cellIndex, int encodedTile)
        {
            Position CellPos = cellMap[cellIndex].position;

            for (int patternIndex = 0; patternIndex < patternInfo.Length; patternIndex++)
            {
                if (encodedTile != patternInfo[patternIndex].GetEncodedTileIndex())
                {
                    cellMap[cellIndex].RemovePattern(patternIndex, patternInfo);
                    AddRemovalUpdate(CellPos, patternIndex);
                }
            }
            cellMap[cellIndex].CalculateEntrophy();

            if (cellMap[cellIndex].possiblePatterns.Count == 1)
            {
                CollapseCell(cellIndex, cellMap[cellIndex].possiblePatterns.First());
            }

            return;
        }

        private void InitializeOutputGrid(Tilemap incompleteMap = null)
        {
            int[,] enablerCountTemplate = InitialEnablerCount();

            yOffset = width;
            zOffset = width * height;

            cellMap = new Cell[width * height * depth];
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int cellIndex = x + y * yOffset + z * zOffset;
                        cellMap[cellIndex] = new Cell(new Position(x, y, z), Enumerable.Range(0, patternInfo.Length).ToArray(), patternInfo, enablerCountTemplate);
                        if (incompleteMap is not null && incompleteMap.GetEncodedTileAt(x, y, z) != 0)
                        {
                            RemoveBasedOnEncodedTile(cellIndex, incompleteMap.GetEncodedTileAt(x, y, z));
                        }
                    }
                }
            }
            uncollapsedCellIndices = Enumerable.Range(0, width * height * depth).ToList();
        }

        public WfcSolver(InputReader inputReader, int width = -1, int height = -1, int depth = -1, bool deepFirstPropagation = true)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            this.deepFirstPropagation = deepFirstPropagation;

            this.patternInfo = inputReader.GetPatternInfo();
            this.numberOfPatterns = patternInfo.Length;

            removalDictionary = new Dictionary<Position, HashSet<int>>();
            symmetryDictionary = inputReader.symmetryDictionary;

            if (width != -1 && height != -1 && depth != -1)
            {
                InitializeOutputGrid();
            }
        }

        public void SetOutputSize(int width, int height, int depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            InitializeOutputGrid();
        }

        private bool PositionIsValid(Position pos)
        {
            return (pos.x < width && pos.x >= 0) && (pos.y < height && pos.y >= 0) && (pos.z < depth && pos.z >= 0);
        }

        private Position FindLowestEntropyCell()
        {
            int lessEntrophyIndex = uncollapsedCellIndices.Aggregate((current, next) => cellMap[current].entrophy <= cellMap[next].entrophy ? current : next);

            return cellMap[lessEntrophyIndex].position;
        }

        private void CollapseCell(int cellIndex, int patternToCollapse)
        {
            uncollapsedCellIndices.Remove(cellIndex);
            cellMap[cellIndex].CollapseOn(patternToCollapse);
            collapsedCount++;
        }

        private (Position, int) Observe()
        {
            // find cell with minimal entropy;
            Position candidatePosition = FindLowestEntropyCell();
            int collapsedPattern = CollapseBasedOnPatternFrecuency(candidatePosition);
            return (candidatePosition, collapsedPattern);
        }

        private int CollapseBasedOnPatternFrecuency(Position pos)
        {
            int cellIndex = pos.x + pos.y * yOffset + pos.z * zOffset;

            int[] candidatePatternIndices = cellMap[pos.x + pos.y * yOffset + pos.z * zOffset].possiblePatterns.ToArray();
            int numberOfCandidates = candidatePatternIndices.Length;

            float[] candidateFrecuencies = new float[numberOfCandidates];
            float sumOfFrecuencies = 0;
            for (int i = 0; i < numberOfCandidates; i++)
            {
                candidateFrecuencies[i] = patternInfo[candidatePatternIndices[i]].relativeFrecuency;
                sumOfFrecuencies += candidateFrecuencies[i];
            }

            int collapsedIndex = -1;
            double randomValue = random.NextDouble();
            randomValue *= (sumOfFrecuencies);

            for (int i = 0; i < numberOfCandidates; i++)
            {
                if (randomValue < candidateFrecuencies[i])
                {
                    collapsedIndex = i;
                    break;
                }
                randomValue -= candidateFrecuencies[i];
            }

            AddRemovalUpdate(pos, cellMap[cellIndex].possiblePatterns);
            removalDictionary[pos].Remove(candidatePatternIndices[collapsedIndex]);

            CollapseCell(cellIndex, candidatePatternIndices[collapsedIndex]);
            return candidatePatternIndices[collapsedIndex];
        }

        private void AddRemovalUpdate(Position pos, HashSet<int> patternsRemoved)
        {
            if (!removalDictionary.ContainsKey(pos))
            {
                removalDictionary.Add(pos, new HashSet<int>(patternsRemoved));
            }

            removalDictionary[pos].UnionWith(patternsRemoved);
        }

        private void AddRemovalUpdate(Position pos, int patternRemoved)
        {
            if (!removalDictionary.ContainsKey(pos))
            {
                removalDictionary.Add(pos, new HashSet<int>());
            }
            removalDictionary[pos].Add(patternRemoved);
        }


        private void RemoveUncompatiblePatternsInNeighbour(RemovalUpdate removalUpdate, Position neighbourCoord, int direction)
        {
            int neighbourCellIndex = neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset;
            int[,] neighbourEnablers = cellMap[neighbourCellIndex].tileEnablerCountsByDirection;

            List<int> compatiblePatternsToRemove = new();

            foreach (int patternIndex in removalUpdate.patternIndicesRemoved)
            {
                compatiblePatternsToRemove.AddRange(patternInfo[patternIndex].GetCompatiblesInDirection((Direction)direction));
            }

            bool removedPatterns = false;
            foreach (int compatiblePattern in compatiblePatternsToRemove)
            {
                //  int oppositeDirection = (direction + 2) % 4;

                neighbourEnablers[compatiblePattern, direction]--;

                if (neighbourEnablers[compatiblePattern, direction] == 0 &&
                    cellMap[neighbourCellIndex].possiblePatterns.Contains(compatiblePattern))
                {
                    removedPatterns = true;

                    cellMap[neighbourCellIndex].RemovePattern(compatiblePattern, patternInfo);

                    AddRemovalUpdate(neighbourCoord, compatiblePattern);
                    // collapse cell when only one pattern is left
                    if (cellMap[neighbourCellIndex].possiblePatterns.Count == 1)
                    {
                        int lastPattern = cellMap[neighbourCellIndex].possiblePatterns.ToArray()[0];
                        CollapseCell(neighbourCellIndex, lastPattern);
                        break;
                    }
                }
            }

            if (removedPatterns) cellMap[neighbourCellIndex].CalculateEntrophy();
        }


        private RemovalUpdate GetRemovalUpdate()
        {
            KeyValuePair<Position, HashSet<int>> removalUpdate;
            if (deepFirstPropagation)
            {
                removalUpdate = removalDictionary.First();
            }
            else
            {
                removalUpdate = removalDictionary.Last();
            }

            removalDictionary.Remove(removalUpdate.Key);
            return new RemovalUpdate(removalUpdate.Key, removalUpdate.Value);
        }

        private void Propagate()
        {
            int numberOfDirections = Enum.GetValues(typeof(Direction)).Length;

            while (removalDictionary.Count > 0)
            {
                RemovalUpdate removalUpdate = GetRemovalUpdate();

                for (int direction = 0; direction < numberOfDirections; direction++)
                {
                    Position neighbourCoord = removalUpdate.position + Position.directions[direction];
                    if (!PositionIsValid(neighbourCoord) || cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].collapsed) continue;
                    RemoveUncompatiblePatternsInNeighbour(removalUpdate, neighbourCoord, direction);
                }
            }
        }


        public Tilemap Generate(Tilemap incompleteMap)
        {
            InitializeOutputGrid(incompleteMap);

            int cellsToBeCollapsed = width * height * depth;
            collapsedCount = 0;

            // ProcessIncompleteMap(incompleteMap);
            if (removalDictionary.Count != 0) Propagate();

            while (collapsedCount < cellsToBeCollapsed)
            {
                Observe();
                Propagate();
            }
            return GetOutputTileIndexGrid();
        }

        public Tilemap Generate()
        {
            int cellsToBeCollapsed = width * height * depth;
            collapsedCount = 0;


            Stopwatch sw = new();
            sw.Start();
            while (collapsedCount < cellsToBeCollapsed)
            {
                Observe();
                Propagate();
            }
            sw.Stop();
            UnityEngine.Debug.Log(sw.ElapsedMilliseconds);

            return GetOutputTileIndexGrid();
        }

        public Tilemap GetOutputTileIndexGrid()
        {
            Tilemap output = new(symmetryDictionary, width, height, depth);
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int patternIndex = cellMap[x + y * yOffset + z * zOffset].GetCollapsedPatternIndex();
                        output.SetTile(Tile.DecodeTile(patternInfo[patternIndex].GetEncodedTileIndex()),
                                       x, y, z);
                    }
                }
            }
            return output;
        }
    }
}