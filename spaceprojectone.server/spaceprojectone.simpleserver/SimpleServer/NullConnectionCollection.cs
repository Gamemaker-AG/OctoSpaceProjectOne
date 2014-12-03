using Photon.SocketServer;
using SimpleServer.Codes;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleServer
{
    class NullConnectionCollection : PhotonConnectionCollection
    {
        public Dictionary<Guid, PhotonClientPeer> Characters { get; protected set;}


        public NullConnectionCollection() : base()
        {
            Characters = new Dictionary<Guid, PhotonClientPeer>();
        }
        public override bool IsServerPeer(InitRequest initRequest)
        {
            return false;
        }

        public override void ClientConnect(PhotonClientPeer clientPeer)
        {
            throw new NotImplementedException();
        }

        public override void ClientDisconnect(PhotonClientPeer clientPeer)
        {
            Log.InfoFormat("Logged out {0}", clientPeer.ClientData((byte)ClientDataCode.UserId]));
        }


        #region Unimplemented Methods
        public override void Disconnect(SpaceProjectOne.Photon.Server.PhotonServerPeer serverPeer)
        {
            throw new NotImplementedException();
        }

        public override void Connect(SpaceProjectOne.Photon.Server.PhotonServerPeer serverPeer)
        {
            throw new NotImplementedException();
        }

        public override void ResetServers()
        {
            throw new NotImplementedException();
        }
        
        public override SpaceProjectOne.Photon.Server.PhotonServerPeer OnGetServerByType(int serverType)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
