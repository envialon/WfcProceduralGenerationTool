using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WFC_Model;

public class TrainingTests
{
    WfcModel model;


    private Tilemap GenerateInputMap() {
        Tilemap tm = new Tilemap(10, 3);
        
        for(int i = 0; i < tm.map.Length; i++)
        {
            tm.map[i] = new Tile(UnityEngine.Random.Range(0, 12), UnityEngine.Random.Range(0, 4));
        }
        return tm;
    }



    [Test]
    public void TestDefault()
    {
        Tilemap tm = GenerateInputMap();
        model = new WfcModel(tm);
        model.Train(tm, 2);
    }

    [Test]
    public void TestReflections()
    {
        Tilemap tm = GenerateInputMap();
        model = new WfcModel(tm);
        model.enablePatternReflection = true;
        model.enablePatternRotations = false;
        model.sandwichPatterns = false;
        model.horizontalPeriodicInput = false;
        model.verticalPeriodicInput = false;
        model.Train(tm, 2);
        model.enablePatternReflection = false;
        model.Train(tm, 2);
    }
    
    [Test]
    public void TestRotations()
    {
        Tilemap tm = GenerateInputMap();
        model = new WfcModel(tm);
        model.enablePatternReflection = false;        
        model.sandwichPatterns = false;
        model.horizontalPeriodicInput = false;
        model.verticalPeriodicInput = false;
        model.Train(tm, 2);
        model.enablePatternRotations = false;
        model.Train(tm, 2);
        
    }

    [Test]
    public void TestNonSandwichPatterns()
    {
        Tilemap tm = GenerateInputMap();
        model = new WfcModel(tm);
        model.enablePatternReflection = false;
        model.enablePatternRotations = false;
        model.sandwichPatterns = false;
        model.horizontalPeriodicInput = false;
        model.verticalPeriodicInput = false;
        model.Train(tm, 2);
        model.sandwichPatterns = true;
        model.Train(tm, 2);
    }

    [Test]
    public void TestHorizontalPeriodic()
    {
        Tilemap tm = GenerateInputMap();
        model = new WfcModel(tm);
        model.enablePatternReflection = false;
        model.enablePatternRotations = false;
        model.sandwichPatterns = false;
        model.horizontalPeriodicInput = false;
        model.verticalPeriodicInput = false;
        model.Train(tm, 2);
        model.horizontalPeriodicInput = true;
        model.Train(tm, 2);
    }


    [Test]
    public void TestVerticalPeriodic()
    {
        Tilemap tm = GenerateInputMap();
        model = new WfcModel(tm);
        model.enablePatternReflection = false;
        model.enablePatternRotations = false;
        model.sandwichPatterns = false;
        model.horizontalPeriodicInput = false;
        model.verticalPeriodicInput = false;
        model.Train(tm, 2);
        model.verticalPeriodicInput = true;
        model.Train(tm, 2);
    }

}
