using ComplexServerCommon;
using Photon.SocketServer;
using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Client;
using SpaceProjectOne.Photon.Server;
using SpaceProjectOne.Photon.Application;
using System;

namespace ComplexServer.Handlers
{
    class ResponseForwardHandler : DefaultResponseHandler
    {
        public ResponseForwardHandler(PhotonApplication application)
            : base(application)
        {

        }

        public override MessageType Type
        {
            get { return MessageType.Response; }
        }

        public override byte Code
        {
            get { return (byte)(ClientOperationCode.Chat | ClientOperationCode.Login | ClientOperationCode.Region); }
        }

        public override int? SubCode
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool OnHandleMessage(IMessage message, PhotonServerPeer serverPeer)
        {
            if(message.Parameters.ContainsKey((byte) ClientParameterCode.PeerId))
            {
                Log.DebugFormat("Looking for Peer Id {0}", new Guid((byte[])message.Parameters[(byte)ClientParameterCode.PeerId]));
                PhotonClientPeer peer;
                Server.ConnectionCollection.Clients.TryGetValue(new Guid((byte[])message.Parameters[(byte)ClientParameterCode.PeerId]), out peer);
                if(peer != null)
                {
                    Log.DebugFormat("Found Peer");
                    message.Parameters.Remove((byte)ClientParameterCode.PeerId);
                    var response = message as PhotonResponse;
                    if(response != null)
                    {
                        peer.SendOperationResponse(new OperationResponse(response.Code, response.Parameters) { DebugMessage = response.DebugMessage, ReturnCode = response.ReturnCode }, new SendParameters());
                    }
                    else
                    {
                        peer.SendOperationResponse(new OperationResponse(message.Code, message.Parameters), new SendParameters());
                    }

                }
            }

            return true;
        }
    }
}
