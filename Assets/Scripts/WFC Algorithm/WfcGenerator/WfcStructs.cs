using System;
using System.Collections.Generic;
using System.Text;

namespace WFC_Model
{
    public enum Direction
    {
        south,
        east,
        west,
        north,
        up,
        down
    }

    public enum SymmetryType
    {
        ERR,
        X,
        T,
        I,
        L,
        D   //diagonal
    }

    [Serializable]
    public struct Tile
    {
        public int id;
        public int rotation;
        public SymmetryType symmetry;
        public bool reflected;


        public Tile(int id = 0, int rotation = 0, SymmetryType stype = SymmetryType.ERR, bool isReflected = false)
        {
            this.id = id;
            this.rotation = rotation;
            this.symmetry = stype;
            this.reflected = isReflected;

        }

        public void Set(int id, int rotation, SymmetryType stype)
        {
            this.id = id;
            this.rotation = rotation;
            this.symmetry = stype;
        }

        public int RotateClockwise()
        {
            rotation = (rotation + 1) % 4;
            return rotation;
        }

        public void Reflect()
        {
            reflected = !reflected;
        }

        private static int mod(int x, int y)
        {
            return x - y * (int)Math.Floor((double)x / y);
        }

        private static int EncodeRotationAndRotation(Tile tile)
        {

            if (tile.symmetry == SymmetryType.X)
            {
                return 0;
            }

            int output = 0;

            if (tile.symmetry == SymmetryType.D || tile.symmetry == SymmetryType.I)
            {
                output = mod(tile.rotation, 2);
            }
            else
            {
                output = mod(tile.rotation, 4);
            }
            
            return output | ((tile.reflected) ? (1 << 31) : 0);
        }


        public static int EncodeTile(Tile tile)
        {
            int encoded = (tile.id * 4 + EncodeRotationAndRotation(tile)) ;

            string binary = Convert.ToString(encoded, 2).PadLeft(32, '0');

            return encoded;
        }
       
        public static int DecodeTileId(int encodedTile)
        {
            int lastBitCleared = encodedTile & ~(1 << 31);
            return lastBitCleared / 4;
        }

        public static int DecodeTileRotation(int encodedTile)
        {
            int lastBitCleared = encodedTile & ~(1 << 31);
            return ((lastBitCleared - (lastBitCleared / 4) * 4)) % 4;
        }

        public static bool DecodeReflection(int encodedTile)
        {
            bool output = ((encodedTile >> 31) & 1) == 1;
            string binary = Convert.ToString(encodedTile, 2).PadLeft(32, '0');

            return output;
        }

        public static Tile DecodeTile(int encodedTile, Dictionary<int, SymmetryType> symmetryDictionary)
        {
            int id = DecodeTileId(encodedTile);
            return new Tile(id, DecodeTileRotation(encodedTile), symmetryDictionary[id], DecodeReflection(encodedTile));
        }
    }

    public struct Position
    {
        public int x; public int y; public int z;


        public static Position[] directions = new Position[]{
                                            new Position(0, 0, -1),
                                            new Position(-1, 0, 0),
                                            new Position(1, 0, 0),
                                            new Position(0, 0, 1),
                                            new Position(0, 1, 0),
                                            new Position(0, -1, 0),
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
        public HashSet<int> patternIndicesRemoved;
        public Position position;

        public RemovalUpdate(Position position, HashSet<int> patternIndicesRemoved)
        {
            this.patternIndicesRemoved = patternIndicesRemoved;
            this.position = position;
        }
    }

    public struct Cell : IComparable<Cell>
    {
        public Position position;
        public HashSet<int> possiblePatterns;
        //first index is the pattern, second is the direction
        public int[,] tileEnablerCountsByDirection;
        public float entrophy;
        public bool collapsed;

        float sumOfRelativeFreq;
        float sumOfRelativeFreqLog2;
        int collapsedIndex;

