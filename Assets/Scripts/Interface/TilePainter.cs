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
        public int numberOfLayers = 1;
        public int selectedLayer = 0;
        public int selectedTile = 1;
        public int mapSize = 10;

        public Grid grid;
        public TileSet tileSet;

        [SerializeField]
        private Material tilemapRenderMaterial;

        private BoxCollider selectedCollider;
        private GameObject child;
        private Tilemap tileMap;


        public void Initialize()
        {
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
            
        }

        private void MoveGridSelector()
        {
            grid.transform.position = new Vector3(0f, selectedLayer, 0f);
        }

        public void AddLayer()
        {
            numberOfLayers++;
            selectedLayer = numberOfLayers - 1;
            MoveGridSelector();
            //GameObject child = new GameObject();
            //child.transform.parent = transform;
            //child.transform.position = new Vector3(0f, layers.Count, 0f);

            //child.name = "Layer " + layers.Count;

            //layers.Add(child.AddComponent<Grid>());
            //layers[layers.Count - 1].cellSwizzle = GridLayout.CellSwizzle.XZY;

            //layers[selectedLayer] = layers[layers.Count - 1];
        }

        public void HandleClick(Vector3 mousePosition)
        {
            //Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosition);
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Vector3Int cellPosition = grid.WorldToCell(hit.point);
                Debug.Log("Hit at: " + hit.point + " Corresponds to cell " + cellPosition);
            }
        }

        public void HandleKeyPress(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.UpArrow:
                    selectedLayer = (selectedLayer + 1 >= numberOfLayers) ? numberOfLayers - 1 : selectedLayer++;
                    MoveGridSelector();
                    break;
                case KeyCode.DownArrow:
                    selectedLayer = (selectedLayer - 1 < 0) ? 0 : selectedLayer--;
                    MoveGridSelector();
                    break;
                case KeyCode.LeftArrow:
                    selectedTile = (selectedTile - 1 < 0) ? 0 : selectedTile--;
                    break;
                case KeyCode.RightArrow:
                    selectedTile = (selectedTile + 1 >= tileSet.tiles.Count - 1) ? tileSet.tiles.Count - 1 : selectedTile++;
                    break;
            }
        }

        public void SerializeTileMap()
        {
            Debug.Log("Not implemented");
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i <= mapSize; i++)
            {
                Gizmos.DrawLine(new Vector3(0, selectedLayer, i), new Vector3(mapSize, selectedLayer, i));
                Gizmos.DrawLine(new Vector3(i, selectedLayer, 0), new Vector3(i, selectedLayer, mapSize));
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

            GUILayout.Space(10);
            if (GUILayout.Button("Serialize tileMap"))
            {
                tilePainter.SerializeTileMap();
            }
            tilePainter.Initialize();
        }


        private void OnSceneGUI()
        {
            TilePainter tilePainter = (TilePainter)target;
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                Debug.Log("Mouse down");
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