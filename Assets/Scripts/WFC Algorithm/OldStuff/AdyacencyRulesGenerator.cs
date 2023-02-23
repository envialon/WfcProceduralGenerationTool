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
        var comparer = HashSet<Vector2>.CreateSetComparer();
        return comparer.GetHashCode(obj.Item1) + obj.Item2;
    }
}

namespace WFC_Procedural_Generator_Framework
{
    public class AdyacencyRulesGenerator // DEPRECATED
    {        
    //    public TileSet tileSet;
    //    public float tileSize;

    //    public AdyacencyRulesGenerator(TileSet tileSet, float tileSize = 1)
    //    {
    //        this.tileSet = tileSet;
    //        this.tileSize = tileSize;
    //        GenerateAdyacencyRules();
    //    }


    //    public void GenerateAdyacencyRules()
    //    {
    //        List<Tile> tiles = tileSet.tiles;
    //        Dictionary<(HashSet<Vector2>, int), string> uniqueHorizontalFaces = new Dictionary<(HashSet<Vector2>, int), string>(new Comparer());
    //        Dictionary<(HashSet<Vector2>, int), string> uniqueVerticalFaces = new Dictionary<(HashSet<Vector2>, int), string>(new Comparer());
    //        int uniqueHorizontalCounter = 0;
    //        int uniqueVerticalCounter = 0;

    //        for (int i = 0; i < tiles.Count; i++)
    //        {
    //            List<(HashSet<Vector2>, int)> facesAndTriangles = GetFaces(tiles[i].mesh);

    //            for (int j = 0; j < facesAndTriangles.Count; j++)
    //            {
    //                string faceID;
    //                (HashSet<Vector2>, int) face = facesAndTriangles[j];
    //                if (j >= 4)
    //                {
    //                    faceID = "v";
    //                    ManageFace(uniqueVerticalFaces, face, ref uniqueVerticalCounter, ref faceID);
    //                }
    //                else
    //                {
    //                    faceID = "";
    //                    ManageFace(uniqueHorizontalFaces, face, ref uniqueHorizontalCounter, ref faceID);
    //                }
    //                tiles[i].faces[j] = faceID;
    //            }
    //        }
    //    }

    //    public List<List<List<int>>> GenerateAdjacencyMatrix()
    //    {
    //        List<Tile> tiles = tileSet.tiles;
    //        List<List<List<int>>> neighbors = new List<List<List<int>>>();


    //        for (int i = 0; i < tiles.Count; i++)
    //        {
    //            neighbors.Add(new List<List<int>>());
    //            for (int j = 0; j < tiles.Count; j++)
    //            {
    //                neighbors[i].Add(new List<int>());
    //            }
    //        }

    //        for (int i = 0; i < tiles.Count; i++)
    //        {
    //            Tile origin = tiles[i];
    //            for (int j = i; j < tiles.Count; j++)
    //            {
    //                Tile destination = tiles[j];
    //                for (int f = 0; f < origin.faces.Count; f++)
    //                {
    //                    if (origin.faces[f] == destination.faces[f] + "f" ||
    //                        destination.faces[f] == origin.faces[f] + "f" ||
    //                        (origin.faces[f] == destination.faces[f] &&
    //                        origin.faces[f][origin.faces[f].Length - 1] == 's'))
    //                    {

    //                        neighbors[i][j].Add(f);
    //                        if (i != j)
    //                        {
    //                            neighbors[j][i].Add(f);
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        return neighbors;
    //    }

    //    private string ManageFace(Dictionary<(HashSet<Vector2>, int), string> uniqueFaces, (HashSet<Vector2>, int) face, ref int uniqueFaceCounter, ref string faceID)
    //    {
    //        if (face.Item1.Count == 0)
    //        {
    //            faceID = "-1";
    //        }
    //        else if (!uniqueFaces.ContainsKey(face))
    //        {
    //            faceID = faceID + uniqueFaceCounter;

