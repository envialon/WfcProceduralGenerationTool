using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(BoxCollider))]
public class TilePainter : MonoBehaviour
{
    public int mapSize = 10;
    public List<Grid> layers = new List<Grid>();

    [SerializeField]
    private Material tilemapRenderMaterial;

    public int selectedLayer = 0;
    private BoxCollider selectedCollider;
    
    
    private void OnEnable()
    {
        selectedCollider = gameObject.GetComponent<BoxCollider>();
        selectedCollider.size = new Vector3(mapSize, 0, mapSize);
    }

    public void AddLayer()
    {
        GameObject child = new GameObject();
        child.transform.parent = transform;
        child.transform.position = new Vector3(0f, layers.Count, 0f);

        child.name = "Layer " + layers.Count;

        layers.Add(child.AddComponent<Grid>());
        layers[layers.Count - 1].cellSwizzle = GridLayout.CellSwizzle.XZY;

        layers[selectedLayer] = layers[layers.Count - 1];
    }

    public void ClearLayers()
    {
        layers = new List<Grid>();
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    public void HandleClick(Vector3 mousePosition)
    {
        if (layers[selectedLayer] == null)
        {
            Debug.Log("No layer selected");
            return;
        }

        if (Physics.Raycast(Camera.main.transform.position, mousePosition, out RaycastHit hit, 1000f))
        {
            Vector3Int cellPosition = layers[selectedLayer].WorldToCell(hit.point);
            Debug.Log(cellPosition);
        }
    }

    public void HandleKeyPress(KeyCode keycode)
    {
        switch (keycode)
        {
            case KeyCode.UpArrow:
                selectedLayer = (selectedLayer + 1 >= layers.Count) ? layers.Count - 1 : selectedLayer++;
                selectedCollider.transform.position = new Vector3(0, selectedLayer, 0);
                break;
            case KeyCode.DownArrow:
                selectedLayer = (selectedLayer - 1 < 0) ? 0 : selectedLayer--;
                selectedCollider.transform.position = new Vector3(0, selectedLayer, 0);
                break;
        }
    }

    public void SerializeTileMap()
    {

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
        if (GUILayout.Button("Serialize tileMap"))
        {
            tilePainter.SerializeTileMap();
        }
        DrawDefaultInspector();
    }


    private void OnSceneGUI()
    {
        TilePainter tilePainter = (TilePainter)target;
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            Debug.Log("Mouse down");
            tilePainter.HandleClick(Input.mousePosition);
        }
        if (e.type == EventType.KeyDown)
        {
            tilePainter.HandleKeyPress(e.keyCode);
        }
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }
}
