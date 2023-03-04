using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(BoxCollider))]
public class GridManager : MonoBehaviour
{
    public BoxCollider selectorCollider;
    List<Tilemap> tilemaps;

    public Grid grid;

    public int numberOfLayers = 0;
    public int selectedLayer = -1;

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        tilemaps.Clear();
        Initialize();
    }

    public void Initialize(int mapsize)
    {
        Initialize();
        SetGridSize(mapsize);
    }

    public void Initialize()
    {
        numberOfLayers = 0;
        selectedLayer = -1;
        tilemaps = new List<Tilemap>();
        grid = GetComponent<Grid>();
        selectorCollider = GetComponent<BoxCollider>();
        grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
        if (tilemaps.Count == 0) { AddLayer(); }
    }

    public void SetGridSize(int size)
    {
        selectorCollider.size = new Vector3(size, 0, size);
        selectorCollider.center = new Vector3(size / 2, 0, size / 2);
    }

    public void SelectLayer(int selection)
    {
        if (selection < 0)
        {
            selectedLayer = Mathf.Min(numberOfLayers, selection);
            selectorCollider.center = new Vector3(selectorCollider.center.x, selectedLayer, selectorCollider.center.z);
        }
    }

    public void AddLayer()
    {
        numberOfLayers++;
        selectedLayer = numberOfLayers - 1;
        GameObject layer = new GameObject("Layer " + numberOfLayers);
        layer.transform.parent = transform;
        Tilemap tilemap = layer.AddComponent<Tilemap>();
        tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0.5f);
        tilemap.transform.localPosition = new Vector3(0, numberOfLayers - 1, 0);
        SelectLayer(selectedLayer);
    }

    public void SetTile(int x, int y, Tile tile)
    {
        tilemaps[0].SetTile(new Vector3Int(x, selectedLayer, y), tile);
    }

}
