using UnityEditor;
using UnityEngine;
using WFC_Model;

[RequireComponent(typeof(Grid))]
public class WfcGenerator : MonoBehaviour
{
    public WfcModel model;

    private void OnValidate()
    {
        model = new WfcModel();
    }

    public void ReadInput(Tilemap inputMap, int patternSize)
    {
        model = new WfcModel(inputMap);
        model.ReadInput(inputMap, patternSize);
        //Debug.Log(model.inputReader.GetPatternSummary());
    }

    public Tilemap Generate(Tilemap inputTilemap, Tilemap mapToComplete)
    {
        ReadInput(inputTilemap, 2);
        return model.Generate(mapToComplete);
    }

    public Tilemap Generate(Tilemap inputTilemap, Vector3Int outputSize)
    {
        ReadInput(inputTilemap, 2);
        return model.Generate((int)outputSize.x, (int)outputSize.y, (int)outputSize.z);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WfcGenerator))]
public class WfcGeneratorEditor : Editor
{
    WfcGenerator t;
    private void Awake()
    {
        t = (WfcGenerator)target;
    }
    public override void OnInspectorGUI()
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

        GUILayout.BeginHorizontal();
        t.model.depthFirstPropagation = EditorGUILayout.Toggle("Deep-First propagation", t.model.depthFirstPropagation);
        GUILayout.EndHorizontal();


        GUILayout.Space(20);

    }
}
#endif