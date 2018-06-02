using System;

namespace Project3.ATS.Interfaces
{
    public interface ITerminal : IEventsClear
    {
        PhoneNumber PhoneNumber { get; }

        event EventHandler<PhoneNumber> PhoneChanged;

        event EventHandler Plugging;

        event EventHandler UnPlugging;

        event EventHandler Online;

        event EventHandler Offline;

        //terminal connect to station
        event EventHandler<Request> OutConnection;
        
        //station connect to terminal
        event EventHandler<Request> IncomConnection;
        
        //terminal respond to station
        event EventHandler<Respond> IncomRespond;

        //dialog begin
        event EventHandler<Request> CallAccepted;
        
        //interrupt conection
        event EventHandler<Request> InterruptConnection;
        
        //drop connetion
        event EventHandler<Request> DropConnection;

        void Call(PhoneNumber target);

        void Drop();

        void Answer();

        void Interrupt();

        void Plug();

        void UnPlug();

        void IncomingRequest(Request incomingRequest);

        void IncomingRespond(Respond incomingRespond);
        
        void RegisterEventForPort(IPort port);
    }
}
