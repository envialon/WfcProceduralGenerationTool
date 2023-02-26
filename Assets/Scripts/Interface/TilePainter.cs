using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;


public class TilePainter : MonoBehaviour
{
    public List<Grid> layers = new List<Grid>();

    [SerializeField]
    private Material tilemapRenderMaterial;

    private Tilemap currentLayer;

    public void AddLayer()
    {
        GameObject child = new GameObject();
        child.transform.parent = transform;
        child.transform.position = new Vector3(0f, layers.Count, 0f);

        child.name = "Layer " + layers.Count;

        layers.Add(child.AddComponent<Grid>());
        layers[layers.Count - 1].cellSwizzle = GridLayout.CellSwizzle.XZY;
    }

    public void ClearLayers()
    {
        layers = new List<Grid>();
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

}


[CustomEditor(typeof(TilePainter))]
public class TilePainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TilePainter tilePainter = (TilePainter)target;
        if (GUILayout.Button("Add Layer"))
        {
            tilePainter.AddLayer();
        }        
        if (GUILayout.Button("Clear Layers"))
        {
            tilePainter.ClearLayers();
        }
        GUILayout.Space(10);     
        DrawDefaultInspector();
    }
}
