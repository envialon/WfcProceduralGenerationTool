using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{

    public class InterfaceManager : MonoBehaviour
    {
        int tileSize = 1;
        int mapSize = 10;

        public TileSet tileSet;

        Dictionary<Vector3, Tile> placedTiles = new Dictionary<Vector3, Tile>();

        Collider baseCollider;
        MeshFilter meshFilter;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            baseCollider = GetComponent<Collider>();
            baseCollider.transform.localScale = new Vector3(mapSize, mapSize, mapSize);
        }

        public void PlaceTile(Vector3 spot)
        {

        }

        public void RemoveTile(Vector3 spot)
        {

        }

        private void HandleClick(Vector3 spot)
        {
            if (placedTiles.ContainsKey(spot))
            {
                RemoveTile(spot);
            }
            else
            {
                PlaceTile(spot);
            }
        }

        private Vector3 RoundCoords(Vector3 hitpoint)
        {
            float threshold = tileSize / 2;
            float xRemainder = hitpoint.x % tileSize;
            float yRemainder = hitpoint.y % tileSize;
            float zRemainder = hitpoint.z % tileSize;
            float x = (xRemainder >= threshold) ? hitpoint.x - xRemainder + tileSize : hitpoint.x - xRemainder;
            float y = (yRemainder >= threshold) ? hitpoint.y - yRemainder + tileSize : hitpoint.y - yRemainder;
            float z = (zRemainder >= threshold) ? hitpoint.z - zRemainder + tileSize : hitpoint.z - zRemainder;

            return new Vector3(x, y, z);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 collisionSpot = RoundCoords(hit.point);
                    HandleClick(collisionSpot);
                }
            }
        }
    }
}