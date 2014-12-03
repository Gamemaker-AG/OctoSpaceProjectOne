using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceProjectOne.Framework
{
    [Flags]
    public enum MessageType
    {
        //Values are chosen to be able to combine Messagetypes
        Request = 0x1,
        Response = 0x2,
        Async = 0x4
    }
}
