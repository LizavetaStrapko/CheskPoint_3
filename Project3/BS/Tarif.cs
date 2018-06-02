using Project3.BS.Interfaces;

namespace Project3.BS
{
    public class Tarif : ITarif
    {
        public string Name { get; }

        public double FreeMinutes { get; }

        public double CostPerMinutes { get; }

        public bool CanHaveNegativeBalanse { get; }
        
        public Tarif(string name, double freeMinutes, double costPerMinutes, bool canHaveNegativeBalanse)
        {
            Name = name;
            FreeMinutes = freeMinutes;
            CostPerMinutes = costPerMinutes;
            CanHaveNegativeBalanse = canHaveNegativeBalanse;
        }
    }
}
