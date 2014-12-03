using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceProjectOne.Framework
{
    public interface IHandler<T>
    {
        MessageType Type { get; }
        byte Code { get; }                                  //Corresponds to OperationCode in Photon
        int? SubCode { get; }                               //To be abled to differentiate between Messagedestinations(Async-Event with Chatmessage)
        bool HandleMessage(IMessage message, T peer);       
    }

}
