namespace WFC_Tutorial
{
    public interface IInputReader<T>
    {
        IValue<T>[,] ReadInputToGrid();
    }
}
