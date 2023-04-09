using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    public class WfcModel
    {
        int outputX = 20;
        int outputY = 20;
        int outputZ = 20;
        int patternSize = 2;

        private InputReader inputReader;
        private WfcSolver solver;

        public WfcModel(InputTileMapData data)
        {
            inputReader = new InputReader(data, patternSize);
            solver = new WfcSolver(inputReader, outputX, outputY, outputZ);
        }
    }
}