using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{

    public class InterfaceManager : MonoBehaviour
    {
        public GameObject tileSpotPrefab;

        int tileSize = 1;
        int mapSize = 10;

        List<List<GameObject>> displayGrid = new List<List<GameObject>>();

        void Start()
        {        
            // instantiate all of the gameobjects with mesh filters at their appropiate positions based on the tileSize & mapSize
            

            for(int x = 0; x < mapSize; x += mapSize/tileSize)
            {
                displayGrid.Add(new List<GameObject>());
                for(int z = 0; z < mapSize; z += mapSize/tileSize) {
                    displayGrid[x].Add(Instantiate(tileSpotPrefab, new Vector3(x, 0, z), Quaternion.identity, this.gameObject.transform));
                }
            }
        }

        public void PlaceTile()
        {

        }

        public void RemoveTile()
        {

        }
    }
}