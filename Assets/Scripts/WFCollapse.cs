using System.Collections.Generic;
using UnityEngine;


public class Comparer : IEqualityComparer<(HashSet<Vector2>, int)>
{
    public bool Equals((HashSet<Vector2>, int) x, (HashSet<Vector2>, int) y)
    {
        return (x.Item1.SetEquals(y.Item1) && x.Item2 == y.Item2) ? true : false;
    }

    public int GetHashCode((HashSet<Vector2>, int) obj)
    {
        return obj.Item1.GetHashCode() + obj.Item2.GetHashCode();
    }
}
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
            Dictionary<(HashSet<Vector2>, int), string> uniqueFaces = new Dictionary<(HashSet<Vector2>, int), string>(new Comparer());
            int uniqueFaceCounter = 0;

            for (int i = 0; i < tiles.Count; i++)
            {
                List<(HashSet<Vector2>, int)> facesAndTriangleCount = GetFaces(tiles[i].mesh);


                for (int j = 0; j < facesAndTriangleCount.Count; j++)
                {
                    (HashSet<Vector2>, int) face = facesAndTriangleCount[j];
                    string faceID = j >= 4 ? "v" : "";

                    if (face.Item1.Count == 0)
                    {
                        faceID = "-1";
                    }
                    else if (!uniqueFaces.ContainsKey(face))
                    {
                        faceID = faceID + uniqueFaceCounter;

                        if (CheckFaceSymmetry(face.Item1))
                        {
                            // if symmetric, add s 
                            faceID = faceID + "s";
                        }
                        else
                        {
                            // transform all of the vertices to be mirrored
                            // add the mirrored version of the face
                            uniqueFaces.Add((MirrorFace(face.Item1),face.Item2),  faceID + "f");
                            // update counter
                            uniqueFaceCounter++;
                        }
                        uniqueFaces.Add(face, faceID);
                        uniqueFaceCounter++;
                    }
                    else
                    {
                        //figure what's the faceID and update it if it has been registered before
                        faceID = uniqueFaces[face];
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
                if (!faceVertices.Contains(threshold - faceVertices[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private List<(HashSet<Vector2>, int)> GetFaces(Mesh mesh)
        {
            int faceCount = 6;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            List<(HashSet<Vector2>, int)> faceVertices = new List<(HashSet<Vector2>, int)>();
            for (int i = 0; i < faceCount; i++)
            {
                faceVertices.Add(new(new HashSet<Vector2>(), new int()));
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x == 0)
                {
                    faceVertices[0].Item1.Add(new Vector2(vertices[i].z, vertices[i].y));
                }
                else if (vertices[i].x == tileSize)
                {
                    faceVertices[1].Item1.Add(new Vector2(vertices[i].z, vertices[i].y));
                }

                if (vertices[i].z == 0)
                {
                    faceVertices[2].Item1.Add(new Vector2(vertices[i].x, vertices[i].y));
                }
                else if (vertices[i].z == tileSize)
                {
                    faceVertices[3].Item1.Add(new Vector2(vertices[i].x, vertices[i].y));
                }

                if (vertices[i].y == 0)
                {
                    faceVertices[4].Item1.Add(new Vector2(vertices[i].x, vertices[i].z));
                }
                else if (vertices[i].y == tileSize)
                {
                    faceVertices[5].Item1.Add(new Vector2(vertices[i].x, vertices[i].z));
                }
            }
            return faceVertices;
        }


        public void RandomGenerate()
        {

        }
    }
}