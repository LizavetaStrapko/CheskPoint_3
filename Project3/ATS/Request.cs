namespace Project3.ATS
{
    class Request
    {
        public const int IncomingCall = 101;
        public const int OutcomingCall = 102;
        public const int DisconnectCall = 103;
        public Request(PhoneNumber source, PhoneNumber target, int code)
        {
            this.Source = source;
            this.Target = target;
            this.Code = code;
        }

        public int Code { get; }

        public PhoneNumber Source { get; }
        public PhoneNumber Target { get; }

        public override string ToString()
        {
            string res = "Request:";
            res += " Source: " + Source;
            res += " Target: " + Target;
            res += " Code: " + Code;
            return res;
        }
    }
}
