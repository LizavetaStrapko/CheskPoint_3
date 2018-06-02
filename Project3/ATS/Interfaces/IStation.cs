using System;

namespace Project3.ATS.Interfaces
{
    public interface IStation : IEventsClear
    {
        event EventHandler<CallInfo> CallInfoAdded;

        void RegisterEventForTerminal(ITerminal terminal);

        void RegisterEventForPort(IPort port);
    }
}
