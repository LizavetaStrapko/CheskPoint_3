using Project3.ATS;
using System;

namespace Project3.BilS.Interfaces
{
    public interface IContract
    {
        Client Client { get; }
        PhoneNumber PhoneNumber { get; }
        DateTime AcceptedDate { get; }
        IAccount Account { get; }
    }
}
