using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;
using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Application;

namespace SpaceProjectOne.Photon.Client
{
    public abstract class PhotonClientHandler : IHandler<PhotonClientPeer>
    {
        public abstract MessageType Type { get; }
        public abstract byte Code { get; }
        public abstract int? SubCode { get; }
        protected PhotonApplication Server;
        protected ILogger Log = LogManager.GetCurrentClassLogger();

        public PhotonClientHandler(PhotonApplication application)
        {
            Server = application;
        }

        public bool HandleMessage(IMessage message, PhotonClientPeer peer)
        {
            OnHandleMessage(message, peer);
            return true;
        }

        protected abstract bool OnHandleMessage(IMessage message, PhotonClientPeer peer);


    }
}
