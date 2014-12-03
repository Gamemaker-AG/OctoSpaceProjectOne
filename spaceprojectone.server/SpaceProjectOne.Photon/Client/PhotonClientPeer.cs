using ExitGames.Logging;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceProjectOne.Photon.Client
{
    public class PhotonClientPeer : PeerBase
    {
        protected ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Guid _peerID;
        private readonly Dictionary<Type, ClientData> _clientData = new Dictionary<Type, ClientData>();
        //private readonly Dictionary<byte, object> _clientData = new Dictionary<byte, object>();
        private readonly PhotonApplication _server;
        private readonly PhotonClientHandlerList _handlerList;
        public PhotonServerPeer CurrentServer { get; set; }

        #region Factory Method

        //public delegate PhotonClientPeer Factory(InitRequest initRequest);
        public delegate PhotonClientPeer Factory(IRpcProtocol protocol, IPhotonPeer photonPeer);

        #endregion

        public PhotonClientPeer(IRpcProtocol protocol, IPhotonPeer photonPeer, IEnumerable<ClientData> clientData, PhotonClientHandlerList handlerList, PhotonApplication application)
            : base(protocol, photonPeer)
        {
            _peerID = Guid.NewGuid();
            _handlerList = handlerList;
            _server = application;

            foreach (var data in clientData)
            {
                _clientData.Add(data.GetType(), data);
            }
            _server.ConnectionCollection.Clients.Add(_peerID, this);

        }



        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            _handlerList.HandleMessage(new PhotonRequest(operationRequest.OperationCode, operationRequest.Parameters.ContainsKey(_server.SubCodeParameterCode) ? (int?)Convert.ToInt32(operationRequest.Parameters[_server.SubCodeParameterCode]) : null, operationRequest.Parameters), this);
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            _server.ConnectionCollection.OnClientDisconnect(this);
            Log.DebugFormat("Client {0} disconnected", _peerID);
        }

        public Guid PeerId
        {
            get { return _peerID; }
        }

       public T ClientData<T>() where T : ClientData
        {
            ClientData result;
            _clientData.TryGetValue(typeof(T), out result);
            if (result != null)
            {
                return result as T;
            }
            return null;
        }

    }
}
