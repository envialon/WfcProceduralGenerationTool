using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VisualizationTest : MonoBehaviour
{
    public MeshRenderer rend;
    public MeshFilter filter;
    public WfcInterface wfc;

    public void SetMaterials(TileSet tileSet)
    {
        Material[] materials= new Material[tileSet.tiles.Count];
        for(int i = 1; i < tileSet.tiles.Count; i++)
        {
            materials[i] = tileSet.GetMaterial(i);
        }
        rend.materials = materials;
    }

}


#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(VisualizationTest))]
public class VisualizationTestEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        VisualizationTest test = (VisualizationTest)target;
        if (GUILayout.Button("Generate"))
        {
            Debug.Log("");
            test.filter.mesh = test.wfc.CreateMeshFromOutput();
            test.SetMaterials(test.wfc.tileSet);
        }
    }
}

#endif