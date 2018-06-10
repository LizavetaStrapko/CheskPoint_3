namespace Project3.ATS
{
    public class Request
    {
        public const int IncomingCall = 101;

        public const int OutcomingCall = 102;

        public const int DisconnectCall = 103;

        public Request(PhoneNumber caller, PhoneNumber receiver, int code)
        {
            Caller = caller;
            Receiver = receiver;
            Code = code;
        }

        public int Code { get; }

        public PhoneNumber Caller { get; }

        public PhoneNumber Receiver { get; }

        public override string ToString()
        {
            string res = "Request:";
            res += " Caller: " + Caller;
            res += " Receiver: " + Receiver;
            res += " Code: " + Code;
            return res;
        }
    }
}
