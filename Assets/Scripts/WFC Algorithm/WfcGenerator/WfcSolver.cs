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
        private Cell[,,] cellMap;
        private PatternInfo[] patternInfo;
        private int numberOfPatterns;
        private int collapsedCount = 0;

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

        public WfcSolver(InputReader inputReader, int width = 0, int height = 0, int depth = 0)
        {
            this.patternSize = inputReader.patternSize;
            this.finalWidth = width;
            this.finalHeight = height;
            this.finalDepth = depth;

            this.width = width+1 - (patternSize);
            this.height = height;// - (patternHeight - 1);
            this.depth = depth+1 - (patternSize);

            this.patternInfo = inputReader.GetPatternInfo();
            this.numberOfPatterns = patternInfo.Length;
            InitializeOutputGrid();
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
                        Cell current = cellMap[i, k, j];
                        if (!current.collapsed && minEntropy > current.entrophy)
                        {
                            minEntropy = current.entrophy;
                            pos = new Position(i, k, j);
                        }
                    }
                }
            }
            return pos;
        }

        private (Position, int) Observe()
        {
            // find cell with minimal entropy;
            Position candidatePosition = FindLowestEntropyCell();
            int removedPattern = CollapseBasedOnPatternFrecuency(candidatePosition.x, candidatePosition.y, candidatePosition.z);
            return (candidatePosition, removedPattern);
        }

        //we can optimize this:
        private int CollapseBasedOnPatternFrecuency(int x, int y, int z)
        {
            int[] candidatePatternIndices = cellMap[x, y, z].possiblePatterns.ToArray();
            int numberOfCandidates = candidatePatternIndices.Length;

            if (numberOfCandidates == 0)
            {
                collapsedCount++;
                cellMap[x, y, z].CollapseOn(0);
                return 0;
            }

            if (numberOfCandidates == 1)
            {
                collapsedCount++;
                cellMap[x, y, z].CollapseOn(candidatePatternIndices[0]);
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
                if (randomValue > candidateFrecuencies[i])
                {
                    randomValue -= candidateFrecuencies[i];
                }
                else { collapsedIndex = i; break; }
            }

            collapsedCount++;

            cellMap[x, y, z].CollapseOn(candidatePatternIndices[collapsedIndex]);
            return candidatePatternIndices[collapsedIndex];
        }

        private bool PositionIsValid(Position pos)
        {
            return (pos.x < width && pos.x >= 0) && (pos.y < height && pos.y >= 0) && (pos.z < depth && pos.z >= 0);
        }

        private void Propagate(Position origin, int collapsedPattern)
        {
            Queue<(Position, int)> removalQueue = new Queue<(Position, int)>();
            removalQueue.Enqueue((origin, collapsedPattern));
            int numberOfDirections = Enum.GetValues(typeof(Direction)).Length;
            while (removalQueue.Count > 0)
            {
                (Position currentPosition, int currentPatternIndex) = removalQueue.Dequeue();

                for (int direction = 0; direction < numberOfDirections; direction++)
                {
                    Position neigbourCoord = currentPosition + Position.directions[direction];
                    if (!PositionIsValid(neigbourCoord)) continue;
                    Cell neighbourCell = cellMap[neigbourCoord.x, neigbourCoord.y, neigbourCoord.z];
                    int[,] neighbourEnablers = neighbourCell.tileEnablerCountsByDirection;
                    HashSet<int> compatiblePatterns = patternInfo[currentPatternIndex].GetCompatiblesInDirection((Direction)direction);

                    foreach (int compatiblePattern in compatiblePatterns)
                    {
                        int oppositeDirection = (direction + 2) % 4;
                        if (neighbourEnablers[currentPatternIndex, direction] == 1)
                        {
                            //check the other directions to see if we have a 0
                            for (int i = 0; i < numberOfDirections; i++)
                            {
                                if (neighbourEnablers[currentPatternIndex, i] == 0)
                                {
                                    neighbourCell.RemovePattern(currentPatternIndex, patternInfo);

                                    //CHECK FOR NO MORE POSSIBLE TILES NOW


                                    removalQueue.Enqueue((neigbourCoord, currentPatternIndex));

                                    break;
                                }
                            }
                        }
                    }
                    neighbourEnablers[currentPatternIndex, direction]--;
                }
            }

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
                    int[,,] pattern = patternInfo[patternIndex].pattern;
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


        public int[,,] Generate()
        {
            int cellsToBeCollapsed = width * height * depth;
            collapsedCount = 0;
            while (collapsedCount < cellsToBeCollapsed)
            {
                (Position candidatePosition, int collapsedPattern) = Observe();
                Propagate(candidatePosition, collapsedPattern);
            }
            return GetOutputTileIndexGrid();
        }
    }
}
