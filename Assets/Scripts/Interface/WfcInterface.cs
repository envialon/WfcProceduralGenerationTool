using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using WFC_Model;
using UnityEngine.UI;
using Codice.Utils;
using UnityEngine.AI;
using System.Data.SqlTypes;
using UnityEngine.Rendering;
using System;
using System.Linq;
using static UnityEditor.PlayerSettings;

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

    private Mesh[] mirroredMeshes;

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
        if (cam && tileSet)
        {
            DrawInputMap(cam);
            if (lastMapGenerated != null)
            {
                DrawGeneratedMap(cam);
            }

        }
    }

    private Vector3 GetRotationOffset(Tile currentTile)
    {
        switch ((currentTile.rotation, currentTile.reflected))
        {
            case (1, false):
                return new Vector3(0, 0, 1);
            case (2, false):
                return new Vector3(1, 0, 1);
            case (3, false):
                return new Vector3(1, 0, 0);
            case (0, true):
                return new Vector3(1, 0, 0);
            case (2, true):
                return new Vector3(0, 0, 1);

            case (3, true):
                return new Vector3(1, 0, 1);
            default:
                return Vector3.zero;
        }
    }

    private Quaternion GetRotationFromTile(Tile currentTile)
    {
        int rotation = currentTile.rotation;



        return Quaternion.Euler(new Vector3(0, 90 * rotation, 0));
    }

    private Matrix4x4 GetTransformMatrixFromTile(Tile currentTile, Vector3 tilePos)
    {
        Quaternion rotation = GetRotationFromTile(currentTile);

        Vector3 rotationOffset = GetRotationOffset(currentTile);

        return Matrix4x4.TRS(transform.position + tilePos + rotationOffset,
                                            rotation,
                                            Vector3.one);


    }

    private void DrawTile(Tile currentTile, Vector3 tilePos, Camera cam)
    {
        Mesh mesh = tileSet.GetMesh(currentTile.id);
        if (mesh)
        {

            Matrix4x4 currentTRS = GetTransformMatrixFromTile(currentTile, tilePos);
            Vector3 s = currentTRS.lossyScale;
            bool needToConvertCulling = Mathf.Sign(s.x * s.y * s.z) < 0;
            new CommandBuffer().SetInvertCulling(needToConvertCulling);

            if (currentTile.reflected)
            {
                Graphics.DrawMesh(mirroredMeshes[currentTile.id],
                            currentTRS, tileSet.GetMaterial(currentTile.id), 0, cam);
            }
            else
            {
                Graphics.DrawMesh(tileSet.GetMesh(currentTile.id),
                            currentTRS, tileSet.GetMaterial(currentTile.id), 0, cam);
            }


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


    public Mesh[] CreateMeshGroups()
    {
        int outputX = lastMapGenerated.width;
        int outputY = lastMapGenerated.height;
        int outputZ = lastMapGenerated.depth;

        Dictionary<int, List<(Tile, Vector3Int)>> tilesById = new Dictionary<int, List<(Tile, Vector3Int)>>();

        for (int i = 1; i < tileSet.tiles.Count; i++)
        {
            if (tileSet.GetMesh(i) is not null)
            {
                tilesById.Add(i, new List<(Tile, Vector3Int)>());
            }
        }

        for (int i = 0; i < outputX; i++)
        {
            for (int k = 0; k < outputY; k++)
            {
                for (int j = 0; j < outputZ; j++)
                {
                    Tile tile = lastMapGenerated.GetTile(i, k, j);
                    if (tilesById.ContainsKey(tile.id))
                    {
                        tilesById[tile.id].Add((tile, new Vector3Int(i, k, j)));
                    }
                }
            }
        }

        Mesh[] submeshes = new Mesh[tilesById.Count];
        int index = 0;
        foreach (KeyValuePair<int, List<(Tile, Vector3Int)>> keyValuePair in tilesById)
        {
            int currentId = keyValuePair.Key;
            List<(Tile, Vector3Int)> tileList = keyValuePair.Value;


            List<CombineInstance> combineByTile = new List<CombineInstance>();
            Debug.Log("adfs");
            Mesh mesh = tileSet.GetMesh(currentId);
            if (mesh is null) continue;

            CombineInstance template = new CombineInstance();
            template.mesh = new Mesh();
            template.mesh.vertices = mesh.vertices;
            template.mesh.triangles = mesh.triangles;
            template.mesh.uv = mesh.uv;
            template.mesh.normals = mesh.normals;
            template.mesh.colors = mesh.colors;
            template.mesh.tangents = mesh.tangents;


            if (template.mesh is null && mesh is not null) throw new System.Exception();

            foreach ((Tile currentTile, Vector3Int currentPos) in tileList)
            {
                if (mesh is not null)
                {
                    template.transform = GetTransformMatrixFromTile(currentTile, currentPos);
                    combineByTile.Add(template);

                }
            }
            submeshes[index] = new Mesh();
            submeshes[index].CombineMeshes(combineByTile.ToArray(), mergeSubMeshes: true, useMatrices: true);
            index++;
        }
        return submeshes;


    }

    public Mesh CreateMeshFromOutput()
    {
        Mesh[] submeshes = CreateMeshGroups();

        CombineInstance[] combine = new CombineInstance[submeshes.Length];
        int index = 0;
        foreach (Mesh mesh in submeshes)
        {
            combine[index].mesh = new Mesh();
            combine[index].mesh.vertices = mesh.vertices;
            combine[index].mesh.triangles = mesh.triangles;
            combine[index].mesh.uv = mesh.uv;
            combine[index].mesh.normals = mesh.normals;
            combine[index].mesh.colors = mesh.colors;
            combine[index].mesh.tangents = mesh.tangents;
            index++;
        }

        Mesh outputMesh = new Mesh();
        outputMesh.CombineMeshes(combine, mergeSubMeshes: false, useMatrices: false);
        return outputMesh;
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
            lastMapGenerated.SetTile(new Tile(selectedTile, 0, tileSet.GetSymmetry(selectedTile)), pos.x, pos.y, pos.z);
        }
        else
        {
            inputMap.SetTile(new Tile(selectedTile, 0, tileSet.GetSymmetry(selectedTile)), pos.x, pos.y, pos.z);
        }
    }

    private void ReflectTile(Vector3Int pos)
    {
        if (selectOutputMap)
        {
            lastMapGenerated.ReflectAt(pos.x, pos.y, pos.z);
        }
        else
        {
            inputMap.ReflectAt(pos.x, pos.y, pos.z);
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

    private Tile GetClickedOnTile(int x, int y, int z)
    {
        Tilemap tm;
        if (selectOutputMap)
        {
            tm = lastMapGenerated;
        }
        else
        {
            tm = inputMap;
        }
        return tm.GetTile(x, y, z);
    }

    public void HandleClick(Vector3 mousePosition, int mouseButton)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Vector3Int cellPosition = GetCellCoords(hit.point);

            Debug.Log(cellPosition);

            //Debug.Log("Hit at: " + hit.point + " Corresponds to cell " + cellPosition);
            if (mouseButton == 1)
            {
                if (selectedTile == GetClickedOnTile(cellPosition.x, cellPosition.y, cellPosition.z).id)
                {
                    ReflectTile(cellPosition);
                }
                else
                {
                    DeleteTile(cellPosition);
                }
            }
            else if (selectedTile == GetClickedOnTile(cellPosition.x, cellPosition.y, cellPosition.z).id)
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

    public void SerializeInputMap()
    {
        TilemapSerializer.SerializeTilemap(inputMap, tileSet, inputMapSerializationPath);
    }

    public void LoadSerializedInputMap(string path)
    {
        ClearOutputMap();
        inputMap = TilemapSerializer.DeserializeTilemap(path);
        tileSet = TilemapSerializer.DeserializeTileSet(path);
        inputMapSize = inputMap.depth;
        inputMapHeight = inputMap.height;
        RefreshCollider();
    }

    private Mesh MirrorMesh(in Mesh mesh)
    {
        Mesh output = new Mesh();

        Vector3 mirrorVector = new Vector3(-1, 1, 1);

        Vector3[] vertices = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Scale(mesh.vertices[i], mirrorVector);
        }
        output.vertices = vertices;

        vertices = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.normals.Length; i++)
        {
            vertices[i] = Vector3.Scale(mesh.normals[i], mirrorVector);
        }
        output.normals = vertices;

        //output.normals = mesh.normals;
        output.triangles = Enumerable.Reverse(mesh.triangles).ToArray();
        output.uv = mesh.uv;
        output.normals = mesh.normals;
        output.colors = mesh.colors;
        output.tangents = mesh.tangents;

        return output;
    }

    public void CreateMirroredMeshes()
    {
        int numberOfTiles = tileSet.tiles.Count;
        mirroredMeshes = new Mesh[numberOfTiles];
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (tileSet.tiles[i].mesh && tileSet.tiles[i].mesh.vertexCount > 0)
            {
                mirroredMeshes[i] = MirrorMesh(tileSet.tiles[i].mesh);
            }
        }
    }

    public void Generate()
    {
        Debug.Log(inputMap);
        lastMapGenerated = model.Generate(outputSize.x, outputSize.y, outputSize.z);
        Debug.Log(lastMapGenerated.ToString());
        CreateMirroredMeshes();
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