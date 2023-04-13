namespace WFC_Procedural_Generator_Framework
{
    public class WfcModel
    {
        int patternSize = 2;

        private InputReader inputReader;
        private WfcSolver solver;

        public WfcModel(Tilemap data)
        {
            inputReader = new InputReader(data, patternSize);
            // solver = new WfcSolver(inputReader, outputX, outputY, outputZ);
        }

        public void Train(Tilemap inputTileMap, int patternSize = 2)
        {
            inputReader.Train(patternSize, inputTileMap);
            solver = new WfcSolver(inputReader);
        }

        public int[,,] Generate(int outputX, int outputY, int outputZ)
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

        public int[,,] Iterate()
        {
            return solver.Iterate();
        }
    }
}