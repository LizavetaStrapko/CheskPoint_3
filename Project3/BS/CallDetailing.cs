using Project3.ATS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3.BS
{
    public class CallDetailing
    {
        public CallInfo CallInfo { get; }

        public double Cost { get; }

        public CallDetailing(CallInfo callInfo, double cost)
        {
            CallInfo = callInfo;
            Cost = cost;
        }
    }
}
