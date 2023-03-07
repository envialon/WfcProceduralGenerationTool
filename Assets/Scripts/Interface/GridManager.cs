using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Tilemap))]
public class GridManager : MonoBehaviour
{
    public BoxCollider selectorCollider;
    Tilemap tilemap;

    public Grid grid;

    public int numberOfLayers = 0;
    public int selectedLayer = -1;

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        tilemap.ClearAllTiles();
        Initialize();
    }

    public void Initialize(int mapsize, int height = 1)
    {
        Initialize();
        SetGridSize(mapsize, height);
    }

    public void Initialize()
    {
        numberOfLayers = 1;
        selectedLayer = 0;
        tilemap = GetComponent<Tilemap>();
        grid = GetComponent<Grid>();
        selectorCollider = GetComponent<BoxCollider>();
        grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
    }

    public void SetGridSize(int size, int height)
    {
        numberOfLayers = height;
        selectorCollider.size = new Vector3(size, 0, size);
        selectorCollider.center = new Vector3(size / 2, 0, size / 2);
    }

    public void SelectLayer(int selection)
    {
        selectedLayer = Mathf.Max(0, Mathf.Min(numberOfLayers - 1, selection));
        selectorCollider.center = new Vector3(selectorCollider.center.x, selectedLayer, selectorCollider.center.z);
    }

    public void SetTile(Vector3Int coords, Tile tile)
    {
        tilemap.SetTile(coords, tile);
    }

    public GameObject GetTilePrefab(Vector3Int coords)
    {
        return tilemap.GetTile(coords).GameObject();
    }
}
