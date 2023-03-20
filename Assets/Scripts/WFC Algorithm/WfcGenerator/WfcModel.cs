using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;

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

    public class WfcModel
    {
        int width;
        int height;
        int depth;
        bool trained = false;
        private Random random = new Random();

        //eventually change to bitwise operations
        HashSet<int>[,,] outputGrid;

        public PatternInfo[] patterns;
        public float[,,] entropyGrid;
        public bool[,,] collapsed;

        private void InitializeOutputGrid()
        {
            outputGrid = new HashSet<int>[width, height, depth];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        outputGrid[i, k, j] = new HashSet<int>();
                        outputGrid[i, k, j].UnionWith(Enumerable.Range(0, patterns.Length));
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
            collapsed = new bool[width, height, depth];
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
                        if (minEntropy > entropyGrid[i, k, j] && !collapsed[i, k, j])
                        {
                            minEntropy = entropyGrid[i, k, j];
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

        private void CollapseBasedOnPatternFrecuency(int x, int y, int z)
        {
            int[] candidateIndices = outputGrid[x, y, z].ToArray();
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

            outputGrid[x, y, z].RemoveWhere(x => x != collapsedPattern);
            entropyGrid[x, y, z] = 0;
            collapsed[x, y, z] = true;
        }

   
        private void Propagation(Position origin)
        {
            List<Position> propagationList = new List<Position> { origin };
            HashSet<Position> propagated = new HashSet<Position>();
            int remaining = 1; 
            while (remaining >0)
            {
                Position current = propagationList[0];
                propagationList.RemoveAt(0);
                propagated.Add(current);   
                

            }
        }


        public void Generate()
        {
            Position candidatePosition = Observe();
            Propagation(candidatePosition);
        }


    }
}
