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
        }

        public int[,,] Generate(int outputX, int outputY, int outputZ)
        {
            solver = new WfcSolver(inputReader, outputX, outputY, outputZ);
            return solver.Generate();
        }
    }
}