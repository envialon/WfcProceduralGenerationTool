using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine.Tilemaps;

namespace WFC_Procedural_Generator_Framework
{
    public class TilePainter : MonoBehaviour
    {
        public int numberOfLayers = 0;
        public int selectedLayer = 0;
        public int selectedTile = 1;
        public int mapSize = 10;

        public Grid grid;
        public TileSet tileSet;

        [SerializeField]
        private Material tilemapRenderMaterial;

        private (MeshFilter, MeshRenderer)[,,] tileRenderers;
        private BoxCollider selectedCollider;
        private GameObject child;
        private GameObject slots;
        private TileMap tileMap;

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i <= mapSize; i++)
            {
                Gizmos.DrawLine(new Vector3(0, selectedLayer, i), new Vector3(mapSize, selectedLayer, i));
                Gizmos.DrawLine(new Vector3(i, selectedLayer, 0), new Vector3(i, selectedLayer, mapSize));
            }
        }

        public void Initialize()
        {
            if (slots == null)
            {
                slots = new GameObject("Slots");
                slots.transform.parent = this.transform;
            }
            if (child == null)
            {
                child = new GameObject("GridSelector");
                child.transform.localPosition = new Vector3(0, 0, 0);
                child.transform.parent = transform;
            }
            if (selectedCollider == null)
            {
                selectedCollider = child.AddComponent<BoxCollider>();
                selectedCollider.size = new Vector3(mapSize, 0, mapSize);
                selectedCollider.center = new Vector3(mapSize / 2, 0, mapSize / 2);
            }
            if (grid == null)
            {
                grid = child.AddComponent<Grid>();
                grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
            }
            if (numberOfLayers == 0) AddLayer();
        }

        private void MoveGridSelector()
        {
            grid.transform.position = new Vector3(0f, selectedLayer, 0f);
        }

        public void AddLayer()
        {
            numberOfLayers++;
            selectedLayer = numberOfLayers - 1;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    GameObject slot = new GameObject("Slot");
                    slot.transform.position = new Vector3(i, selectedLayer, j);
                    slot.transform.parent = slots.transform;
                    tileRenderers[i, selectedLayer, j] = (slot.AddComponent<MeshFilter>(), slot.AddComponent<MeshRenderer>());
                }
            }
            MoveGridSelector();
        }

        public void HandleKeyPress(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.UpArrow:
                    selectedLayer = (selectedLayer + 1 >= numberOfLayers) ? numberOfLayers - 1 : selectedLayer + 1;
                    MoveGridSelector();
                    break;
                case KeyCode.DownArrow:
                    selectedLayer = (selectedLayer - 1 < 0) ? 0 : selectedLayer - 1;
                    MoveGridSelector();
                    break;
                case KeyCode.LeftArrow:
                    selectedTile = (selectedTile - 1 < 0) ? 0 : selectedTile - 1;
                    break;
                case KeyCode.RightArrow:
                    selectedTile = (selectedTile + 1 >= tileSet.tiles.Count - 1) ? tileSet.tiles.Count - 1 : selectedTile + 1;
                    break;
            }
        }

        private void PlaceTile(Vector3Int coords)
        {
            (MeshFilter filter, MeshRenderer render) = tileRenderers[coords.x, coords.y, coords.z];
            filter.mesh = tileSet.tiles[selectedTile].mesh;
            render.material = tilemapRenderMaterial;
        }

        private void RotateTile(Vector3Int coords)
        {

        }

        public void HandleClick(Vector3 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Vector3Int cellPosition = grid.WorldToCell(hit.point);
                cellPosition = new Vector3Int(cellPosition.x, selectedLayer, cellPosition.y);

                Debug.Log("Hit at: " + hit.point + " Corresponds to cell " + cellPosition);

                if (selectedTile == tileMap.GetTile(cellPosition.x, cellPosition.y, cellPosition.y).id)
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


    [CustomEditor(typeof(TilePainter))]
    public class TilePainterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            TilePainter tilePainter = (TilePainter)target;
            if (GUILayout.Button("Add Layer"))
            {
                tilePainter.AddLayer();
            }
            //add a restart button

            GUILayout.Space(10);
            tilePainter.Initialize();
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