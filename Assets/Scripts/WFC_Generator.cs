using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct FreeSpace
{

    public Vector3 coords;
    public HashSet<int> possibleTiles;

    public FreeSpace(Vector3 pos)
    {
        coords = pos;
        possibleTiles = new HashSet<int>();
    }
}

public struct PlacedTile
{

    public int id;
    public Vector3 pos;
    public Quaternion rot;

    public PlacedTile(int id, Vector3 pos, Quaternion rot)
    {
        this.id = id;
        this.pos = pos;
        this.rot = rot;
    }
}

namespace WFC_Procedural_Generator_Framework
{
    public class WFC_Generator : MonoBehaviour
    {
        public int mapSize = 10;

        public float tileSize = 1f;

        public TileSet tileSet;
        AdyacencyRulesGenerator adyacencyRulesGenerator;


        void Start()
        {
            //generate rules 
            adyacencyRulesGenerator = new AdyacencyRulesGenerator(tileSet, tileSize);


            //wave collapsing
            WaveFunctionCollapse(adyacencyRulesGenerator.GenerateAdjacencyMatrix(), tileSize, mapSize);

            //creating the new mesh 

        }

        private List<PlacedTile> WaveFunctionCollapse(List<List<List<int>>> adjacencyMatrix, float tileSize, int mapSize)
        {
            List<FreeSpace> freeSpaces = new List<FreeSpace>();
            List<PlacedTile> placedTiles = new List<PlacedTile>();

            InitializeFreeSpaces(freeSpaces, tileSize, mapSize);

            while (freeSpaces.Count > 0)
            {
                // get the space with the least possible tiles
                int candidateIndex = GetCandidateIndex(freeSpaces);
                FreeSpace candidate = freeSpaces[candidateIndex];

                // select a random tile from those
                // might be replaceable by mesh
                int tileIndexToPlace = GetTileToPLace(candidate);
                Vector3 posToPlace = candidate.coords;

                // delete it from freespaces 
                freeSpaces.RemoveAt(candidateIndex);

                // add it to placed tiles 
                placedTiles.Add(new PlacedTile(tileIndexToPlace, posToPlace, Quaternion.identity));

                // reorganize the freeSpaces
            }

            return placedTiles;
        }

        private int GetTileToPLace(FreeSpace candidate)
        {
            List<int> possibleTiles = new List<int>(candidate.possibleTiles);
            int randomIndex = Random.Range(0, possibleTiles.Count);
            return possibleTiles[randomIndex];
        }

        private int GetCandidateIndex(List<FreeSpace> freeSpaces)
        {
            int minPossibleTiles = int.MaxValue;
            int freeSpaceIndex = 0;

            for (int i = 0; i < freeSpaces.Count; i++)
            {
                if (freeSpaces[i].possibleTiles.Count < minPossibleTiles)
                {
                    freeSpaceIndex = i;
                    minPossibleTiles = freeSpaces[i].possibleTiles.Count;
                }
            }

            return freeSpaceIndex;
        }



        private void InitializeFreeSpaces(List<FreeSpace> freeSpaces, float tileSize, int mapSize)
        {
            float increment = mapSize / tileSize;

            // ONLY INITIALIZING FOR Y = 0 
            for (float i = 0; i <= mapSize; i += increment)
            {
                for (float j = 0; j <= mapSize; j += increment)
                {
                    freeSpaces.Add(new FreeSpace(new Vector3(i, 0, j)));
                }
            }
        }

    }
}