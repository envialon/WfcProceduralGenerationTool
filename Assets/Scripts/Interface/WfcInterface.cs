using UnityEngine;
using UnityEditor;


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
        private InputTileMapData inputMap;
        private WfcModel model;
        private int[,,] lastMapGenerated;

        private void OnDrawGizmosSelected()
        {
            Vector3 pos = transform.position;
            for (int i = 0; i <= inputMapSize; i++)
            {
                Gizmos.DrawLine(new Vector3(0, 0, i) + pos, new Vector3(inputMapSize, 0, i) + pos);
                Gizmos.DrawLine(new Vector3(i, 0, 0) + pos, new Vector3(i, 0, inputMapSize) + pos);
            }
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
            inputMap = new InputTileMapData(inputMapSize, inputMapHeight);
            model = new WfcModel(inputMap);

            grid = GetComponent<Grid>();
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            boxCollider = GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(inputMapSize, 0, inputMapSize);
            boxCollider.center = new Vector3(inputMapSize / 2, 0, inputMapSize / 2);
        }

        private void DrawWithCamera(Camera cam)
        {
            if (cam)
            {
                if (tileSet)
                {
                    DrawInputMap(cam);
                }
            }
        }

        private void DrawInputMap(Camera cam)
        {
            for (int i = 0; i < inputMapSize; i++)
            {
                for (int j = 0; j < inputMapSize; j++)
                {
                    Tile currentTile = inputMap.GetTile(i, 0, j);
                    Matrix4x4 currentTRS = Matrix4x4.TRS(transform.position + new Vector3(i, 0, j), Quaternion.identity, Vector3.one);
                    Graphics.DrawMesh(tileSet.GetMesh(currentTile.id), currentTRS, tileSet.GetMaterial(currentTile.id), 0, cam);
                }
            }
        }

        public void HandleKeyPress(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.S:

                    selectedTile = (selectedTile + 1) % tileSet.tiles.Count;
                    break;

                case KeyCode.A:
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
        }

        public void Train()
        {
            model.Train(inputMap, patternSize);
        }

        public void Generate()
        {
            lastMapGenerated = model.Generate();
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

            if (GUILayout.Button("Train"))
            {
                t.Train();
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