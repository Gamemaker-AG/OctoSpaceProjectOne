using SpaceProjectOne.Photon.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceProjectOne.Photon.Server
{
    public abstract class DefaultEventHandler : PhotonServerHandler
    {
        public DefaultEventHandler(PhotonApplication application)
            : base(application)
        {

        }
    }
}
