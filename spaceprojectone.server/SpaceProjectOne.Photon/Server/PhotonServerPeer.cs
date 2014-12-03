using SpaceProjectOne.Photon.Application;
using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceProjectOne.Photon.Server
{
    public class PhotonServerPeer : ServerPeerBase
    {
        private readonly PhotonServerHandlerList _handlerList;
        protected readonly PhotonApplication Server;
        public Guid? ServerId { get; set; }
        public string TcpAdress { get; set; }
        public string UdpAdress { get; set; }
        public string ApplicationName { get; set; }
        public int ServerType { get; set; }

        #region Factory Method

        public delegate PhotonServerPeer Factory(IRpcProtocol protocol, IPhotonPeer photonPeer);

        public PhotonServerPeer(IRpcProtocol protocol, IPhotonPeer photonPeer, PhotonServerHandlerList handlerList, PhotonApplication application) : base(protocol, photonPeer)
        {
            _handlerList = handlerList;
            Server = application;
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            _handlerList.HandleMessage(new PhotonRequest(operationRequest.OperationCode, operationRequest.Parameters.ContainsKey(Server.SubCodeParameterCode) ? (int?)Convert.ToInt32(operationRequest.Parameters[Server.SubCodeParameterCode]) : null, operationRequest.Parameters), this);
        }
        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            _handlerList.HandleMessage(new PhotonEvent(eventData.Code, eventData.Parameters.ContainsKey(Server.SubCodeParameterCode) ? (int?)Convert.ToInt32(eventData.Parameters[Server.SubCodeParameterCode]) : null, eventData.Parameters), this);
        }

        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            _handlerList.HandleMessage(new PhotonResponse(operationResponse.OperationCode, operationResponse.Parameters.ContainsKey(Server.SubCodeParameterCode) ? (int?)Convert.ToInt32(operationResponse.Parameters[Server.SubCodeParameterCode]) : null, operationResponse.Parameters, operationResponse.DebugMessage, operationResponse.ReturnCode), this);

        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            //Server.ConnectionCollection.OnDisconnect(this);
        }
        #endregion
    }
}
