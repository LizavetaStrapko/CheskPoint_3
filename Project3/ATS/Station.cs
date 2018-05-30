using Project3.ATS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project3.ATS
{
    class Station : IStation
    {
        private ICollection<IPort> _ports;
        private ICollection<ITerminal> _terminals;
        private ICollection<CallInfo> _connectionCollection;
        private ICollection<CallInfo> _callCollection;
        private IDictionary<IPort, ITerminal> _portMap;

        public Station(ICollection<IPort> ports, ICollection<ITerminal> terminals)
        {
            this._ports = ports;
            this._terminals = terminals;
            this._connectionCollection = new List<CallInfo>();
            this._callCollection = new List<CallInfo>();
            this._portMap = new Dictionary<IPort, ITerminal>();
        }

        public void MapPort(IPort port, ITerminal terminal)
        {
            if (port == null)
                throw new ArgumentNullException(nameof(port) + " is null");
            if (terminal == null)
                throw new ArgumentNullException(nameof(terminal) + " is null");
            if (this._portMap.ContainsKey(port))
                throw new Exception("This port is already use");
            if (this._portMap.Values.Contains(terminal))
                throw new Exception("This terminal is already use");

            this._portMap.Add(port, terminal);

        }
        public void UnmapPort(IPort port)
        {
            if (port == null) return;
            _portMap.Remove(port);
        }


        public IPort GetPort(ITerminal terminal)
        {
            return _portMap.FirstOrDefault(pair => pair.Value == terminal).Key;
        }
        public ITerminal GetTerminal(PhoneNumber phoneNumber)
        {
            return this._terminals.FirstOrDefault(terminal => terminal.PhoneNumber == phoneNumber);
        }

        public IEnumerable<CallInfo> GetCallInfo(PhoneNumber source)
        {
            return _callCollection.Where(info => (info.Caller == source || info.Receiver == source));
        }
        public CallInfo GetLastConnectionInfo(PhoneNumber source)
        {
            return _connectionCollection.FirstOrDefault(info => (info.Caller == source || info.Receiver == source));
        }

        public void AddCallInfo(CallInfo callInfo)
        {
            _callCollection.Add(callInfo);
            OnCallInfoAdded(this, callInfo);
        }


        public void Add(IPort port)
        {
            if (!this._ports.Contains(port))
                this._ports.Add(port);
        }
        public void Add(ITerminal terminal)
        {
            var freePort = this._ports.Except(_portMap.Keys).FirstOrDefault();
            if (freePort == null) return;

            if (this._terminals.Any(term => term.PhoneNumber == terminal.PhoneNumber))
                throw new Exception("this number alredy used");

            if (!this._terminals.Contains(terminal))
                _terminals.Add(terminal);

            MapPort(freePort, terminal);
            freePort.RegisterEventsForTerminal(terminal);
            terminal.RegisterEventForPort(freePort);
            this.RegisterEventForTerminal(terminal);
            this.RegisterEventForPort(freePort);
            //freePort.State = PortState.Free;
        }
        public void Remove(ITerminal terminal)
        {
            if (terminal == null) return;
            var port = GetPort(terminal);
            if (port == null) return;

            var connection = GetLastConnectionInfo(terminal.PhoneNumber);
            InterruptConnection(connection);

            UnmapPort(port);
            port.State = PortState.Off;
            port.EventsClear();
            terminal.EventsClear();
            _terminals.Remove(terminal);

        }

        public event EventHandler<CallInfo> CallInfoAdded;

        protected virtual void OnCallInfoAdded(object sender, CallInfo e)
        {
            CallInfoAdded?.Invoke(sender, e);
        }


        protected void RegisterOutgoingRequest(object sender, Request request)
        {
            Console.WriteLine("Terminal connect to station");


            switch (request.Code)
            {
                case Request.OutcomingCall:
                    Console.WriteLine("call");
                    RegisterCall(request);

                    break;
                case Request.DisconnectCall:
                    Console.WriteLine("disconect call");
                    var connectInfoSource = GetLastConnectionInfo(request.Source);
                    if (connectInfoSource != null)
                        InterruptConnection(connectInfoSource);
                    //InterruptActiveCall(connectInfoSource);

                    break;
                default:
                    break;
            }
        }

        public void RegisterCall(Request request)
        {
            if (request.Source != default(PhoneNumber) && request.Target != default(PhoneNumber))
            {
                var sourceTerminal = GetTerminal(request.Source);
                var sourcePort = GetPort(sourceTerminal);

                var targetTerminal = GetTerminal(request.Target);
                var targetPort = GetPort(targetTerminal);

                var callInfo = new CallInfo()
                {
                    Caller = request.Source,
                    Receiver = request.Target,
                    Started = DateTime.Now,
                    Duration = TimeSpan.Zero
                };

                var sourceConnection = GetLastConnectionInfo(request.Source);
                var targetConnection = GetLastConnectionInfo(request.Target);

                this._connectionCollection.Add(callInfo);

                if ((sourceConnection == null && targetConnection == null)
                    && (sourcePort.State != PortState.Off && targetPort.State != PortState.Off))
                {
                    sourcePort.State = PortState.Busy;
                    targetPort.State = PortState.Busy;

                    var incomingRequest = new Request(request.Source, request.Target, Request.IncomingCall);
                    targetTerminal.IncomingRequest(incomingRequest);
                }
                else
                {
                    InterruptConnection(callInfo);
                    Console.WriteLine("Drop");
                    sourceTerminal.IncomingRespond(new Respond(Respond.Drop, request));
                }
            }

        }
        public void OnIncomingCallRespond(object sender, Respond respond)
        {
            Console.WriteLine("Respond ok");
            Console.WriteLine(respond);

            var registeredCallInfo = GetLastConnectionInfo(respond.Request.Source);
            if (registeredCallInfo == null) return;
            if (registeredCallInfo.Receiver != respond.Request.Target) return;


            var targetTerminal = GetTerminal(respond.Request.Source);
            targetTerminal.IncomingRespond(respond);
            switch (respond.Code)
            {
                case Respond.Accept:
                    Console.WriteLine("Accept");
                    break;
                case Respond.Drop:
                    Console.WriteLine("Drop");
                    this.InterruptConnection(registeredCallInfo);
                    break;

                default:
                    break;
            }

        }

        protected void InterruptConnection(CallInfo connection)
        {
            var sourceTerminal = GetTerminal(connection.Caller);
            var sourcePort = GetPort(sourceTerminal);

            var targetTerminal = GetTerminal(connection.Receiver);
            var targetPort = GetPort(targetTerminal);

            var cl = GetLastConnectionInfo(connection.Receiver);
            if (connection == cl && targetPort.State != PortState.Off)
            {
                connection.Duration = DateTime.Now - connection.Started;
                SetPortStateWhenConnectionInterrupted(connection.Caller, connection.Receiver);
            }
            else
                SetPortStateWhenConnectionInterrupted(connection.Caller, default(PhoneNumber));

            this._connectionCollection.Remove(connection);
            AddCallInfo(connection);
        }

        protected void InterruptActiveCall(CallInfo connection)
        {
            connection.Duration = DateTime.Now - connection.Started;
            InterruptConnection(connection);
        }

        protected void SetPortStateWhenConnectionInterrupted(PhoneNumber source, PhoneNumber target)
        {
            var sourcePort = GetPort(GetTerminal(source));
            if (sourcePort?.State == PortState.Busy)
                sourcePort.State = PortState.Free;

            var targetPort = GetPort(GetTerminal(target));
            if (targetPort?.State == PortState.Busy)
                targetPort.State = PortState.Free;
        }


        public virtual void RegisterEventForTerminal(ITerminal terminal)
        {
            terminal.OutConnection += RegisterOutgoingRequest;
            terminal.IncomRespond += OnIncomingCallRespond;
        }
        public virtual void RegisterEventForPort(IPort port)
        {
            port.StateChanged += (sender, state) =>
            {
                Console.WriteLine("station: port changed state to " + port.State);
            };
        }


        public void EventsClear()
        {
            this.CallInfoAdded = null;
        }
    }
}
