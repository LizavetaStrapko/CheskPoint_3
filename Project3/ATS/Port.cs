using Project3.ATS.Interfaces;
using System;

namespace Project3.ATS
{
    public enum PortState 
    {
        Free,
        Busy,
        Off
    }

    class Port : IPort
    {
        private PortState _state = PortState.Off;
        public PortState State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state == value) return;
                OnStateChanging(this, value);
                _state = value;
                OnStateChanged(this, _state);
            }
        }

        public void EventsClear()
        {
            this.StateChanged = null;
            this.StateChanging = null;
        }

        public event EventHandler<PortState> StateChanging;
        public event EventHandler<PortState> StateChanged;
        public void RegisterEventsForTerminal(ITerminal terminal)
        {
            terminal.Plugging += (sender, args) => { this.State = PortState.Free; };
            terminal.UnPlugging += (sender, args) => { this.State = PortState.Off; };
            terminal.OutConnection += (sender, request) =>
            {
                if (request.Code == Request.OutcomingCall && this.State == PortState.Free)
                {
                    this.State = PortState.Busy;
                }
            };
        }

        protected virtual void OnStateChanging(object sender, PortState e)
        {
            StateChanging?.Invoke(sender, e);
        }
        protected virtual void OnStateChanged(object sender, PortState e)
        {
            StateChanged?.Invoke(sender, e);
        }

    }
}
