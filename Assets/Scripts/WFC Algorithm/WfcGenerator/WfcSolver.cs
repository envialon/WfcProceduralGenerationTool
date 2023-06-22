using System;
using System.Collections.Generic;
using System.Linq;
namespace WFC_Model
{
    // depht or wide mode 
    public class WfcSolver
    {
        public int width;
        public int height;
        public int depth;

        private Random random = new Random();
        public Cell[] cellMap;



        private PatternInfo[] patternInfo;
        private int numberOfPatterns;
        private int collapsedCount = 0;

        private int yOffset;
        private int zOffset;

        private Queue<RemovalUpdate> removalQueue;

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

        private void InitializeOutputGrid()
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
                        cellMap[x + y * yOffset + z * zOffset] = new Cell(Enumerable.Range(0, patternInfo.Length).ToArray(), patternInfo, enablerCountTemplate);
                    }
                }
            }
        }

        public WfcSolver(InputReader inputReader, int width = -1, int height = -1, int depth = -1)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            this.patternInfo = inputReader.GetPatternInfo();
            this.numberOfPatterns = patternInfo.Length;

            removalQueue = new Queue<RemovalUpdate>();

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
            float minEntropy = float.MaxValue;
            Position pos = new Position();
            //for now, lineal search, must optimize later
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (!cellMap[x + y * yOffset + z * zOffset].collapsed &&
                            minEntropy > cellMap[x + y * yOffset + z * zOffset].entrophy)
                        {
                            minEntropy = cellMap[x + y * yOffset + z * zOffset].entrophy;
                            pos = new Position(x, y, z);
                        }
                    }
                }
            }
            return pos;
        }

        private void PrintCellEntrophy()
        {
            string msg = "";
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        msg += cellMap[x + y * yOffset + z * zOffset].ToString() + " ";
                    }
                }
                msg += "\n";
            }
            //UnityEngine.Debug.Log(msg);
        }

        private (Position, int) Observe()
        {
            // find cell with minimal entropy;
            Position candidatePosition = FindLowestEntropyCell();
            int collapsedPattern = CollapseBasedOnPatternFrecuency(candidatePosition);
            return (candidatePosition, collapsedPattern);
        }

        //we can optimize this:
        private int CollapseBasedOnPatternFrecuency(Position pos)
        {
            int[] candidatePatternIndices = cellMap[pos.x + pos.y * yOffset + pos.z * zOffset].possiblePatterns.ToArray();
            int numberOfCandidates = candidatePatternIndices.Length;


            if (numberOfCandidates == 0)
            {
                collapsedCount++;
                cellMap[pos.x + pos.y * yOffset + pos.z * zOffset].CollapseOn(0);
                return 0;
            }

            if (numberOfCandidates == 1)
            {
                collapsedCount++;
                cellMap[pos.x + pos.y * yOffset + pos.z * zOffset].CollapseOn(candidatePatternIndices[0]);
                return candidatePatternIndices[0];
            }

            float[] candidateFrecuencies = new float[numberOfCandidates];
            float sumOfFrecuencies = 0;
            for (int i = 0; i < numberOfCandidates; i++)
            {
                candidateFrecuencies[i] = patternInfo[candidatePatternIndices[i]].relativeFrecuency;
                sumOfFrecuencies += candidateFrecuencies[i];
            }

            int collapsedIndex = -1;

            double randomValue = random.NextDouble();
            randomValue = randomValue * (sumOfFrecuencies);
            for (int i = 0; i < numberOfCandidates; i++)
            {
                if (collapsedIndex < 0)
                {
                    if (randomValue > candidateFrecuencies[i])
                    {
                        randomValue -= candidateFrecuencies[i];
                        removalQueue.Enqueue(new RemovalUpdate(pos, candidatePatternIndices[i]));
                    }
                    else { collapsedIndex = i; }
                }
                else
                {
                    removalQueue.Enqueue(new RemovalUpdate(pos, candidatePatternIndices[i]));
                }
            }

            collapsedCount++;

            cellMap[pos.x + pos.y * yOffset + pos.z * zOffset].CollapseOn(candidatePatternIndices[collapsedIndex]);
            return candidatePatternIndices[collapsedIndex];
        }

        private void RemoveUncompatiblePatternsInNeighbour(RemovalUpdate removalUpdate, Position neighbourCoord, int direction)
        {
            int[,] neighbourEnablers = cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].tileEnablerCountsByDirection;
            HashSet<int> compatiblePatterns = patternInfo[removalUpdate.patternIndex].GetCompatiblesInDirection((Direction)direction);

            foreach (int compatiblePattern in compatiblePatterns)
            {
                int oppositeDirection = (direction + 2) % 4;

                //We must remove in the opossite direction from the pov of the neighbour cell.
                neighbourEnablers[compatiblePattern, direction]--;

                if (neighbourEnablers[compatiblePattern, direction] == 0 &&
                    cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].possiblePatterns.Contains(compatiblePattern))
                {

                    cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].RemovePattern(compatiblePattern, patternInfo);

                    // collapse cell when only one pattern is left
                    if (cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].possiblePatterns.Count == 1)
                    {
                        int lastPattern = cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].possiblePatterns.ToArray()[0];
                        cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].CollapseOn(lastPattern);
                        collapsedCount++;
                    }

                    removalQueue.Enqueue(new RemovalUpdate(neighbourCoord, compatiblePattern));
                }
            }
        }

        private void Propagate()
        {
            int numberOfDirections = Enum.GetValues(typeof(Direction)).Length;

            string msg = "Propagation Function call:\n";
            while (removalQueue.Count > 0)
            {
                RemovalUpdate removalUpdate = removalQueue.Dequeue();

                msg += $"\tRemoved pattern {removalUpdate.patternIndex} from cell {removalUpdate.position.x}, {removalUpdate.position.y}, {removalUpdate.position.z}\n";

                for (int direction = 0; direction < numberOfDirections; direction++)
                {
                    Position neighbourCoord = removalUpdate.position + Position.directions[direction];

                    if (!PositionIsValid(neighbourCoord) || cellMap[neighbourCoord.x + neighbourCoord.y * yOffset + neighbourCoord.z * zOffset].collapsed) continue;

                    RemoveUncompatiblePatternsInNeighbour(removalUpdate, neighbourCoord, direction);
                }
            }
            //UnityEngine.Debug.Log(msg);
        }

        public void ManualCollapse(Position pos, int encodedTile)
        {
            //get all possible patterns that place that tile
            //remove all of the patterns that don't place that tile
            //propagate
        }

        public Tilemap Generate()
        {
            int cellsToBeCollapsed = width * height * depth;
            collapsedCount = 0;

            //PrintCellEntrophy();
            while (collapsedCount < cellsToBeCollapsed)
            {
                (Position candidatePosition, int collapsedPattern) = Observe();
                // UnityEngine.Debug.Log($"Collapsed cell {candidatePosition} with pattern {collapsedPattern}");
                Propagate();
                //PrintCellEntrophy();
            }
            return GetOutputTileIndexGrid();
        }

        public Tilemap GetOutputTileIndexGrid()
        {
            Tilemap output = new Tilemap(width, height, depth);
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int patternIndex = cellMap[x + y * yOffset + z * zOffset].GetCollapsedPatternIndex();
                        output.SetTile(InputReader.DecodeTile(patternInfo[patternIndex].GetEncodedTileIndex()),
                                       x, y, z);
                    }
                }
            }
            return output;
        }
    }
}