using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace WFC_Procedural_Generator_Framework
{
    [RequireComponent(typeof(GridManager))]
    public class TilePainter : MonoBehaviour
    {
        public int selectedTile = 1;
        public int mapSize = 10;
        public int height = 1;

        GridManager gridManager;
        public TileSet tileSet;

        private List<GameObject> tilePrefabs = new List<GameObject>();
        private TileMapData tileMap;
        private UnityEngine.Tilemaps.Tile tile;

        private void OnDrawGizmosSelected()
        {

            for (int i = 0; i <= mapSize; i++)
            {
                Gizmos.DrawLine(new Vector3(0, gridManager.selectedLayer, i), new Vector3(mapSize, gridManager.selectedLayer, i));
                Gizmos.DrawLine(new Vector3(i, gridManager.selectedLayer, 0), new Vector3(i, gridManager.selectedLayer, mapSize));
            }
        }

        public void Clear()
        {
            this.gridManager.Clear();
            this.tileMap = new TileMapData(mapSize, height);
            this.tile = null;
            CreatePrefabList();
            Initialize();
        }


        private void CreatePrefabList()
        {
            foreach (GameObject tileobj in tilePrefabs)
            {
                DestroyImmediate(tileobj);
            }
            tilePrefabs.Clear();
            int tileCount = tileSet.tiles.Count;
            for (int i = 0; i < tileCount; i++)
            {
                GameObject go = new GameObject("Tile " + i);
                go.AddComponent<MeshRenderer>().material = tileSet.tiles[i].material;
                go.AddComponent<MeshFilter>().mesh = tileSet.tiles[i].mesh;
                go.transform.parent = this.transform;
                go.SetActive(false);
                tilePrefabs.Add(go);
            }
        }

        public void Initialize()
        {            
            tileMap = new TileMapData(mapSize, height);
            gridManager = GetComponent<GridManager>();
            tile = (UnityEngine.Tilemaps.Tile)ScriptableObject.CreateInstance(typeof(UnityEngine.Tilemaps.Tile));
            gridManager.Initialize(mapSize, height);
            SetCurrentTile(selectedTile);
        }

        private void MoveGridSelector()
        {
            gridManager.SelectLayer(gridManager.selectedLayer);
        }

        public void SetCurrentTile(int selection)
        {
            selectedTile = Mathf.Max(0, Mathf.Min(tileSet.tiles.Count - 1, selection));
            tile.gameObject = tilePrefabs[selection];
        }

        public void HandleKeyPress(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.UpArrow:
                    gridManager.SelectLayer(gridManager.selectedLayer + 1);
                    MoveGridSelector();
                    break;
                case KeyCode.DownArrow:
                    gridManager.SelectLayer(gridManager.selectedLayer - 1);
                    MoveGridSelector();
                    break;
                case KeyCode.LeftArrow:
                    SetCurrentTile(selectedTile - 1);
                    break;
                case KeyCode.RightArrow:
                    SetCurrentTile(selectedTile + 1);
                    break;
            }
        }

        private void PlaceTile(Vector3Int coords)
        {
            SetCurrentTile(selectedTile);
            gridManager.SetTile(coords, tile);
            gridManager.GetTilePrefab(coords).SetActive(true);
            //update tileMap
        }

        private void RotateTile(Vector3Int coords)
        {

        }



        public void HandleClick(Vector3 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Vector3Int cellPosition = gridManager.grid.WorldToCell(hit.point);
                cellPosition = new Vector3Int(cellPosition.x, gridManager.selectedLayer, cellPosition.y);

                Debug.Log("Hit at: " + hit.point + " Corresponds to cell " + cellPosition);

                if (selectedTile == tileMap.GetTile(cellPosition.x, cellPosition.y, cellPosition.z).id)
                {
                    RotateTile(cellPosition);
                }
                else
                {
                    PlaceTile(cellPosition);
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TilePainter))]
    public class TilePainterEditor : Editor
    {

        private void Awake()
        {
            TilePainter tilePainter = (TilePainter)target;
            tilePainter.Clear();
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            TilePainter tilePainter = (TilePainter)target;

            if (GUILayout.Button("Clear"))
            {
                tilePainter.Clear();
            }
            GUILayout.Space(10);
        }


        private void OnSceneGUI()
        {
            TilePainter tilePainter = (TilePainter)target;
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                tilePainter.HandleClick(e.mousePosition);
            }
            if (e.type == EventType.KeyDown)
            {
                tilePainter.HandleKeyPress(e.keyCode);
            }
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}
#endif