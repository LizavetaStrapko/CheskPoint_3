namespace Project3.BS.Interfaces
{
    public interface ITarif
    {
        string Name { get; }
        double FreeMinutes { get; }
        double CostPerMinutes { get; }
        bool CanHaveNegativeBalanse { get; }
    }
}
