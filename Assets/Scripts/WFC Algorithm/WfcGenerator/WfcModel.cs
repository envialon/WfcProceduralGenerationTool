using System;

namespace WFC_Model
{
    public class WfcModel
    {
        int patternSize = 2;

        public InputReader inputReader;
        public WfcSolver solver;

        public bool enablePatternReflection = false;
        public bool enablePatternRotations = true;

        public WfcModel(Tilemap data)
        {
            inputReader = new InputReader(data, patternSize);
        }
        
        public void Train(Tilemap inputTileMap, int patternSize = 2)
        {
            inputReader.Train(patternSize, inputTileMap, enablePatternReflection, enablePatternRotations);
            solver = new WfcSolver(inputReader);
        }

        public Tilemap Generate(int outputX, int outputY, int outputZ)
        {
            solver = new WfcSolver(inputReader, outputX, outputY, outputZ);
            return solver.Generate();
        }

        public void SetOutputSize(int outputX, int outputY, int outputZ)
        {
            if (solver != null)
            {
                solver.SetOutputSize(outputX, outputY, outputZ);
            }
        }

        public int GetNumberOfPatterns()
        {
            return inputReader.GetPatternInfo().Length;
        }
    }
}