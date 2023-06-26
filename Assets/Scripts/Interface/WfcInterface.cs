using UnityEngine;
using UnityEditor;
using WFC_Model;
using UnityEditor.Experimental.GraphView;

[ExecuteAlways]
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(BoxCollider))]
public class WfcInterface : MonoBehaviour
{
    private const string inputMapSerializationPath = "Assets/Generated/";

    public int patternSize = 2;

    public int inputMapSize = 10;
    private int inputMapSizeCheck = 0;

    public int inputMapHeight = 1;
    private int inputMapHeightCheck = 0;

    public int selectedTile = 0;

    public Vector3Int outputSize = new Vector3Int(20, 1, 20);
    private Vector3Int outputSizeCheck = Vector3Int.zero;

    private Vector3Int startingPoint;

    public int selectedLayer = 0;

    public bool selectOutputMap = false;

    public TileSet tileSet;

    private Grid grid;
    private BoxCollider boxCollider;

    public WfcModel model;
    private Tilemap inputMap;
    private Tilemap lastMapGenerated;



    private void DrawOutputMeshGizmos()
    {
        Vector3 pos = transform.position;
        for (int i = 0; i <= outputSize.x; i++)
        {
            for (int j = 0; j <= outputSize.z; j++)
            {

                Gizmos.DrawLine(new Vector3(startingPoint.x, selectedLayer, i + startingPoint.z) + pos, new Vector3(outputSize.x + startingPoint.x, selectedLayer, i + startingPoint.z) + pos);
                Gizmos.DrawLine(new Vector3(i + startingPoint.x, selectedLayer, startingPoint.z) + pos, new Vector3(i + startingPoint.x, selectedLayer, outputSize.z + startingPoint.z) + pos);

            }
        }
    }


