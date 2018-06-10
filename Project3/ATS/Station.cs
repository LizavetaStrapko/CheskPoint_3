using Project3.ATS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project3.ATS
{
    public class Station : IStation
    {
        private ICollection<IPort> _ports;

        private ICollection<ITerminal> _terminals;

        private ICollection<CallInfo> _connectionCollection;

        private ICollection<CallInfo> _callCollection;

        //Коллекция пар порт-терминал
        private IDictionary<IPort, ITerminal> _portMap;

        public Station(ICollection<IPort> ports, ICollection<ITerminal> terminals)
        {
            _ports = ports;
            _terminals = terminals;
            _connectionCollection = new List<CallInfo>();
            _callCollection = new List<CallInfo>();
            _portMap = new Dictionary<IPort, ITerminal>();
        }

        public void MapPort(IPort port, ITerminal terminal)
        {
            if (port == null)
                throw new ArgumentNullException(nameof(port) + " is null");
            if (terminal == null)
                throw new ArgumentNullException(nameof(terminal) + " is null");
            if (_portMap.ContainsKey(port))
                throw new Exception("This port is already use");
            if (_portMap.Values.Contains(terminal))
                throw new Exception("This terminal is already use");

            _portMap.Add(port, terminal);
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
            return _terminals.FirstOrDefault(terminal => terminal.PhoneNumber == phoneNumber);
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
            if (!_ports.Contains(port))
                _ports.Add(port);
        }

        //Смотрит есть ли свободный терминал в портмэп, где собраны пары порт-терминал
        public void Add(ITerminal terminal)
        {
            var freePort = _ports.Except(_portMap.Keys).FirstOrDefault();
            if (freePort == null) return;

            if (_terminals.Any(term => term.PhoneNumber == terminal.PhoneNumber))
                throw new Exception("this number alredy used");

            if (!_terminals.Contains(terminal))
                _terminals.Add(terminal);

            MapPort(freePort, terminal);
            //для порта
            freePort.RegisterEventsForTerminal(terminal);
            terminal.RegisterEventForPort(freePort);
            //для станции
            RegisterEventForTerminal(terminal);
            RegisterEventForPort(freePort);
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
                    var connectInfoSource = GetLastConnectionInfo(request.Caller);
                    if (connectInfoSource != null)
                        InterruptConnection(connectInfoSource);
                    break;
                default:
                    break;
            }
        }

        public void RegisterCall(Request request)
        {
            if (request.Caller != default(PhoneNumber) && request.Receiver != default(PhoneNumber))
            {
                var callerTerminal = GetTerminal(request.Caller);
                var callerPort = GetPort(callerTerminal);

                var receiverTerminal = GetTerminal(request.Receiver);
                var receiverPort = GetPort(receiverTerminal);

                var callInfo = new CallInfo()
                {
                    Caller = request.Caller,
                    Receiver = request.Receiver,
                    Started = DateTime.Now,
                    Duration = TimeSpan.Zero
                };

                var callerConnection = GetLastConnectionInfo(request.Caller);
                var receiverConnection = GetLastConnectionInfo(request.Receiver);

                _connectionCollection.Add(callInfo);

                if ((callerConnection == null && receiverConnection == null)
                    && (callerPort.State != PortState.Off && receiverPort.State != PortState.Off))
                {
                    callerPort.State = PortState.Busy;
                    receiverPort.State = PortState.Busy;

                    var incomingRequest = new Request(request.Caller, request.Receiver, Request.IncomingCall);
                    receiverTerminal.IncomingRequest(incomingRequest);
                }
                else
                {
                    InterruptConnection(callInfo);
                    Console.WriteLine("Drop");
                    callerTerminal.IncomingRespond(new Respond(Respond.Drop, request));
                }
            }
        }

        public void OnIncomingCallRespond(object sender, Respond respond)
        {
            Console.WriteLine("Respond ok");
            Console.WriteLine(respond);

            var registeredCallInfo = GetLastConnectionInfo(respond.Request.Caller);
            if (registeredCallInfo == null) return;
            if (registeredCallInfo.Receiver != respond.Request.Receiver) return;

            var receiverTerminal = GetTerminal(respond.Request.Caller);
            receiverTerminal.IncomingRespond(respond);

            switch (respond.Code)
            {
                case Respond.Accept:
                    Console.WriteLine("Accept");
                    break;
                case Respond.Drop:
                    Console.WriteLine("Drop");
                    InterruptConnection(registeredCallInfo);
                    break;
                default:
                    break;
            }
        }

        protected void InterruptConnection(CallInfo connection)
        {
            var callerTerminal = GetTerminal(connection.Caller);
            var callerPort = GetPort(callerTerminal);

            var receiverTerminal = GetTerminal(connection.Receiver);
            var receiverPort = GetPort(receiverTerminal);

            var cl = GetLastConnectionInfo(connection.Receiver);
            if (connection == cl && receiverPort.State != PortState.Off)
            {
                connection.Duration = DateTime.Now - connection.Started;
                SetPortStateWhenConnectionInterrupted(connection.Caller, connection.Receiver);
            }
            else
                SetPortStateWhenConnectionInterrupted(connection.Caller, default(PhoneNumber));

            _connectionCollection.Remove(connection);
            AddCallInfo(connection);
        }

        protected void InterruptActiveCall(CallInfo connection)
        {
            connection.Duration = DateTime.Now - connection.Started;
            InterruptConnection(connection);
        }

        protected void SetPortStateWhenConnectionInterrupted(PhoneNumber caller, PhoneNumber receiver)
        {
            var callerPort = GetPort(GetTerminal(caller));
            if (callerPort?.State == PortState.Busy)
                callerPort.State = PortState.Free;

            var receiverPort = GetPort(GetTerminal(receiver));
            if (receiverPort?.State == PortState.Busy)
                receiverPort.State = PortState.Free;
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
            CallInfoAdded = null;
        }
    }
}
