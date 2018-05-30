using Project3.ATS.Interfaces;
using System;

namespace Project3.ATS
{
    class Terminal : ITerminal
    {
        private PhoneNumber _phoneNumber;
        public PhoneNumber PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                this._phoneNumber = value;
                OnPhoneChanged(this, this._phoneNumber);
            }
        }

        public Request Request { get; set; }
        public bool IsOnline { get; private set; }
        public bool IsPlugged { get; private set; }

        public Terminal(PhoneNumber phoneNumber)
        {
            this.PhoneNumber = phoneNumber;
            this.IsPlugged = false;
            this.IsOnline = false;
        }



        public void Plug()
        {
            OnPlugging(this, EventArgs.Empty);
        }
        public void UnPlug()
        {
            OnUnPlugging(this, EventArgs.Empty);
        }


        public void Call(PhoneNumber target)
        {
            if (!IsOnline)
            {
                //OnOnline(this, EventArgs.Empty);
                this.Request = new Request(this.PhoneNumber, target, Request.OutcomingCall);
                OnOutConnection(this, this.Request);
            }
        }
        public void Answer()
        {
            if (Request == null) return;
            var respond = new Respond(Respond.Accept, this.Request);
            OnIncomRespond(this, respond);
            // OnOnline(this, EventArgs.Empty);
        }
        //Сбросил
        public void Drop()
        {
            if (this.Request == null) return;
            var respond = new Respond(Respond.Drop, this.Request);
            OnIncomRespond(this, respond);
            //OnOffline(this, EventArgs.Empty);
        }

        //прервался
        public void Interrupt()
        {
            if (this.Request == null) return;
            this.Request = new Request(this.PhoneNumber, Request.Target, Request.DisconnectCall);
            OnOutConnection(this, Request);
            // OnOffline(this, EventArgs.Empty);
        }
        #region 
        //EVENTS
        public event EventHandler<PhoneNumber> PhoneChanged;
        public event EventHandler Plugging;
        public event EventHandler UnPlugging;
        public event EventHandler Online;
        public event EventHandler Offline;

        //terminal connect to station
        public event EventHandler<Request> OutConnection;
        //station connect to terminal
        public event EventHandler<Request> IncomConnection;
        //terminal respond to station
        public event EventHandler<Respond> IncomRespond;

        //dialog begin
        public event EventHandler<Request> CallAccepted;
        //interrupt conection
        public event EventHandler<Request> InterruptConnection;
        //drop connetion
        public event EventHandler<Request> DropConnection;


        protected virtual void OnPlugging(object sender, EventArgs args)
        {
            this.IsPlugged = true;
            Plugging?.Invoke(sender, args);
        }
        protected virtual void OnUnPlugging(object sender, EventArgs args)
        {
            this.IsPlugged = false;
            UnPlugging?.Invoke(sender, args);
        }

        protected virtual void OnPhoneChanged(object sender, PhoneNumber e)
        {
            PhoneChanged?.Invoke(sender, e);
        }

        protected virtual void OnOnline(object sender, EventArgs args)
        {
            Online?.Invoke(sender, args);
            this.IsOnline = true;
        }
        protected virtual void OnOffline(object sender, EventArgs args)
        {
            if (IsOnline)
                Offline?.Invoke(sender, args);
            Request = null;
            this.IsOnline = false;
        }

        protected virtual void OnOutConnection(object sender, Request e)
        {
            OutConnection?.Invoke(sender, e);
        }
        protected virtual void OnIncomConnection(object sender, Request e)
        {
            IncomConnection?.Invoke(sender, e);
        }
        protected virtual void OnIncomRespond(object sender, Respond e)
        {
            IncomRespond?.Invoke(sender, e);
        }

        protected virtual void OnCallAccepted(object sender, Request e)
        {
            CallAccepted?.Invoke(sender, e);
        }
        protected virtual void OnInterruptConnection(object sender, Request e)
        {
            InterruptConnection?.Invoke(sender, e);
        }
        protected virtual void OnDropConnection(object sender, Request e)
        {
            this.Request = null;
            DropConnection?.Invoke(sender, e);
        }

        #endregion


        public void IncomingRequest(Request incomingRequest)
        {
            this.Request = incomingRequest;
            OnIncomConnection(this, this.Request);

            Console.WriteLine("Current number:" + this.PhoneNumber);
            Console.WriteLine("Incoming Call From: " + Request.Source);

            Console.WriteLine("Actions:");
            Console.WriteLine("1. Accept");
            Console.WriteLine("2. Drop");

            int action = int.Parse(Console.ReadLine());

            switch (action)
            {
                case 1:
                    Answer();
                    break;
                case 2:
                    Drop();
                    break;
                default:
                    break;
            }
        }

        public void IncomingRespond(Respond incomingRespond)
        {
            OnIncomConnection(this, incomingRespond.Request);

            //Console.WriteLine("Current number:" + this.PhoneNumber);
            Console.WriteLine("Incoming Call From: " + incomingRespond.Request.Source);
            Console.WriteLine("Incoming Call To: " + incomingRespond.Request.Target);



            switch (incomingRespond.Code)
            {
                case Respond.Accept:
                    Console.WriteLine("Accepted");
                    OnCallAccepted(this, incomingRespond.Request);
                    break;
                case Respond.Drop:
                    Console.WriteLine("Dropped");
                    OnDropConnection(this, incomingRespond.Request);
                    break;
                default:
                    break;
            }
        }

        public void RegisterEventForPort(IPort port)
        {
            port.StateChanged += (sender, state) =>
            {
                if (!this.IsPlugged)
                {
                    if (state == PortState.Free)
                    {
                        this.OnPlugging(this, EventArgs.Empty);
                    }
                }
                if (this.IsPlugged)
                {
                    if (state == PortState.Off)
                    {
                        this.OnOffline(this, EventArgs.Empty);
                        OnUnPlugging(this, EventArgs.Empty);
                    }
                    if (!this.IsOnline && state == PortState.Busy)
                    {
                        this.OnOnline(this, EventArgs.Empty);
                    }
                    if (this.IsOnline && state == PortState.Free)
                    {
                        this.OnOffline(this, EventArgs.Empty);
                    }
                }


            };
        }

        public void RemoveEventHandlersForPort(IPort port)
        {
        }



        public void EventsClear()
        {
            this.OutConnection = null;
            this.IncomConnection = null;
            this.IncomRespond = null;
            this.Offline = null;
            this.Online = null;
            this.PhoneChanged = null;
            this.Plugging = null;
            this.UnPlugging = null;
        }


    }
}

   