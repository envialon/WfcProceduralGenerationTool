using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{

    public class InterfaceManager : MonoBehaviour
    {
        int tileSize = 1;
        int mapSize = 10;

        public TileSet tileSet;

        Dictionary<Vector3, Tile> placedTiles = new Dictionary<Vector3, Tile>();

        void Start()
        {          

          
        }

        

        public void PlaceTile(Vector3 spot)
        {

        }

        public void RemoveTile(Vector3 spot)
        {

        }

        private void HandleClick(Vector3 spot)
        {
            if(placedTiles.ContainsKey(spot))
            {
                RemoveTile(spot);
            }
            else
            {
                PlaceTile(spot);
            }
        }

        private void Update()
        {
        if(Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit))
                {
                    Vector3 collisionSpot = new Vector3(Mathf.Round(hit.point.x), Mathf.Round(hit.point.y), Mathf.Round(hit.point.z));
                    HandleClick(collisionSpot);
                }
            }
        }
    }
}