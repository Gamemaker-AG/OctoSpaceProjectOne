using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;

namespace SpaceProjectOne.Photon.Server
{
    public abstract class PhotonServerHandler : IHandler<PhotonServerPeer>
    {
        public abstract MessageType Type { get; }
        protected readonly ILogger Log = LogManager.GetCurrentClassLogger(); 
        public abstract byte Code { get; }
        public abstract int? SubCode { get; }
        protected PhotonApplication Server;

        public PhotonServerHandler(PhotonApplication application)
        {
            Server = application;
        }

        public bool HandleMessage(IMessage message, PhotonServerPeer serverPeer)
        {
            OnHandleMessage(message, serverPeer);
            return true;
        }

        protected abstract bool OnHandleMessage(IMessage message, PhotonServerPeer serverPeer);
        
    }
}
