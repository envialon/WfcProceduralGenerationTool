using System;
using System.Collections.Generic;

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
        none,
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
        public bool reflected;


        public Tile(int id = 0, int rotation = 0, bool isReflected = false)
        {
            this.id = id;
            this.rotation = rotation;
            this.reflected = isReflected;

        }

        public void Set(int id, int rotation)
        {
            this.id = id;
            this.rotation = rotation;
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

        private static int Mod(int x, int y)
        {
            return x - y * (int)Math.Floor((double)x / y);
        }

        private static int EncodeRotationAndReflection(Tile tile, in Dictionary<int, SymmetryType> symmetryDictionary)
        {
            SymmetryType symmetry = symmetryDictionary[tile.id];

            if (symmetry == SymmetryType.X)
            {
                return 0;
            }

            int output;

            if (tile.reflected)
            {
                if (symmetry == SymmetryType.I || symmetry == SymmetryType.D)
                {
                    output = Mod(tile.rotation, 2);
                }
                else if (symmetry == SymmetryType.T)
                {
                    output = (tile.rotation % 2 == 0) ? Mod(tile.rotation, 4) : Mod(tile.rotation, 4) | (1 << 31);
                }
                else if (symmetry == SymmetryType.D)
                {
                    output = Mod(tile.rotation + 1, 2);
                }
                else if (symmetry == SymmetryType.L)
                {
                    output = (tile.rotation % 2 == 0) ? Mod(tile.rotation + 2, 4) : Mod(tile.rotation, 4);
                    output |= (1 << 31);
                }
                else
                {
                    output = Mod(tile.rotation, 4) | ((tile.reflected) ? (1 << 31) : 0);
                }
            }
            else
            {
                if (symmetry == SymmetryType.I || symmetry == SymmetryType.D)
                {
                    output = Mod(tile.rotation, 2);
                }
                else
                {
                    output = Mod(tile.rotation, 4) | ((tile.reflected) ? (1 << 31) : 0);
                }
            }

            return output;
        }


        public static int EncodeTile(Tile tile, in Dictionary<int, SymmetryType> symmetryDictionary)
        {
            int encoded = (tile.id * 4 + EncodeRotationAndReflection(tile, symmetryDictionary));
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
            return output;
        }

        public static Tile DecodeTile(int encodedTile)
        {
            int id = DecodeTileId(encodedTile);
            return new Tile(id, DecodeTileRotation(encodedTile), DecodeReflection(encodedTile));
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

        public override readonly string ToString()
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
        public int[,] tileEnablerCountsByDirection;
        public float entrophy;
        public bool collapsed;

        float sumOfRelativeFreq;
        float sumOfRelativeFreqLog2;
        int collapsedIndex;

        private static Random random = new();

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
            random = new Random(Guid.NewGuid().GetHashCode());
            CalculateEntrophy();
        }


        public readonly int GetCollapsedPatternIndex()
        {
            //TO FIX?�
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

        public void CalculateEntrophy()
        {
            entrophy = (float)(Math.Log(sumOfRelativeFreq, 2) - (sumOfRelativeFreqLog2 / sumOfRelativeFreq) + (random.NextDouble() * 0.001f));
        }

        public void RemovePattern(int patternIndex, in PatternInfo[] patternInfo)
        {
            if (possiblePatterns.Contains(patternIndex))
            {
                possiblePatterns.Remove(patternIndex);
                RemoveFrequencies(patternIndex, patternInfo);
            }
        }

        public override readonly string ToString()
        {
            return entrophy.ToString("0.0");
        }

        public readonly int CompareTo(Cell other)
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

        public Dictionary<Direction, HashSet<int>> neigbourIndices;


        public PatternInfo(int patternId, int[] pattern, int patternSize, int patternHeight, int frecuency = 0)
        {
            this.pattern = pattern;
            this.id = patternId;
            this.patternHeight = patternHeight;
            this.patternSize = patternSize;
            this.frecuency = frecuency;
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
        public readonly HashSet<int> GetCompatiblesInDirection(Direction direction)
        {
            return neigbourIndices[direction];
        }

        internal void UpdateFrecuencies(float totalPatterns)
        {
            relativeFrecuency = frecuency / totalPatterns;
            relativeFrecuencyLog2 = (float)(Math.Log(relativeFrecuency, 2));
            freqTimesFreqLog2 = relativeFrecuency * relativeFrecuencyLog2;
        }

        public readonly int GetEncodedTileIndex()
        {
            return pattern[0];
        }

        public override readonly string ToString()
        {
            return string.Join(".", pattern);
        }

    }
}