    private void DrawInputMeshGizmos()
    {
        Vector3 pos = transform.position;
        for (int i = 0; i <= inputMapSize; i++)
        {
            Gizmos.DrawLine(new Vector3(0, selectedLayer, i) + pos, new Vector3(inputMapSize, selectedLayer, i) + pos);
            Gizmos.DrawLine(new Vector3(i, selectedLayer, 0) + pos, new Vector3(i, selectedLayer, inputMapSize) + pos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (selectOutputMap) { DrawOutputMeshGizmos(); }
        else
        {
            DrawInputMeshGizmos();
        }
    }

    private void OnValidate()
    {
        ResizeInputMap();
        ResizeOutputMap();
    }

    private void RefreshCollider()
    {
        if (boxCollider is null)
        {
            boxCollider = gameObject.GetComponent<BoxCollider>();
        }

        if (selectOutputMap)
        {
            boxCollider.size = new Vector3(outputSize.x, 0, outputSize.z);
            boxCollider.center = new Vector3((outputSize.x / 2) + startingPoint.x, selectedLayer, (outputSize.z / 2) + startingPoint.z);
        }
        else
        {
            boxCollider.size = new Vector3(inputMapSize, 0, inputMapSize);
            boxCollider.center = new Vector3(inputMapSize / 2, selectedLayer, inputMapSize / 2);
        }

        if (inputMapSize % 2 != 0)
        {
            boxCollider.center += new Vector3(.5f, 0, .5f);
        }
    }



    private void ResizeInputMap()
    {
        if (inputMapSize != inputMapSizeCheck || inputMapHeight != inputMapHeightCheck)
        {
            inputMapSizeCheck = inputMapSize;
            inputMapHeightCheck = inputMapHeight;
            inputMap = new Tilemap(inputMapSize, inputMapHeight, inputMapSize);
            model = new WfcModel(inputMap);

            RefreshCollider();
        }
    }

    private void ResizeOutputMap()
    {
        if (outputSizeCheck != outputSize && model is not null)
        {
            model.SetOutputSize(outputSize.x, outputSize.y, outputSize.z);
            startingPoint = new Vector3Int(inputMapSize + 1, 0, -outputSize.z / 2);
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
        lastMapGenerated = new Tilemap(outputSize.x, outputSize.y, outputSize.z);
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
                for (int k = 0; k < inputMapHeight; k++)
                {
                    Vector3 tilePos = new Vector3(i, k, j);
                    DrawTile(inputMap.GetTile(i, k, j), tilePos, cam);
                }
            }
        }
    }

    private void DrawGeneratedMap(Camera cam)
    {
        int outputX = lastMapGenerated.width;
        int outputY = lastMapGenerated.height;
        int outputZ = lastMapGenerated.depth;


        for (int i = 0; i < outputX; i++)
        {
            for (int k = 0; k < outputY; k++)
            {
                for (int j = 0; j < outputZ; j++)
                {
                    Vector3Int tilePos = startingPoint + new Vector3Int(i, k, j);
                    Tile tile = lastMapGenerated.GetTile(i, k, j);

                    DrawTile(tile, tilePos, cam);
                }
            }
        }
    }

    private void DeleteTile(Vector3Int pos)
    {
        if (selectOutputMap)
        {
            lastMapGenerated.SetTile(new Tile(0, 0), pos.x, pos.y, pos.z);
        }
        else
        {
            inputMap.SetTile(new Tile(0, 0), pos.x, pos.y, pos.z);
        }
    }

    private void PlaceTile(Vector3Int pos)
    {
        if (selectOutputMap)
        {
            lastMapGenerated.SetTile(new Tile(selectedTile, 0), pos.x, pos.y, pos.z);
        }
        else
        {
            inputMap.SetTile(new Tile(selectedTile, 0), pos.x, pos.y, pos.z);
        }
    }

    private void RotateTile(Vector3Int pos)
    {
        if (selectOutputMap)
        {
            lastMapGenerated.RotateAt(pos.x, pos.y, pos.z);
        }
        else
        {
            inputMap.RotateAt(pos.x, pos.y, pos.z);
        }
    }

    private Vector3Int GetCellCoords(Vector3 hitPoint)
    {
        Vector3Int output = grid.WorldToCell(hitPoint);

        if (selectOutputMap)
        {
            output -= startingPoint;
        }

        return output;

    }

    public void HandleClick(Vector3 mousePosition, int mouseButton)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Vector3Int cellPosition = GetCellCoords(hit.point);

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

    private void ChangeLayer(int layer)
    {
        if (layer != selectedLayer)
        {
            selectedLayer = layer;
            RefreshCollider();
        }
    }

    private int mod(int x, int y)
    {
        return x - y * (int)Mathf.Floor(x / y);
    }

    private void AlternateBetweenInputAndOutputMap()
    {
        selectOutputMap = !selectOutputMap;
        RefreshCollider();
    }

    public void HandleKeyPress(KeyCode keycode)
    {
        switch (keycode)
        {
            case KeyCode.O:
                AlternateBetweenInputAndOutputMap();
                break;

            case KeyCode.N:
                selectedTile = Mathf.Abs((selectedTile - 1)) % tileSet.tiles.Count;
                break;

            case KeyCode.M:
                selectedTile = (selectedTile + 1) % tileSet.tiles.Count;
                break;

            case KeyCode.K:
                ChangeLayer(mod(selectedLayer + 1, inputMapHeight));
                break;

            case KeyCode.J:
                ChangeLayer(Mathf.Abs(mod(selectedLayer - 1, inputMapHeight)));
                break;
        }
    }


    public void Clear()
    {
        inputMap.Clear();
        if (lastMapGenerated is not null) lastMapGenerated.Clear();
    }

    public void ClearOutputMap()
    {
        lastMapGenerated.Clear();
    }

    public void Train()
    {
        model.Train(inputMap, patternSize);
        Debug.Log(model.inputReader.GetPatternSummary());
    }

    public void Train3D()
    {
        model.Train(inputMap, patternSize);
    }

    public void SerializeInputMap()
    {
        TilemapSerializer.SerializeTilemap(inputMap, tileSet, inputMapSerializationPath);
    }

    public void LoadSerializedInputMap(string path)
    {
        inputMap = TilemapSerializer.DeserializeTilemap(path);
        tileSet = TilemapSerializer.DeserializeTileSet(path);
        inputMapSize = inputMap.depth;
        inputMapHeight = inputMap.height;
        RefreshCollider();
    }

    public void Generate()
    {
        Debug.Log(inputMap);
        lastMapGenerated = model.Generate(outputSize.x, outputSize.y, outputSize.z);
        Debug.Log(lastMapGenerated.ToString());
    }

    public void CompleteOutputMap()
    {
        Debug.Log(lastMapGenerated.ToString());
        lastMapGenerated = model.Generate(lastMapGenerated);
        Debug.Log(lastMapGenerated.ToString());
    }

}


#if UNITY_EDITOR
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

        GUILayout.BeginHorizontal();
        t.model.sandwichPatterns = EditorGUILayout.Toggle("Sandwich patterns", t.model.sandwichPatterns);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        t.model.horizontalPeriodicInput = EditorGUILayout.Toggle("Horizontal periodic input", t.model.horizontalPeriodicInput);
        t.model.verticalPeriodicInput = EditorGUILayout.Toggle("Vertical periodic input", t.model.verticalPeriodicInput);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        obj = EditorGUILayout.ObjectField("Serialized input map file:", obj, typeof(UnityEngine.Object), false);


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Load serialized input map") && obj)
        {
            t.LoadSerializedInputMap(AssetDatabase.GetAssetPath(obj));
        }

        if (GUILayout.Button("Serialize current input map"))
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

        if (GUILayout.Button("Clear outputMap"))
        {
            t.ClearOutputMap();
        }

        GUILayout.Space(20);

        TrainingEditor();

        GUILayout.Space(10);

        if (GUILayout.Button("Generate"))
        {
            t.Generate();
        }
        if (GUILayout.Button("Generate from incomplete output"))
        {
            t.CompleteOutputMap();
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
#endif