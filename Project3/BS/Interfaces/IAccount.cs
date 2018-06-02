using Project3.BS;
using Project3.BS.Interfaces;

namespace Project3.BilS.Interfaces
{
    public interface IAccount
    {
        Client Client { get; }
        IContract Contract { get; }
        ITarif Tarif { get; }
        Statistics Statistics { get; }
        void Pay(double money);
        bool ChangeTarrif(ITarif newTarrif);
        double AddIncomingMinutes(double minutes);
        double AddOutcomingMinutes(double minutes);
    }
}
