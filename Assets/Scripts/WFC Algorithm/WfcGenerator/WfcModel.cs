using System;
using System.Collections.Generic;
using System.Linq;

namespace WFC_Procedural_Generator_Framework
{


    public class WfcModel
    {
        int width;
        int height;
        int depth;

        private Random random = new Random();
        private Cell[,,] outputGrid;
        private PatternInfo[] patterns;
        private int numberOfPatterns;
        private int[,] InitialEnablerCount()
        {
            int numberOfDirections = Enum.GetValues(typeof(Direction)).Length;
            int[,] result = new int[numberOfPatterns, numberOfDirections];

            for (int patternIndex = 0; patternIndex < numberOfPatterns; patternIndex++)
            {
                for (int direction = 0; direction < numberOfDirections; direction++)
                {
                    HashSet<int> compatibles = patterns[patternIndex].GetCompatiblesInDirection((Direction)direction);
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

            outputGrid = new Cell[width, height, depth];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        outputGrid[i, k, j] = new Cell(Enumerable.Range(0, patterns.Length).ToArray(), patterns, enablerCountTemplate);
                    }
                }
            }
        }

        public WfcModel(InputReader inputReader, int width = 0, int height = 0, int depth = 0)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.patterns = inputReader.GetPatternInfo();
            this.numberOfPatterns = patterns.Length;
            InitializeOutputGrid();
        }

        private Position FindLowestEntropyCell()
        {
            float minEntropy = float.MaxValue;
            int[] position = new int[3];
            //for now, lineal search, must optimize later
            for (int i = 0; i <= width; i++)
            {
                for (int j = 0; j <= depth; j++)
                {
                    for (int k = 0; k <= height; k++)
                    {
                        Cell current = outputGrid[i, k, j];
                        if (!current.collapsed && minEntropy > current.entrophy)
                        {
                            minEntropy = current.entrophy;
                            position = new int[] { i, k, j };
                        }
                    }
                }
            }
            return new Position(position[0], position[1], position[2]);
        }


        private Position Observe()
        {
            // find cell with minimal entropy;
            Position candidatePosition = FindLowestEntropyCell();
            CollapseBasedOnPatternFrecuency(candidatePosition.x, candidatePosition.y, candidatePosition.z);
            return candidatePosition;
        }

        //we can optimize this:
        private void CollapseBasedOnPatternFrecuency(int x, int y, int z)
        {
            int[] candidateIndices = outputGrid[x, y, z].possiblePatterns.ToArray();
            int numberOfCandidates = candidateIndices.Length;
            if (numberOfCandidates <= 1)
            {
                return;
            }

            float[] candidateFrecuencies = new float[numberOfCandidates];
            float sumOfFrecuencies = 0;
            for (int i = 0; i < numberOfCandidates; i++)
            {
                candidateFrecuencies[i] = patterns[candidateIndices[i]].relativeFrecuency;
                sumOfFrecuencies += candidateFrecuencies[i];
            }

            int collapsedPattern = -1;

            double acc = 0;
            double randomValue = random.NextDouble();
            for (int i = 0; i < numberOfCandidates; i++)
            {
                acc += candidateFrecuencies[i];
                if (randomValue <= acc)
                {
                    collapsedPattern = i;
                    break;
                }
            }
            outputGrid[x, y, z].CollapseOn(collapsedPattern);
        }

        private void Propagate(Position origin)
        {

        }

        public void Generate()
        {
            Position candidatePosition = Observe();
            Propagate(candidatePosition);
        }
    }
}
