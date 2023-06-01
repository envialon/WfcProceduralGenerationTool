using UnityEngine;
using UnityEditor;
using System;

namespace WFC_Procedural_Generator_Framework
{
    [ExecuteAlways]
    [RequireComponent(typeof(Grid))]
    [RequireComponent(typeof(BoxCollider))]
    public class WfcInterface : MonoBehaviour
    {
        public int patternSize = 2;
        public int inputMapSize = 10;
        public int inputMapHeight = 1;

        public Vector3Int outputSize = new Vector3Int(20, 1, 20);
        public TileSet tileSet;
        public int selectedTile = 0;

        private Grid grid;
        private BoxCollider boxCollider;

        private WfcModel model;
        private Tilemap inputMap;
        private Tilemap lastMapGenerated;

        private void OnDrawGizmosSelected()
        {
            Vector3 pos = transform.position;
            for (int i = 0; i <= inputMapSize; i++)
            {
                Gizmos.DrawLine(new Vector3(0, 0, i) + pos, new Vector3(inputMapSize, 0, i) + pos);
                Gizmos.DrawLine(new Vector3(i, 0, 0) + pos, new Vector3(i, 0, inputMapSize) + pos);
            }
        }

        private void OnValidate()
        {
            Resize();
        }

        private void Resize()
        {
            inputMap = new Tilemap(inputMapSize, inputMapHeight);
            model = new WfcModel(inputMap);

            if(boxCollider is null)
            {
                boxCollider = gameObject.GetComponent<BoxCollider>();
            }

            boxCollider.size = new Vector3(inputMapSize, 0, inputMapSize);
            boxCollider.center = new Vector3(inputMapSize / 2, 0, inputMapSize / 2);
        }

        private void OnEnable()
        {
            Initialize();
            Camera.onPreCull -= DrawWithCamera;
            Camera.onPreCull += DrawWithCamera;
        }

        private void OnDisable()
        {
            Camera.onPreCull -= DrawWithCamera;
        }

        private void Initialize()
        {
            inputMap = new Tilemap(inputMapSize, inputMapHeight);
            model = new WfcModel(inputMap);

            grid = GetComponent<Grid>();
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            boxCollider = GetComponent<BoxCollider>();
            Resize();
        }

        private void DrawWithCamera(Camera cam)
        {
            if (cam)
            {
                if (tileSet)
                {
                    DrawInputMap(cam);
                    if (lastMapGenerated != null)
                    {
                        DrawGeneratedMap(cam);
                    }
                }
            }
        }

        private void DrawTile(Tile currentTile, Vector3 tilePos, Camera cam)
        {
            Mesh mesh = tileSet.GetMesh(currentTile.id);
            if (mesh)
            {
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 90 * currentTile.rotation, 0));

                Matrix4x4 currentTRS = Matrix4x4.TRS(transform.position + tilePos, Quaternion.identity, Vector3.one);

                Graphics.DrawMesh(tileSet.GetMesh(currentTile.id), currentTRS, tileSet.GetMaterial(currentTile.id), 0, cam);
            }
        }

        private void DrawInputMap(Camera cam)
        {
            for (int i = 0; i < inputMapSize; i++)
            {
                for (int j = 0; j < inputMapSize; j++)
                {
                    Vector3 tilePos = new Vector3(i, 0, j);
                    DrawTile(inputMap.GetTile(i, 0, j), tilePos, cam);
                }
            }
        }

        private void DrawGeneratedMap(Camera cam)
        {
            int outputX = lastMapGenerated.width;
            int outputY = lastMapGenerated.height;
            int outputZ = lastMapGenerated.depth;

            Vector3Int startingPoint = new Vector3Int(inputMapSize + 1, 0, -outputZ / 2);

            for (int i = 0; i < outputX; i++)
            {
                for (int j = 0; j < outputZ; j++)
                {

                    Vector3Int tilePos = startingPoint + new Vector3Int(i, 0, j);
                    Tile tile = lastMapGenerated.GetTile(i, 0, j);

                    DrawTile(tile, tilePos, cam);
                }
            }
        }

        public void HandleKeyPress(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.N:

                    selectedTile = (selectedTile + 1) % tileSet.tiles.Count;
                    break;

                case KeyCode.M:
                    selectedTile = Mathf.Abs((selectedTile - 1)) % tileSet.tiles.Count;
                    break;
            }
        }

        private void DeleteTile(Vector3Int pos)
        {
            inputMap.SetTile(new Tile(0, 0), pos.x, pos.y, pos.z);
        }

        private void PlaceTile(Vector3Int pos)
        {
            inputMap.SetTile(new Tile(selectedTile, 0), pos.x, pos.y, pos.z);
        }

        private void RotateTile(Vector3Int pos)
        {
            inputMap.RotateAt(pos.x, pos.y, pos.z);
        }

        public void HandleClick(Vector3 mousePosition, int mouseButton)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Vector3Int cellPosition = grid.WorldToCell(hit.point);

                //Debug.Log("Hit at: " + hit.point + " Corresponds to cell " + cellPosition);
                if (mouseButton == 1)
                {
                    DeleteTile(cellPosition);
                }
                else if (selectedTile == inputMap.GetTile(cellPosition.x, cellPosition.y, cellPosition.z).id)
                {
                    RotateTile(cellPosition);
                }
                else
                {
                    PlaceTile(cellPosition);
                }
            }
        }

        public void Clear()
        {
            inputMap.Clear();
            if (lastMapGenerated is not null) lastMapGenerated.Clear();
        }

        public void Train()
        {
            model.Train(inputMap, patternSize);
            Debug.Log(model.inputReader.GetPatternSummary());
        }

        public void Generate()
        {
            int[,,] generatedIndexMap = model.Generate(outputSize.x, outputSize.y, outputSize.z); ;

            string msg = "";
            for (int i = 0; i < generatedIndexMap.GetLength(0); i++)
            {
                for (int j = 0; j < generatedIndexMap.GetLength(2); j++)
                {
                    msg += generatedIndexMap[i, 0, j] + " ";
                }
                msg += "\n";
            }
            Debug.Log(msg);

            lastMapGenerated = new Tilemap(generatedIndexMap);
        }

        internal void ResizeOutput()
        {
            model.SetOutputSize(outputSize.x, outputSize.y, outputSize.z);
        }

        internal void Iterate()
        {
            int[,,] generatedIndexMap = model.Iterate();
            string msg = "";
            for (int i = 0; i < generatedIndexMap.GetLength(0); i++)
            {
                for (int j = 0; j < generatedIndexMap.GetLength(2); j++)
                {
                    msg += generatedIndexMap[i, 0, j] + " ";
                }
                msg += "\n";
            }
            Debug.Log(msg);
        }
    }

    [CustomEditor(typeof(WfcInterface))]
    public class WfcInterfaceEditor : Editor
    {
        WfcInterface t;

        private void Awake()
        {
            t = (WfcInterface)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Clear"))
            {
                t.Clear();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Train"))
            {
                t.Train();
                
            }

            if(GUILayout.Button("Resize output")) {
                t.ResizeOutput();
            }

            GUILayout.Space(10);

            if(GUILayout.Button("Iterate"))
            {
                t.Iterate();
            }

            if (GUILayout.Button("Generate"))
            {
                t.Generate();
            }
        }

        private void OnSceneGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                t.HandleClick(e.mousePosition, e.button);
            }
            if (e.type == EventType.KeyDown)
            {
                t.HandleKeyPress(e.keyCode);
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}