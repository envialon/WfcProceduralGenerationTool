using UnityEngine;
using UnityEditor;


namespace WFC_Procedural_Generator_Framework
{
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    public class WfcInterface : MonoBehaviour
    {
        public int patternSize = 2;
        public int inputMapSize = 10;
        public int inputMapHeight = 1;
        public Vector3Int outputSize = new Vector3Int(20, 1, 20);

        public TileSet tileSet;

        private InputTileMapData inputMap;
        private WfcModel model;
        private int[,,] lastMapGenerated;


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
            inputMap.SetTile(new Tile(2, 0), 4, 0, 4);
            model = new WfcModel(inputMap);

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
            }
            if (e.type == EventType.KeyDown)
            {
            }
        }
    }
}