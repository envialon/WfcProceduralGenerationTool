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

        private const int defaultRotation = 0;

        TileMap tileMap = new TileMap();
        Collider baseCollider;
        MeshFilter[][] meshFilters;
        GameObject meshContainer;

        public int selectedTile = 0;
        public TileSet tileSet;


        void Start()
        {
            meshContainer = Instantiate(new GameObject(), this.transform);
            baseCollider = GetComponent<Collider>();
            baseCollider.transform.localScale = new Vector3(mapSize, 1, mapSize);


            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    tileMap.map[i][j] = new Tile();
                    meshFilters[i][j] = meshContainer.AddComponent<MeshFilter>();
                }
            }
        }

        public void PlaceTile(int x, int y)
        {
            meshFilters[x][y].mesh = tileSet.tiles[selectedTile].mesh;
            meshFilters[x][y].transform.rotation = Quaternion.Euler(0, defaultRotation * 90, 0);
            tileMap.map[x][y].Set(selectedTile, defaultRotation);
        }

        public void RotateTile(int x, int y)
        {
            tileMap.map[x][y].RotateClockwise();
            meshFilters[x][y].transform.rotation = Quaternion.Euler(0, tileMap.map[x][y].rotation * 90, 0);
        }

        private void HandleClick(int x, int y)
        {
            if (tileMap.map[x][y].id != 0)
            {
                RotateTile(x, y);
            }
            else
            {
                PlaceTile(x, y);
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
                    HandleClick((int)collisionSpot.x, (int)collisionSpot.z);
                }
            }
        }
    }
}