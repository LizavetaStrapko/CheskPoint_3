namespace Project3.ATS
{
    public class Respond
    {
        public const int Accept = 101;

        public const int Drop = 102;

        public const int Interrupt = 103;

        public Respond(int code, Request request)
        {
            Code = code;
            Request = request;
        }

        public int Code { get; }

        public Request Request { get; }

        public override string ToString()
        {
            string res = "Respond:";
            res += " Code: " + Code;
            res += " ";

            switch (Code)
            {
                case Accept:
                    res += "Accepted";
                    break;
                case Drop:
                    res += "Drop";
                    break;
                default:
                    res += "Undefined";
                    break;
            }

            res += " " + Request.ToString();
            return res;
        }
    }
}