    //            if (CheckFaceSymmetry(face.Item1))
    //            {
    //                // if symmetric, add s 
    //                faceID = faceID + "s";
    //            }
    //            else
    //            {
    //                // transform all of the vertices to be mirrored
    //                // add the mirrored version of the face
    //                uniqueFaces.Add((MirrorFace(face.Item1), face.Item2), faceID + "f");
    //                // update counter
    //                uniqueFaceCounter++;
    //            }
    //            uniqueFaces.Add(face, faceID);
    //            uniqueFaceCounter++;
    //        }
    //        else
    //        {
    //            //figure what's the faceID and update it if it has been registered before
    //            faceID = uniqueFaces[face];
    //        }

    //        return faceID;
    //    }

    //    private HashSet<Vector2> MirrorFace(HashSet<Vector2> faceVertex)
    //    {
    //        List<Vector2> result = new List<Vector2>(faceVertex);

    //        //MIRRORS OVER THE VERTICAL AXIS
    //        for (int i = 0; i < result.Count; i++)
    //        {
    //            result[i] = new Vector2(tileSize - result[i].x, result[i].y);
    //        }

    //        return new HashSet<Vector2>(result);
    //    }

    //    private bool CheckFaceSymmetry(HashSet<Vector2> vertices)
    //    {
    //        List<Vector2> faceVertices = new List<Vector2>(vertices);

    //        // ONLY CHECKS FOR SYMMETRY OVER THE VERTICAL AXIS
    //        for (int i = 0; i < faceVertices.Count; i++)
    //        {
    //            if (!faceVertices.Contains(new Vector2(tileSize - faceVertices[i].x, faceVertices[i].y)))
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }

    //    private List<(HashSet<Vector2>, int)> GetFaces(Mesh mesh)
    //    {
    //        int faceCount = 6;
    //        Vector3[] vertices = mesh.vertices;
    //        List<int> triangles = new List<int>(mesh.triangles);

    //        List<List<int>> faceIndexes = new List<List<int>>();
    //        List<int> triangleCount = new List<int>(new int[faceCount]);
    //        List<(HashSet<Vector2>, int)> output = new List<(HashSet<Vector2>, int)>();

    //        for (int i = 0; i < faceCount; i++)
    //        {
    //            faceIndexes.Add(new List<int>());
    //        }

    //        // Finding all of the face indexes
    //        for (int i = 0; i < vertices.Length; i++)
    //        {
    //            if (vertices[i].x == 0)
    //            {
    //                // faceVertices[0].Item1.Add(new Vector2(vertices[i].z, vertices[i].y));
    //                faceIndexes[0].Add(i);
    //            }
    //            else if (vertices[i].x == tileSize)
    //            {
    //                faceIndexes[1].Add(i);
    //            }

    //            if (vertices[i].z == 0)
    //            {
    //                faceIndexes[2].Add(i);
    //            }
    //            else if (vertices[i].z == tileSize)
    //            {
    //                faceIndexes[3].Add(i);
    //            }

    //            if (vertices[i].y == 0)
    //            {
    //                faceIndexes[4].Add(i);
    //            }
    //            else if (vertices[i].y == tileSize)
    //            {
    //                faceIndexes[5].Add(i);
    //            }

    //        }

    //        // counting each face triangle
    //        for (int t = 0; t < triangles.Count; t += 3)
    //        {
    //            int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];

    //            for (int i = 0; i < faceIndexes.Count; i++)
    //            {
    //                if (faceIndexes[i].Contains(a) || faceIndexes[i].Contains(b) || faceIndexes[i].Contains(c))
    //                {
    //                    triangleCount[i]++;
    //                }
    //            }
    //        }

    //        //translating faceIndexes to Vector2
    //        for (int i = 0; i < faceCount; i++)
    //        {
    //            HashSet<Vector2> face = new HashSet<Vector2>();
    //            for (int j = 0; j < faceIndexes[i].Count; j++)
    //            {
    //                if (i < 2)
    //                {
    //                    face.Add(new Vector2(vertices[faceIndexes[i][j]].z, vertices[faceIndexes[i][j]].y));
    //                }
    //                else if (i < 4)
    //                {
    //                    face.Add(new Vector2(vertices[faceIndexes[i][j]].x, vertices[faceIndexes[i][j]].y));
    //                }
    //                else
    //                {
    //                    face.Add(new Vector2(vertices[faceIndexes[i][j]].x, vertices[faceIndexes[i][j]].z));
    //                }
    //            }
    //            output.Add((face, triangleCount[i]));
    //        }
    //        return output;
    //    }
    }
}