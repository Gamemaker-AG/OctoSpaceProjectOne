using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexServer.Codes
{
    public enum ServerEventCode : byte
    {
        SubServerList,
        CharacterChatServerRegister,
        CharacterRegionServerRegister,
        CharacterChatServerDeregister,
        CharacterRegionServerDeregister
    }
}
