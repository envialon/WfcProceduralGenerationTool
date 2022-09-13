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
            Dictionary<HashSet<Vector2>, string> uniqueFaces = new Dictionary<HashSet<Vector2>, string>(HashSet<Vector2>.CreateSetComparer());
            int uniqueFaceCounter = 0;

            for (int i = 0; i < tiles.Count; i++)
            {
                List<HashSet<Vector2>> faces = GetFaces(tiles[i].mesh.vertices);

                for (int j = 0; j < faces.Count; j++)
                {
                    string faceID = j >= 4 ? "v" : "";

                    if (faces[j].Count == 0)
                    {
                        faceID = "-1";
                    }
                    else if (!uniqueFaces.ContainsKey(faces[j]))
                    {
                        faceID = faceID + uniqueFaceCounter;

                        if (CheckFaceSymmetry(faces[j]))
                        {
                            // if symmetric, add s 
                            faceID = faceID + "s";
                        }
                        else
                        {
                            // transform all of the vertices to be mirrored
                            // add the mirrored version of the face
                            uniqueFaces.Add(MirrorFace(faces[j]), faceID + "f");
                            // update counter
                            uniqueFaceCounter++;
                        }
                        uniqueFaces.Add(faces[j], faceID);
                        uniqueFaceCounter++;
                    }
                    else
                    {
                        //figure what's the faceID and update it if it has been registered before
                        faceID = uniqueFaces[faces[j]];
                    }
                    //update the code for the face 
                    tiles[i].faces[j] = faceID;
                }
            }
        }

        private HashSet<Vector2> MirrorFace(HashSet<Vector2> faceVertex)
        {
            List<Vector2> result = new List<Vector2>(faceVertex);
            //MIRRORS OVER THE VERTICAL AXIS
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = new Vector2(tileSize, 0) - result[i];
            }

            return new HashSet<Vector2>(result);
        }

        private bool CheckFaceSymmetry(HashSet<Vector2> vertices)
        {
            List<Vector2> faceVertices = new List<Vector2>(vertices);
            Vector2 threshold = new Vector2(tileSize, 0);

            // ONLY CHECKS FOR SYMMETRY OVER THE VERTICAL AXIS


            for (int i = 0; i < faceVertices.Count; i++)
            {
                Vector3 v = threshold - faceVertices[i];
                if (!faceVertices.Contains(threshold - faceVertices[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private List<HashSet<Vector2>> GetFaces(Vector3[] vertices)
        {
            int faceCount = 6;
            List<HashSet<Vector2>> faceVertices = new List<HashSet<Vector2>>();
            for (int i = 0; i < faceCount; i++)
            {
                faceVertices.Add(new HashSet<Vector2>());
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x == 0)
                {
                    faceVertices[0].Add(new Vector2(vertices[i].z, vertices[i].y));
                }
                else if (vertices[i].x == tileSize)
                {
                    faceVertices[1].Add(new Vector2(vertices[i].z, vertices[i].y));
                }

                if (vertices[i].z == 0)
                {
                    faceVertices[2].Add(new Vector2(vertices[i].x, vertices[i].y));
                }
                else if (vertices[i].z == tileSize)
                {
                    faceVertices[3].Add(new Vector2(vertices[i].x, vertices[i].y));
                }

                if (vertices[i].y == 0)
                {
                    faceVertices[4].Add(new Vector2(vertices[i].x, vertices[i].z));
                }
                else if (vertices[i].y == tileSize)
                {
                    faceVertices[5].Add(new Vector2(vertices[i].x, vertices[i].z));
                }
            }
            return faceVertices;
        }


        public void RandomGenerate()
        {

        }
    }
}