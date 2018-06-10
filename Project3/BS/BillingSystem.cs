using Project3.ATS;
using Project3.ATS.Interfaces;
using Project3.BilS;
using Project3.BilS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project3.BS
{
    class BillingSystem
    {
        private ICollection<IAccount> _acccounts;

        private IDictionary<IAccount, ICollection<CallDetailing>> _callDetailing;

        public BillingSystem(ICollection<IAccount> acccounts, IDictionary<IAccount, ICollection<CallDetailing>> callDetailing)
        {
            _acccounts = acccounts;
            _callDetailing = callDetailing;
        }

        public void Add(IAccount account)
        {
            _acccounts.Add(account);
        }

        private void StationOnCallInfoAdded(object sender, CallInfo callInfo)
        {
            var fromAccount = GetAccount(callInfo.Caller);
            var toAccount = GetAccount(callInfo.Receiver);
            if (fromAccount == null) return;
            double outCost = fromAccount.AddIncomingMinutes(callInfo.Duration.TotalMinutes);

            if (toAccount == null) return;
            double inCost = toAccount.AddIncomingMinutes(callInfo.Duration.TotalMinutes);

            AddCallDetailing(fromAccount, new CallDetailing(callInfo, outCost));
            AddCallDetailing(toAccount, new CallDetailing(callInfo, inCost));
        }

        public void AddCallDetailing(IAccount account, CallDetailing callDetailing)
        {
            _callDetailing[account].Add(callDetailing);
        }

        public IAccount GetAccount(PhoneNumber phoneNumber)
        {
            return _acccounts.FirstOrDefault(account => account.Contract.PhoneNumber == phoneNumber);
        }

        public IAccount GetAccount(Client client)
        {
            return _acccounts.FirstOrDefault(IAccount => IAccount.Client == client);
        }

        public IEnumerable<CallDetailing> GetClientCalls(Client client, Func<CallDetailing, bool> predicate)
        {
            IAccount account = GetAccount(client);
            if (account == null)
                throw new Exception("IAccount can not be found");
            return _callDetailing[account].Where(predicate);
        }

        public void RegisterEventForAts(IStation station)
        {
            station.CallInfoAdded += StationOnCallInfoAdded;
        }
    }
}
