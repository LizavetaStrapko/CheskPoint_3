using System;

namespace Project3.ATS.Interfaces
{
    public interface IPort : IEventsClear
    {
        PortState State { get; set; }

        event EventHandler<PortState> StateChanging;

        event EventHandler<PortState> StateChanged;

        void RegisterEventsForTerminal(ITerminal terminal);
    }
}
