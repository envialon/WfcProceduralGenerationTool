using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace WFC_Procedural_Generator_Framework
{

    [RequireComponent(typeof(BoxCollider))]
    public class InputPainter : MonoBehaviour
    {
        int tileSize = 1;
        int mapSize = 10;

        private const int defaultRotation = 0;

        TileMap tileMap;
        Collider mapCollider;
        GameObject[,] gameObjects;

        public GameObject slotPrefab;
        public int selectedTile = 1;
        public TileSet tileSet;

        private void OnValidate()
        {
            mapCollider = GetComponent<Collider>();
            mapCollider.transform.localScale = new Vector3(mapSize, 1, mapSize);
            tileMap = new TileMap(mapSize);
            gameObjects = new GameObject[mapSize, mapSize];
        }


        public void PlaceTile(int x, int y)
        {

            gameObjects[x, y].GetComponent<MeshFilter>().mesh = tileSet.tiles[selectedTile].mesh;
            gameObjects[x, y].transform.rotation = Quaternion.Euler(0, defaultRotation * 90, 0);
            tileMap.map[x, y].Set(selectedTile, defaultRotation);
        }

        public void RotateTile(int x, int y)
        {
            tileMap.map[x, y].RotateClockwise();
            gameObjects[x, y].transform.rotation = Quaternion.Euler(0, tileMap.map[x, y].rotation * 90, 0);
        }

        private void HandleClick(int x, int y)
        {
            if (gameObjects[x, y] is not null)
            {
                PlaceTile(x, y);
            }
            else if (tileMap.map[x, y].id == selectedTile)
            {
                RotateTile(x, y);
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

        private void OnMouseDown()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 p = transform.InverseTransformPoint(hit.point) + new Vector3(mapSize * 0.5f, 0f, mapSize * 0.5f);
                    p = new Vector3((int)(p.x / mapSize), 0, (int)(p.z / mapSize));
                    HandleClick((int)p.x, (int)p.z);
                }
            }

        }
    }
}