using Project3.BilS.Interfaces;
using Project3.BS;
using Project3.BS.Interfaces;
using System;

namespace Project3.BilS
{
    public class Account : IAccount
    {
        public Client Client { get; }

        public IContract Contract { get; }

        public ITarif Tarif { get; private set; }

        public TimeSpan TarifChangePeriod { get; private set; }

        public TimeSpan PaymentPeriod { get; private set; }

        public Statistics Statistics { get; private set; }


        public Account(IContract contract, ITarif tarif, TimeSpan tarifChangePeriod, TimeSpan paymentPeriod)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (tarif == null) throw new ArgumentNullException(nameof(tarif));

            Contract = contract;
            Client = Contract.Client;

            Statistics = new Statistics()
            {
                LastDatePayment = Contract.AcceptedDate,
                LastChangePlan = Contract.AcceptedDate
            };

            Tarif = tarif;
            TarifChangePeriod = tarifChangePeriod;
            PaymentPeriod = paymentPeriod;
        }

        public void Pay(double money)
        {
            Statistics.Balans += money;
            Statistics.LastDatePayment = DateTime.Now;
        }

        public bool ChangeTarrif(ITarif newTarrif)
        {
            if (Tarif == newTarrif)
                return true;

            var curDate = DateTime.Now;

            if (Statistics.LastDatePayment + PaymentPeriod >= curDate)
            {
                Tarif = newTarrif;
                Statistics.LastChangePlan = curDate;
                return true;
            }
            return false;
        }

        public double AddIncomingMinutes(double minutes)
        {
            if (minutes < 0)
                throw new Exception("Minutes can not be negative");
            Statistics.IncomingMinutes += minutes;
            return 0;
        }

        public double AddOutcomingMinutes(double minutes)
        {
            if (minutes < 0)
                throw new Exception("Minutes can not be negative");

            double cost = 0;
            if (Statistics.LastDatePayment + TarifChangePeriod > DateTime.Now)
            {
                cost = Cost(minutes);
            }
            else if (Statistics.Balans > 0)
            {
                cost = Cost(minutes);
            }
            else if (Tarif.FreeMinutes - Statistics.OutcomingMinutes >= 0)
            {
                cost = Cost(minutes);
            }
            else if (Tarif.CanHaveNegativeBalanse)
            {
                cost = Cost(minutes);
            }
            else
            {
                cost = 0;
                minutes = 0;
            }
            Statistics.OutcomingMinutes += minutes;
            Statistics.TotalCost += cost;
            return cost;
        }

        public double Cost(double minutes)
        {
            double remainingFreeMinutes = Tarif.FreeMinutes - Statistics.OutcomingMinutes;
            if (remainingFreeMinutes > 0)
            {
                return Math.Max(minutes - remainingFreeMinutes, 0) * Tarif.CostPerMinutes;
            }
            else
                return minutes * Tarif.CostPerMinutes;
        }
    }
}
