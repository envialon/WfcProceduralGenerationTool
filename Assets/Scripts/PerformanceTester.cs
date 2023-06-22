
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

    private int patternSize = 2;
    private Vector3Int outputSize = new Vector3Int(5, 5, 5);

    public string summary;

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

    private void SerializeSummary()
    {
        string path = "Assets/Logs/Summary.txt";
        System.IO.File.WriteAllText(path, summary);
        AssetDatabase.Refresh();
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
        model.Train(tm, patternSize);

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
            model.Train(tm, patternSize);
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
        summary += patternSize + "\t";
        summary += numberOfPatterns + "\t";
        summary += enableRotations + "\t";
        summary += enableReflections + "\t";
        summary += numberOfTests + "\t";
        summary += outputSize.ToString() + "\t";
        summary += (averageTrainingTime / 1000f) + "\t";
        summary += (averageGenerationTime / 1000f) + "\t";
        return summary + "\n";
    }

    private string GetHeader()
    {
        return "Tilemap\tPatternSize\tNumberOfPatterns\tEnableRotation\tEnableReflection\tNumberOfTests\tOutputSize\tAverageTrainingTime\tAverageGenerationTime\n";
    }

    public void TestAllNTimes()
    {
        const int numberOfTests = 1;
        Vector3Int[] sizes = {
            new Vector3Int(5, 5, 5),
            new Vector3Int(10,10,10), 
            new Vector3Int(11, 11, 11),
            new Vector3Int(12, 12, 12),
            new Vector3Int(13, 13, 13),
            new Vector3Int(14, 14, 14),
            new Vector3Int(15, 15, 15),
        };

        summary = GetHeader();
        for(patternSize =2; patternSize < 3; patternSize++) {
            for (int i = 0; i < sizes.Length; i++)
            {
                foreach (SerializableTilemap stm in tilemapsToTest)
                {
                    summary += TestTilemapNTimes(stm,
                                                 sizes[i],
                                                 patternSize,
                                                 numberOfTests,
                                                 false, false);
                    summary += TestTilemapNTimes(stm,
                                                sizes[i],
                                                patternSize,
                                                numberOfTests,
                                                false, true);
                    summary += TestTilemapNTimes(stm,
                                                sizes[i],
                                                patternSize,
                                                numberOfTests,
                                                true, false);
                    summary += TestTilemapNTimes(stm,
                                                sizes[i],
                                                patternSize,
                                                numberOfTests,
                                                true, true);
                }
            }
        }

        SerializeSummary();
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