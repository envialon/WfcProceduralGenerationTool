using UnityEngine;
using UnityEditor;
using WFC_Model;

[ExecuteAlways]
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(BoxCollider))]
public class WfcInterface : MonoBehaviour
{
    private const string inputMapSerializationPath = "Assets/Generated/";

    public int patternSize = 2;
    private int patternSizeCheck = 0;
    public int inputMapSize = 10;
    private int inputMapSizeCheck = 0;
    public int inputMapHeight = 1;
    private int inputMapHeightCheck = 0;

    public Vector3Int outputSize = new Vector3Int(20, 1, 20);
    private Vector3Int outputSizeCheck = Vector3Int.zero;
    public TileSet tileSet;
    public int selectedTile = 0;

    private Grid grid;
    private BoxCollider boxCollider;

    public WfcModel model;
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
        ResizeInputMap();
        ResizeOutputMap();
    }

    private void ResizeInputMap()
    {
        if (patternSize != patternSizeCheck || inputMapSize != inputMapSizeCheck || inputMapHeight != inputMapHeightCheck)
        {
            patternSizeCheck = patternSize;
            inputMapSizeCheck = inputMapSize;
            inputMapHeightCheck = inputMapHeight;
            inputMap = new Tilemap(inputMapSize, inputMapHeight, patternSize);
            model = new WfcModel(inputMap);

            if (boxCollider is null)
            {
                boxCollider = gameObject.GetComponent<BoxCollider>();
            }

            boxCollider.size = new Vector3(inputMapSize, 0, inputMapSize);
            boxCollider.center = new Vector3(inputMapSize / 2, 0, inputMapSize / 2);
        }

    }

    private void ResizeOutputMap()
    {
        if (outputSizeCheck != outputSize && model is not null)
        {
            model.SetOutputSize(outputSize.x, outputSize.y, outputSize.z);
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
        inputMap = new Tilemap(inputMapSize, inputMapHeight);
        model = new WfcModel(inputMap);

        grid = GetComponent<Grid>();
        grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

        boxCollider = GetComponent<BoxCollider>();

        ResizeInputMap();
        ResizeOutputMap();
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

    private Vector3 GetRotationOffset(Tile currentTile)
    {
        switch (currentTile.rotation)
        {
            case 1:
                return new Vector3(0, 0, 1);
            case 2:
                return new Vector3(1, 0, 1);
            case 3:
                return new Vector3(1, 0, 0);
            default:
                return Vector3.zero;
        }
    }

    private void DrawTile(Tile currentTile, Vector3 tilePos, Camera cam)
    {
        Mesh mesh = tileSet.GetMesh(currentTile.id);
        if (mesh)
        {
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 90 * currentTile.rotation, 0));

            Vector3 rotationOffset = GetRotationOffset(currentTile);

            Matrix4x4 currentTRS = Matrix4x4.TRS(transform.position + tilePos + rotationOffset,
                                                rotation,
                                                Vector3.one);

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

                selectedTile = Mathf.Abs((selectedTile - 1)) % tileSet.tiles.Count;
                break;

            case KeyCode.M:
                selectedTile = (selectedTile + 1) % tileSet.tiles.Count;
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

    public void SerializeInputMap()
    {       
        TilemapSerializer.SerializeTilemap(inputMap, inputMapSerializationPath);
    }

    public void LoadSerializedInputMap(string path)
    {
        inputMap = TilemapSerializer.DeserializeTilemap(path);
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


}

[CustomEditor(typeof(WfcInterface))]
public class WfcInterfaceEditor : Editor
{
    WfcInterface t;
    UnityEngine.Object obj = null;

    private void Awake()
    {
        t = (WfcInterface)target;
    }

    private void TrainingEditor()
    {

        GUILayout.BeginHorizontal();
        t.model.enablePatternReflection = EditorGUILayout.Toggle("Enable pattern reflection", t.model.enablePatternReflection);
        t.model.enablePatternRotations = EditorGUILayout.Toggle("Enable pattern rotation", t.model.enablePatternRotations);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        
        obj = EditorGUILayout.ObjectField( "Serialized input map file:", obj, typeof(UnityEngine.Object), false);


        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Load serialized input map") && obj)
        {
            t.LoadSerializedInputMap(AssetDatabase.GetAssetPath(obj));
        }        
        
        if(GUILayout.Button("Serialize current input map"))
        {
            t.SerializeInputMap();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button("Train"))
        {
            t.Train();
        }
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Clear"))
        {
            t.Clear();
        }

        GUILayout.Space(20);

        TrainingEditor();

        GUILayout.Space(10);

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