        public Cell(Position pos, int[] possiblePatterns, in PatternInfo[] patternInfo, in int[,] tileEnablerTemplate, int collapsedValue = -1)
        {
            this.position = pos;
            this.possiblePatterns = new HashSet<int>(possiblePatterns);
            sumOfRelativeFreq = 0;
            sumOfRelativeFreqLog2 = 0;
            entrophy = 0;
            this.collapsedIndex = collapsedValue;
            collapsed = false;
            for (int i = 0; i < possiblePatterns.Length; i++)
            {
                sumOfRelativeFreq += patternInfo[possiblePatterns[i]].relativeFrecuency;
                sumOfRelativeFreqLog2 += patternInfo[possiblePatterns[i]].freqTimesFreqLog2;
            }
            tileEnablerCountsByDirection = new int[tileEnablerTemplate.GetLength(0), tileEnablerTemplate.GetLength(1)];
            Buffer.BlockCopy(tileEnablerTemplate, 0, tileEnablerCountsByDirection, 0, tileEnablerCountsByDirection.Length * sizeof(int));
            CalculateEntrophy();
        }

        public int GetCollapsedPatternIndex()
        {
            //TO FIX?¿
            return collapsedIndex == -1 ? 0 : collapsedIndex;
        }

        public void CollapseOn(int patternToCollapse)
        {
            entrophy = float.MaxValue;
            collapsed = true;
            collapsedIndex = patternToCollapse;
        }

        private void RemoveFrequencies(int removedPatternIndex, in PatternInfo[] patternInfo)
        {
            sumOfRelativeFreq -= patternInfo[removedPatternIndex].relativeFrecuency;
            sumOfRelativeFreqLog2 -= patternInfo[removedPatternIndex].freqTimesFreqLog2;
        }

        private void CalculateEntrophy()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            entrophy = (float)(Math.Log(sumOfRelativeFreq, 2) - (sumOfRelativeFreqLog2 / sumOfRelativeFreq) + (rand.NextDouble() * 0.001f));
        }

        public void RemovePattern(int patternIndex, in PatternInfo[] patternInfo)
        {
            if (possiblePatterns.Contains(patternIndex))
            {
                possiblePatterns.Remove(patternIndex);
                RemoveFrequencies(patternIndex, patternInfo);
                CalculateEntrophy();
            }
        }

        public override string ToString()
        {
            return entrophy.ToString("0.0");
        }

        public int CompareTo(Cell other)
        {
            if (entrophy < other.entrophy) return -1;
            if (entrophy == other.entrophy) return 0;
            return 1;
        }

    }


    /// <summary>
    /// Contains all of the information necessary to define a pattern.
    /// </summary>
    public struct PatternInfo
    {
        public int id;
        public int frecuency;
        public float relativeFrecuency;
        public float relativeFrecuencyLog2;
        public float freqTimesFreqLog2;
        public int[] pattern;
        public int patternSize;
        public int patternHeight;
        public int patternRotation;

        public Dictionary<Direction, HashSet<int>> neigbourIndices;


        public PatternInfo(int patternId, int[] pattern, int patternSize, int patternHeight, int frecuency = 0, int patternRotation = 0)
        {
            this.pattern = pattern;
            this.id = patternId;
            this.patternHeight = patternHeight;
            this.patternSize = patternSize;
            this.frecuency = frecuency;
            this.patternRotation = patternRotation;
            relativeFrecuency = 0;
            relativeFrecuencyLog2 = 0;
            freqTimesFreqLog2 = 0;
            neigbourIndices = new Dictionary<Direction, HashSet<int>>
            {
                { Direction.north, new HashSet<int>() },
                { Direction.south, new HashSet<int>() },
                { Direction.west, new HashSet<int>() },
                { Direction.east, new HashSet<int>() },
                { Direction.up, new HashSet<int>() },
                { Direction.down, new HashSet<int>() }
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

        internal void UpdateFrecuencies(float totalPatterns)
        {
            relativeFrecuency = frecuency / totalPatterns;
            relativeFrecuencyLog2 = (float)(Math.Log(relativeFrecuency, 2));
            freqTimesFreqLog2 = relativeFrecuency * relativeFrecuencyLog2;
        }

        public int GetEncodedTileIndex()
        {
            return pattern[0];
        }

        public override string ToString()
        {
            return string.Join(".", pattern);
        }

    }
}