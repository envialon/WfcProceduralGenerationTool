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
            SortedSet<HashSet<Vector3>> uniqueFaces = new SortedSet<HashSet<Vector3>>();

            for (int i = 0; i < tiles.Count; i++)
            {
                Tile currentTile = tiles[i];
                Mesh currentMesh = currentTile.mesh;
                List<HashSet<Vector3>> faceVertices = GetFaceVertices(currentMesh.vertices);

                for (int j = 0; j < faceVertices.Count; j++)
                {
                    string faceID = "";

                    if (!uniqueFaces.Contains(faceVertices[j]))
                    {
                        uniqueFaces.Add(faceVertices[j]);


                        // check for symmetry 
                        // if symmetric, add s 

                        // transform all of the vertices to be mirrored 
                        // add the mirrored version of the face
                    }

                    //figure what's the faceID and update it 
                    //update the code for the face 
                }

            }
        }

        private bool CheckSymmetry(HashSet<Vector3> vertices)
        {
            List<Vector3> faceVertices = new List<Vector3>(vertices);
            Vector3 threshold = faceVertices[0] + faceVertices[1];
            if (threshold.x == 0)
            {
                threshold = new Vector3(0, tileSize, tileSize);
            }
            else if (threshold.y == 0)
            {
                threshold = new Vector3(tileSize, 0, tileSize);
            }
            else if (threshold.z == 0)
            {
                threshold = new Vector3(tileSize, tileSize, 0);
            }
            else
            {
                Debug.Log("Something went wrong during check symmetry");
            }

            for (int i = 0; i < faceVertices.Count; i++)
            {

            }
            return false;
        }

        private List<HashSet<Vector3>> GetFaceVertices(Vector3[] vertices)
        {
            int faceCount = 6;
            List<HashSet<Vector3>> faceVertices = new List<HashSet<Vector3>>(faceCount);
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


        public void RandomGenerate()
        {

        }
    }
}