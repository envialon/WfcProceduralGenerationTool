using System;
using System.Collections.Generic;

namespace WFC_Procedural_Generator_Framework
{
    public enum Direction
    {
        north,
        east,
        south,
        west,
    }

    public struct Position
    {
        public int x; public int y; public int z;


        public static Position[] directions = new Position[]{
                                            new Position(0, 0, 1),
                                            new Position(1, 0, 0),
                                            new Position(0, 0, -1),
                                            new Position(-1, 0, 0),
                                            };
        public Position(int x = 0, int y = 0, int z = 0)
        {
            this.x = x; this.y = y; this.z = z;
        }
        public static Position operator +(Position left, Position right)
        {
            return new Position(left.x + right.x, left.y + right.y, left.z + right.z);
        }

        public override string ToString()
        {
            return "{" + x + ", " + y + ", " + z + "}";
        }


    }

    public struct RemovalUpdate
    {
        public int patternIndex;
        public Position position;

        public RemovalUpdate(Position position, int patternIndex)
        {
            this.patternIndex = patternIndex;
            this.position = position;
        }

        public override string ToString()
        {
            return position.ToString() + ", " + patternIndex;
        }
    }

    public class Cell : IComparable
    {
        public HashSet<int> possiblePatterns;
        //first index is the pattern, second is the direction
        public int[,] tileEnablerCountsByDirection;
        public float entrophy;
        public bool collapsed;

        float sumOfRelativeFreq;
        float sumOfRelativeFreqLog2;
        int collapsedIndex;

        public Cell(int[] possiblePatterns, PatternInfo[] patternInfo, int[,] tileEnablerTemplate, int collapsedValue = -1)
        {
            this.possiblePatterns = new HashSet<int>(possiblePatterns);
            sumOfRelativeFreq = 0;
            sumOfRelativeFreqLog2 = 0;
            entrophy = 0;
            this.collapsedIndex = collapsedValue;
            collapsed = false;
            for (int i = 0; i < possiblePatterns.Length; i++)
            {
                float freq = patternInfo[possiblePatterns[i]].relativeFrecuency;
                sumOfRelativeFreq += freq;
                //im doing it different from the rust resource, might be an error:
                sumOfRelativeFreqLog2 += freq * (float)Math.Log(freq, 2);
            }
            tileEnablerCountsByDirection = new int[tileEnablerTemplate.GetLength(0), tileEnablerTemplate.GetLength(1)];
            Buffer.BlockCopy(tileEnablerTemplate, 0, tileEnablerCountsByDirection, 0, tileEnablerCountsByDirection.Length * sizeof(int));
            CalculateEntrophy();
        }

        public int GetCollapsedIndex()
        {
            return collapsedIndex;
        }

        public void CollapseOn(int patternToCollapse)
        {
            possiblePatterns.RemoveWhere(x => x != patternToCollapse);
            entrophy = 0;
            collapsed = true;
            collapsedIndex = patternToCollapse;
            collapsedIndex = patternToCollapse;
        }

        private void CalculateSumOfRelativeFrecuencies(PatternInfo[] patternInfo)
        {
            sumOfRelativeFreq = 0;
            foreach (int i in possiblePatterns)
            {
                float freq = patternInfo[i].relativeFrecuency;
                sumOfRelativeFreq += freq;
            }
        }

        private void CalculateSumOfPatternLogWeights(PatternInfo[] patternInfo)
        {
            sumOfRelativeFreqLog2 = 0;
            foreach (int i in possiblePatterns)
            {
                float freq = patternInfo[i].relativeFrecuency;
                sumOfRelativeFreqLog2 += freq * (float)Math.Log(freq, 2);
            }
        }
        private void CalculateEntrophy()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            entrophy = (float)(Math.Log(sumOfRelativeFreq, 2) - (sumOfRelativeFreqLog2 / sumOfRelativeFreq) + (rand.NextDouble() * 0.001f));
        }

        public void RemovePattern(int patternIndex, PatternInfo[] patternInfo)
        {
            if (possiblePatterns.Contains(patternIndex))
            {
                possiblePatterns.Remove(patternIndex);
                //float freq = patternInfo[patternIndex].relativeFrecuency;
                //sumOfRelativeFreq -= freq;
                //sumOfPatternLogWeights -= patternInfo[patternIndex].relativeFrecuencyLog2; // might be worth to cach freq * log(freq,2) somewhere

                CalculateSumOfRelativeFrecuencies(patternInfo);
                CalculateSumOfPatternLogWeights(patternInfo);
                CalculateEntrophy();
            }
        }

        public bool ContainsAnyZeroEnablerCount(int compatiblePattern)
        {            
            return  tileEnablerCountsByDirection[compatiblePattern, 0] == 0 ||
                    tileEnablerCountsByDirection[compatiblePattern, 1] == 0 ||
                    tileEnablerCountsByDirection[compatiblePattern, 2] == 0 ||
                    tileEnablerCountsByDirection[compatiblePattern, 3] == 0;
        }

        int IComparable.CompareTo(object o)
        {
            Cell other = o as Cell;
            if (entrophy < other.entrophy) return -1;
            if (entrophy == other.entrophy) return 0;
            return 1;
        }
        public override string ToString()
        {
            return entrophy.ToString("0.0");
        }

    }

    /// <summary>
    /// Contains all of the pattern information extracted from the input.
    /// </summary>
    public struct PatternInfo
    {
        public int id;
        public int frecuency;
        public float relativeFrecuency;
        public float relativeFrecuencyLog2;
        public int[,,] pattern;

        public Dictionary<Direction, HashSet<int>> neigbourIndices;

        public PatternInfo(int[,,] pattern, int patternId)
        {
            this.pattern = pattern;
            this.id = patternId;
            frecuency = 0;
            relativeFrecuency = 0;
            relativeFrecuencyLog2 = 0;
            neigbourIndices = new Dictionary<Direction, HashSet<int>>
            {
                { Direction.north, new HashSet<int>() },
                { Direction.south, new HashSet<int>() },
                { Direction.west, new HashSet<int>() },
                { Direction.east, new HashSet<int>() }
            };
        }

        public static PatternInfo operator ++(PatternInfo patternInfo)
        {
            patternInfo.frecuency++;
            return patternInfo;
        }

        public HashSet<int> GetCompatiblesInDirection(Direction direction)
        {
            return neigbourIndices[direction];
        }

        public override int GetHashCode()
        {
            string digits = "";
            foreach (int i in pattern)
            {
                digits += i;
            }
            int output = int.Parse(digits);
            return int.Parse(digits);
        }

        internal void UpdateFrecuencies(float totalPatterns)
        {
            relativeFrecuency = frecuency / totalPatterns;
            relativeFrecuencyLog2 = (float)(Math.Log(relativeFrecuency, 2));
        }
    }
}