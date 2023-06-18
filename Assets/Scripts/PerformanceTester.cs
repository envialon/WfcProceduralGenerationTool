
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;


using WFC_Model;

public class PerformanceTester : MonoBehaviour
{
    public WfcModel model;

    public List<SerializableTilemap> tilemapsToTest;
    public bool serializeLog = false;

    private string log = "";

    private List<List<long>> trainingTimeTable = new List<List<long>>();
    private List<List<long>> generationTimeTable = new List<List<long>>();

    private int patternSize = 3;
    private Vector3Int outputSize = new Vector3Int(5, 5, 5);

    private void TestTilemap(int tilemapIndex, string name, bool enableRotations, bool enableReflections)
    {
        Stopwatch stopwatch = new Stopwatch();

        Tilemap tm = tilemapsToTest[tilemapIndex].GetTilemap();
        model = new WfcModel(tm);
        log += "Tilemap: " + name + "\n";
        log += "Input dimensions: " + "{ " + tm.width + ", " + tm.height + ", " + tm.depth + "}\n";
        log += "Pattern size: " + patternSize + "\n";
        log += "Trained with reflections: " + enableReflections + "\n";
        log += "Trained with rotations: " + enableRotations + "\n";
        log += "Output size: " + outputSize + "\n";

        model.enablePatternReflection = enableReflections;
        model.enablePatternRotations = enableReflections;

        stopwatch.Start();
        model.Train(tm, patternSize);
        stopwatch.Stop();
        trainingTimeTable[tilemapIndex].Add(stopwatch.ElapsedMilliseconds);

        stopwatch.Start();
        model.Generate(outputSize.x, outputSize.y, outputSize.z);
        stopwatch.Stop();
        generationTimeTable[tilemapIndex].Add(stopwatch.ElapsedMilliseconds);

        log += "Training Time: " + trainingTimeTable[tilemapIndex][trainingTimeTable[tilemapIndex].Count - 1] + "\n";
        log += "GenerationTime: " + generationTimeTable[tilemapIndex][generationTimeTable[tilemapIndex].Count - 1] + "\n";
        log += "\n\n";
    }

    private void SerializeLog()
    {
        if (serializeLog)
        {
            string path = "Assets/Logs/PerformanceTestLog.txt";
            System.IO.File.WriteAllText(path, log);
            AssetDatabase.Refresh();
        }
    }

    private string TableToString(List<List<long>> table)
    {
        string result = "";
        for (int i = 0; i < table.Count; i++)
        {
            for (int j = 0; j < table[i].Count; j++)
            {
                result += table[i][j] + " ";
            }
            result += "\n";
        }
        return result;
    }


    public string GetTimeTables()
    {
        string output = "";
        output += "Training time table: \n";
        output += TableToString(trainingTimeTable) + "\n";
        output += "Generation time table: \n";
        output += TableToString(generationTimeTable) + "\n";
        return output;
    }

    public void TestAllTilemapsOnce()
    {
        log = "";
        for (int i = 0; i < tilemapsToTest.Count; i++)
        {
            trainingTimeTable.Add(new List<long>());
            generationTimeTable.Add(new List<long>());


            TestTilemap(i, tilemapsToTest[i].name, false, false);
            TestTilemap(i, tilemapsToTest[i].name, false, true);
            TestTilemap(i, tilemapsToTest[i].name, true, false);
            TestTilemap(i, tilemapsToTest[i].name, true, true);
        }
        UnityEngine.Debug.Log(log);
        SerializeLog();
        UnityEngine.Debug.Log(GetTimeTables());
    }

    public string TestTilemapNTimes(SerializableTilemap stm, Vector3Int outputSize, int patternSize, int numberOfTests, bool enableRotations, bool enableReflections)
    {
        Tilemap tm = stm.GetTilemap();

        Stopwatch stopwatch;

        model = new WfcModel(tm);
        model.enablePatternReflection = enableReflections;
        model.enablePatternRotations = enableRotations;
        model.Train3D(tm, patternSize);
        
        int numberOfPatterns = model.GetNumberOfPatterns();
        long averageTrainingTime = 0;
        long averageGenerationTime = 0;

        for (int i = 0; i < numberOfTests; i++)
        {
            model = new WfcModel(tm);
            model.enablePatternReflection = enableReflections;
            model.enablePatternRotations = enableRotations;

            stopwatch = new Stopwatch();            
            stopwatch.Start();
            model.Train3D(tm, patternSize);
            stopwatch.Stop();
            averageTrainingTime += stopwatch.ElapsedMilliseconds;

            stopwatch = new Stopwatch();
            stopwatch.Start();
            model.Generate(outputSize.x, outputSize.y, outputSize.z);
            stopwatch.Stop();

            averageGenerationTime += stopwatch.ElapsedMilliseconds;
        }

        averageGenerationTime = averageGenerationTime / numberOfTests;
        averageTrainingTime = averageTrainingTime / numberOfTests;

        string summary = stm.name + "\t";
        summary += numberOfPatterns + "\t";
        summary += enableRotations + "\t";
        summary += enableReflections + "\t";
        summary += numberOfTests + "\t";
        summary += averageTrainingTime + "\t";
        summary += averageGenerationTime + "\t";
        return summary + "\n";
    }

    private string GetHeader()
    {
        return "Tilemap\tNumberOfPatterns\tEnableRotation\tEnableReflection\tNumberOfTests\tOutputSize\tAverageTrainingTime\tAverageGenerationTime\n";
    }

    public void TestAllNTimes()
    {
        const int numberOfTests = 5;
        int[] sizeMultipliers = { 1, 2, 3, 4, 5 };

        string summary = GetHeader();
        for (int i = 0; i < sizeMultipliers.Length; i++)
        {
            foreach (SerializableTilemap stm in tilemapsToTest)
            {
                summary += TestTilemapNTimes(stm,
                                             new Vector3Int(outputSize.x * sizeMultipliers[i],
                                                            outputSize.y * sizeMultipliers[i],
                                                            outputSize.z * sizeMultipliers[i]),
                                             patternSize,
                                             numberOfTests,
                                             false, false);
                summary += TestTilemapNTimes(stm,
                                            new Vector3Int(outputSize.x * sizeMultipliers[i],
                                                           outputSize.y * sizeMultipliers[i],
                                                           outputSize.z * sizeMultipliers[i]),
                                            patternSize,
                                            numberOfTests,
                                            false, true);
                summary += TestTilemapNTimes(stm,
                                            new Vector3Int(outputSize.x * sizeMultipliers[i],
                                                           outputSize.y * sizeMultipliers[i],
                                                           outputSize.z * sizeMultipliers[i]),
                                            patternSize,
                                            numberOfTests,
                                            true, false);
                summary += TestTilemapNTimes(stm,
                                            new Vector3Int(outputSize.x * sizeMultipliers[i],
                                                           outputSize.y * sizeMultipliers[i],
                                                           outputSize.z * sizeMultipliers[i]),
                                            patternSize,
                                            numberOfTests,
                                            true, true);
            }
        }

        UnityEngine.Debug.Log(summary);
    }

}

#if UNITY_EDITOR 
[CustomEditor(typeof(PerformanceTester))]
public class PerformanceTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Test all tilemaps once"))
        {
            ((PerformanceTester)target).TestAllTilemapsOnce();
        }
        if (GUILayout.Button("Test all tilemaps n times"))
        {
            ((PerformanceTester)target).TestAllNTimes();
        }
    }
}
#endif