using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct FreeSpace 
{
    public HashSet<int> possibleTiles;   
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
            SortedDictionary<Vector3, FreeSpace> freeSpaces = new SortedDictionary<Vector3, FreeSpace>();
            SortedDictionary<Vector3, PlacedTile> placedTiles = new SortedDictionary<Vector3, PlacedTile>();

            InitializeFreeSpaces(freeSpaces, tileSize, mapSize);
            UpdatePossibleTiles(freeSpaces, placedTiles);

            while (freeSpaces.Count > 0)
            {
                // get the space with the least possible tiles                
                Vector3 candidatePos = GetCandidate(freeSpaces);
                FreeSpace candidate = freeSpaces[candidatePos];

                // select a random tile from those
                // might be replaceable by mesh
                int tileIndexToPlace = GetTileToPLace(candidate);

                // delete it from freespaces 
                freeSpaces.Remove(candidatePos);

                // add it to placed tiles 
                placedTiles.Add(candidatePos, new PlacedTile(tileIndexToPlace, candidatePos, Quaternion.identity));

                // reorganize the freeSpaces
            }

            return new List<PlacedTile>(placedTiles.Values); ;
        }

        private void UpdatePossibleTiles(SortedDictionary<Vector3, FreeSpace> freeSpacesDic, SortedDictionary<Vector3, PlacedTile> placedTiles)
        {
            List<FreeSpace> freeSpaces = new List<FreeSpace>(freeSpacesDic.Values);
            for(int i = 0; i < freeSpaces.Count; i++)
            {

            }
        }
        private int GetTileToPLace(FreeSpace candidate)
        {
            List<int> possibleTiles = new List<int>(candidate.possibleTiles);
            int randomIndex = Random.Range(0, possibleTiles.Count);
            return possibleTiles[randomIndex];
        }

        private Vector3 GetCandidate(SortedDictionary<Vector3, FreeSpace> freeSpaces)
        {
            int minPossibleTiles = int.MaxValue;
            Vector3 candidate = new Vector3();

            
            foreach (KeyValuePair<Vector3, FreeSpace> p in freeSpaces)
            {
                
                if (p.Value.possibleTiles.Count < minPossibleTiles)
                {
                    candidate = p.Key;
                    minPossibleTiles = p.Value.possibleTiles.Count;
                }
            }
            return candidate;
        }

        private void InitializeFreeSpaces( SortedDictionary<Vector3, FreeSpace> freeSpaces, float tileSize, int mapSize)
        {
            float increment = mapSize / tileSize;

            // ONLY INITIALIZING FOR Y = 0 
            for (float i = 0; i <= mapSize; i += increment)
            {
                for (float j = 0; j <= mapSize; j += increment)
                {
                    Vector3 currentPos = new Vector3(i, 0, j);
                    freeSpaces.Add(currentPos, new FreeSpace());
                }
            }
        }

    }
}