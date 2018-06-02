using Project3.ATS;
using Project3.BilS.Interfaces;
using System;

namespace Project3.BilS
{
    public class Contract : IContract
    {
        public Client Client { get; }

        public PhoneNumber PhoneNumber { get; }

        public DateTime AcceptedDate { get; }

        public IAccount Account { get; set; }

        public Contract(Client client, PhoneNumber phoneNumber, DateTime acceptedDate)
        {
            Client = client;
            PhoneNumber = phoneNumber;
            AcceptedDate = acceptedDate;
        }
    
    }
}
