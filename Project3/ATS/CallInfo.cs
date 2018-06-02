using System;

namespace Project3.ATS
{
    public class CallInfo : EventArgs
    {
        public PhoneNumber Caller { get; set; }

        public PhoneNumber Receiver { get; set; }

        public DateTime Started { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
