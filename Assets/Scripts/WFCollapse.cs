using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    public class WFCollapse : MonoBehaviour
    {
        public TileSet tileSet;
        public float tileSize = 1;

        void Start()
        {
            GenerateAdyacencyRules();
            RandomGenerate();
        }



        public void GenerateAdyacencyRules()
        {
            List<Tile> tiles = tileSet.tiles;

            for (int i = 0; i < tiles.Count; i++)
            {
                Tile currentTile = tiles[i];
                Mesh currentMesh = currentTile.mesh;
                Vector3[] currentVertices = currentMesh.vertices;

            }
        }

        public void RandomGenerate()
        {

        }

        private List<List<Vector3>> GetFaceVertices(Vector3[] vertices)
        {
            int faceCount = 6;
            List<List<Vector3>> faceVertices = new List<List<Vector3>>(faceCount);
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x == 0)
                {
                    faceVertices[0].Add(vertices[i]);
                }
                else if (vertices[i].x == tileSize)
                {
                    faceVertices[3].Add(vertices[i]);
                }

                if (vertices[i].y == 0)
                {
                    faceVertices[1].Add(vertices[i]);
                }
                else if (vertices[i].y == tileSize)
                {
                    faceVertices[4].Add(vertices[i]);
                }

                if (vertices[i].z == 0)
                {
                    faceVertices[2].Add(vertices[i]);
                }
                else if (vertices[i].z == tileSize)
                {
                    faceVertices[5].Add(vertices[i]);
                }
            }
            return faceVertices;
        }

    }
}