namespace WFC_Model
{
    public class WfcModel
    {
        readonly int patternSize = 2;

        public InputReader inputReader;
        public WfcSolver solver;

        public bool enablePatternReflection = false;
        public bool enablePatternRotations = true;
        public bool sandwichPatterns = true;
        public bool horizontalPeriodicInput = true;
        public bool verticalPeriodicInput = true;

        public bool depthFirstPropagation = false;

        public WfcModel()
        {
        }

        public WfcModel(Tilemap inputMap)
        {
            inputReader = new InputReader(inputMap, patternSize);
        }

        public void ReadInput(Tilemap inputTileMap, int patternSize = 2)
        {
            inputReader.ReadInput(patternSize, inputTileMap,
                              enablePatternReflection,
                              enablePatternRotations,
                              sandwichPatterns,
                              horizontalPeriodicInput,
                              verticalPeriodicInput);
        }

        public Tilemap Generate(Tilemap incompleteMap)
        {
            
            solver = new WfcSolver(inputReader, incompleteMap.width, incompleteMap.height, incompleteMap.depth, depthFirstPropagation);
            return solver.Generate(incompleteMap);
        }

        public Tilemap Generate(int outputX, int outputY, int outputZ)
        {
            solver = new WfcSolver(inputReader, outputX, outputY, outputZ, depthFirstPropagation);
            return solver.Generate();
        }              

        public int GetNumberOfPatterns()
        {
            return inputReader.GetPatternInfo().Length;
        }
    }
}