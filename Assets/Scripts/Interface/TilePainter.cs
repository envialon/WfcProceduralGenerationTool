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

        GridManager gridManager;
        public TileSet tileSet;

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
            this.tileMap = new TileMapData(mapSize, 2);
        }

        public void Initialize()
        {
            if (tileMap == null)
            {
                tileMap = new TileMapData();
            }
            if (gridManager == null)
            {
                this.gridManager = GetComponent<GridManager>();
                gridManager.Initialize(mapSize);
            }
            tile = new UnityEngine.Tilemaps.Tile();
            SetCurrentTile(selectedTile);
        }

        private void MoveGridSelector()
        {
            gridManager.SelectLayer(gridManager.selectedLayer);
        }

        public void SetCurrentTile(int selection)
        {
            selectedTile = Mathf.Max(0, Mathf.Min(tileSet.tiles.Count - 1, selection));
            
        }

        public void AddLayer()
        {
            gridManager.AddLayer();
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
            gridManager.SetTile(coords.x, coords.z, tile);
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
            tilePainter.Initialize();
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            TilePainter tilePainter = (TilePainter)target;
            if (GUILayout.Button("Add Layer"))
            {
                tilePainter.AddLayer();
            }
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