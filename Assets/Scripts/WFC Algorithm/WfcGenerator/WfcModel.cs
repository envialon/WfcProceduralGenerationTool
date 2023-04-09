namespace WFC_Procedural_Generator_Framework
{
    public class WfcModel
    {
        int patternSize = 2;

        int outputX = 20;
        int outputY = 20;
        int outputZ = 20;

        private InputReader inputReader;
        private WfcSolver solver;

        public WfcModel(InputTileMapData data)
        {
            inputReader = new InputReader(data, patternSize);
           // solver = new WfcSolver(inputReader, outputX, outputY, outputZ);
        }

        public void Train(InputTileMapData inputTileMap, int patternSize = 2)
        {
            inputReader.Train(patternSize, inputTileMap);
        }

        public int[,,] Generate()
        {
            solver = new WfcSolver(inputReader, outputX, outputY, outputZ);
            return solver.Generate();
        }
    }
}