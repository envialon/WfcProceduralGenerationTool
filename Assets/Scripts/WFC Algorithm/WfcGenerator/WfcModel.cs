using System;
using System.Collections.Generic;
using System.Linq;

namespace WFC_Procedural_Generator_Framework
{
    public struct Position
    {
        public int x; public int y; public int z;
        public Position(int x = 0, int y = 0, int z = 0)
        {
            this.x = x; this.y = y; this.z = z;
        }
    }

    public struct RemovalUpdate
    {
        int patternIndex;
        Position position;

        public RemovalUpdate(Position position, int patternIndex)
        {
            this.patternIndex = patternIndex;
            this.position = position;
        }
    }

    public struct Cell
    {
        public HashSet<int> possiblePatterns;
        public float entrophy;
        public bool collapsed;

        float sumOfPatternWeights;
        float sumOfPatternLogWeights;



        public Cell(int[] possiblePatterns, PatternInfo[] patternInfo)
        {
            this.possiblePatterns = new HashSet<int>(possiblePatterns);
            sumOfPatternWeights = 0;
            sumOfPatternLogWeights = 0;
            entrophy = 0;
            collapsed = false;
            for (int i = 0; i < possiblePatterns.Length; i++)
            {
                sumOfPatternWeights += patternInfo[possiblePatterns[i]].relativeFrecuency;
                //im doing it different from the rust resource, might be an error:
                sumOfPatternLogWeights += patternInfo[possiblePatterns[i]].relativeFrecuencyLog2;
            }
            CalculateEntrophy();
        }
        
        public void CollapseOn(int patternToCollapse)
        {
            possiblePatterns.RemoveWhere(x => x != patternToCollapse);
            entrophy = 0;
            collapsed = true;
        }

        private void CalculateEntrophy()
        {
            entrophy = (float)(Math.Log(sumOfPatternWeights, 2) - (sumOfPatternLogWeights / sumOfPatternWeights));
        }

        public void RemovePattern(int patternIndex, PatternInfo[] patternInfo)
        {
            possiblePatterns.Remove(patternIndex);
            sumOfPatternLogWeights -= patternInfo[patternIndex].relativeFrecuencyLog2;
            sumOfPatternWeights -= patternInfo[patternIndex].relativeFrecuency;
            CalculateEntrophy();
        }
    }

    public class WfcModel
    {
        int width;
        int height;
        int depth;

        private Random random = new Random();

        //eventually change to bitwise operations
        Cell[,,] outputGrid;

        public PatternInfo[] patterns;




        private void InitializeOutputGrid()
        {
            outputGrid = new Cell[width, height, depth];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        outputGrid[i, k, j] = new Cell(Enumerable.Range(0, patterns.Length).ToArray(), patterns);
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

        private void Propagation(Position origin)
        {
            Queue<Position> queue = new Queue<Position>();

            int remaining = 1;
            while (remaining > 0)
            {
                Position current = queue.Peek();
                queue.Dequeue();
            }
        }


        public void Generate()
        {
            Position candidatePosition = Observe();
            Propagation(candidatePosition);
        }


    }
}
