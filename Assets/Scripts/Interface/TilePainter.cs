using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;


public class TilePainter : MonoBehaviour
{
    public List<Tilemap> layers = new List<Tilemap>();

    [SerializeField]
    private Material tilemapRenderMaterial;

    private Tilemap currentLayer;
    
    public void AddLayer()
    {
        GameObject child = new GameObject();
        child.transform.parent = transform;
        child.AddComponent<Tilemap>();
        child.AddComponent<TilemapRenderer>().material = tilemapRenderMaterial;
        child.name = "Layer " + layers.Count;
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
        DrawDefaultInspector();
    }
}
