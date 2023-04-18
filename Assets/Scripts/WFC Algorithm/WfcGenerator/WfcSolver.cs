using System;
using System.Collections.Generic;
using System.Linq;

namespace WFC_Procedural_Generator_Framework
{


    public class WfcSolver
    {
        int width;
        int height;
        int depth;

        int finalWidth;
        int finalHeight;
        int finalDepth;

        int patternSize;



        private Random random = new Random();
        public Cell[,,] cellMap;
        private PatternInfo[] patternInfo;
        private int numberOfPatterns;
        private int collapsedCount = 0;

        private Queue<(Position, int)> removalQueue;


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

            cellMap = new Cell[width, height, depth];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        cellMap[i, k, j] = new Cell(Enumerable.Range(0, patternInfo.Length).ToArray(), patternInfo, enablerCountTemplate);
                    }
                }
            }
        }

        public WfcSolver(InputReader inputReader, int width = 1, int height = 1, int depth = 1)
        {
            this.patternSize = inputReader.patternSize;
            this.finalWidth = width;
            this.finalHeight = height;
            this.finalDepth = depth;

            this.width = width + 1 - (patternSize);
            this.height = height;// - (patternHeight - 1);
            this.depth = depth + 1 - (patternSize);
            
            this.patternInfo = inputReader.GetPatternInfo();
            this.numberOfPatterns = patternInfo.Length;

            removalQueue = new Queue<(Position, int)>();

            InitializeOutputGrid();
        }

        public void SetOutputSize(int width, int height, int depth)
        {
            this.finalWidth = width;
            this.finalHeight = height;
            this.finalDepth = depth;

            this.width = width + 1 - (patternSize);
            this.height = height;// - (patternHeight - 1);
            this.depth = depth + 1 - (patternSize);
            
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
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        if (!cellMap[i, k, j].collapsed && 
                            minEntropy > cellMap[i, k, j].entrophy)
                        {
                            minEntropy = cellMap[i, k, j].entrophy;
                            pos = new Position(i, k, j);
                        }
                    }
                }
            }
            return pos;
        }

        private void PrintCellEntrophy()
        {
            string msg = "";
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        msg += cellMap[i, k, j].ToString() + " ";
                    }
                }
                msg += "\n";
            }
            UnityEngine.Debug.Log(msg);
        }


        private (Position, int) Observe()
        {
            // find cell with minimal entropy;
            Position candidatePosition = FindLowestEntropyCell();
            int removedPattern = CollapseBasedOnPatternFrecuency(candidatePosition);
            return (candidatePosition, removedPattern);
        }

        //we can optimize this:
        private int CollapseBasedOnPatternFrecuency(Position pos)
        {
            int[] candidatePatternIndices = cellMap[pos.x, pos.y, pos.z].possiblePatterns.ToArray();
            int numberOfCandidates = candidatePatternIndices.Length;

            if (numberOfCandidates == 0)
            {
                collapsedCount++;
                cellMap[pos.x, pos.y, pos.z].CollapseOn(0);
                return 0;
            }

            if (numberOfCandidates == 1)
            {
                collapsedCount++;
                cellMap[pos.x, pos.y, pos.z].CollapseOn(candidatePatternIndices[0]);
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
                        //Add removal updates to the queue
                        removalQueue.Enqueue((pos, candidatePatternIndices[i]));

                    }
                    else { collapsedIndex = i; }
                }
                else
                {
                    removalQueue.Enqueue((pos, candidatePatternIndices[i]));
                }
            }

            collapsedCount++;

            cellMap[pos.x, pos.y, pos.z].CollapseOn(candidatePatternIndices[collapsedIndex]);
            return candidatePatternIndices[collapsedIndex];
        }


        private void Propagate()
        {
            int numberOfDirections = Enum.GetValues(typeof(Direction)).Length;

            string msg = "Propagaation Function call:\n";
            while (removalQueue.Count > 0)
            {
                (Position currentPosition, int removedPatternIndex) = removalQueue.Dequeue();

                msg += $"\tRemoved pattern {removedPatternIndex} from cell {currentPosition.x}, {currentPosition.y}, {currentPosition.z}\n";

                for (int direction = 0; direction < numberOfDirections; direction++)
                {
                    Position neigbourCoord = currentPosition + Position.directions[direction];
                    if (!PositionIsValid(neigbourCoord) || cellMap[neigbourCoord.x, neigbourCoord.y, neigbourCoord.z].collapsed) continue;
                    int[,] neighbourEnablers = cellMap[neigbourCoord.x, neigbourCoord.y, neigbourCoord.z].tileEnablerCountsByDirection;
                    HashSet<int> compatiblePatterns = patternInfo[removedPatternIndex].GetCompatiblesInDirection((Direction)direction);

                    foreach (int compatiblePattern in compatiblePatterns)
                    {
                        int oppositeDirection = (direction + 2) % 4;
                        if (neighbourEnablers[compatiblePattern, direction] == 1)
                        {
                            //check the other directions to see if we have a 0
                            for (int i = 0; i < numberOfDirections; i++)
                            {
                                if (neighbourEnablers[compatiblePattern, i] == 0)
                                {
                                    cellMap[neigbourCoord.x, neigbourCoord.y, neigbourCoord.z].RemovePattern(compatiblePattern, patternInfo);
                                    //CHECK FOR NO MORE POSSIBLE TILES NOW
                                    removalQueue.Enqueue((neigbourCoord, compatiblePattern));

                                    break;
                                }
                            }
                        }
                        neighbourEnablers[compatiblePattern, direction]--;
                    }
                }
            }
            UnityEngine.Debug.Log(msg);
        }


        public int[,,] Generate()
        {
            int cellsToBeCollapsed = width * height * depth;
            collapsedCount = 0;

            PrintCellEntrophy();
            while (collapsedCount < cellsToBeCollapsed)
            {
                (Position candidatePosition, int collapsedPattern) = Observe();
                UnityEngine.Debug.Log($"Collapsed cell {candidatePosition} with pattern {collapsedPattern}");
                Propagate();
                PrintCellEntrophy();
            }
            return GetOutputTileIndexGrid();
        }

        public int[,,] Iterate()
        {
            Observe();
            Propagate();
            return GetOutputTileIndexGrid();
        }

        public int[,,] GetPatternGridOutOfOutputGrid()
        {
            int[,,] patternGrid = new int[width, height, depth];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        patternGrid[i, k, j] = cellMap[i, k, j].GetCollapsedIndex();
                    }
                }
            }
            return patternGrid;
        }

        public int[,,] GetOutputTileIndexGrid()
        {
            int[,,] output = new int[finalWidth, finalHeight, finalDepth];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    int patternIndex = cellMap[x, 0, z].GetCollapsedIndex();
                    int[,,] pattern;
                    if (patternIndex < 0)
                    {
                        pattern = new int[patternSize, 1, patternSize];
                        for (int i = 0; i < patternSize; i++)
                        {
                            for (int j = 0; j < patternSize; j++)
                            {
                                pattern[i, 0, j] = -1;
                            }
                        }
                    }
                    else
                    {
                        pattern = patternInfo[patternIndex].pattern;
                    }
                    for (int i = 0; i < patternSize; i++)
                    {
                        for (int j = 0; j < patternSize; j++)
                        {
                            output[x + i, 0, z + j] = pattern[i, 0, j];
                        }
                    }
                }
            }
            return output;
        }


    }
}
