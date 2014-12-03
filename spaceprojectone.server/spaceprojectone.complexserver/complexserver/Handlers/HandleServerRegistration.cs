using ComplexServerCommon;
using Photon.SocketServer;
using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;
using SubServerCommon.Data;
using SubServerCommon.Operations;
using System.IO;
using System.Xml.Serialization;

namespace SubServerCommon.Handlers
{
    public class HandleServerRegistration : PhotonServerHandler
    {
        public HandleServerRegistration(PhotonApplication application)
            : base(application)
        {

        }


        public override MessageType Type
        {
            get { return MessageType.Request; }
        }

        public override byte Code
        {
            get { return (byte)ServerOperationCode.RegisterSubServer; }
        }

        public override int? SubCode
        {
            get { return null; }
        }

        protected override bool OnHandleMessage(SpaceProjectOne.Framework.IMessage message, PhotonServerPeer serverPeer)
        {
            OperationResponse operationResponse;
            if (serverPeer.ServerId.HasValue)
            {
                operationResponse = new OperationResponse(message.Code) { ReturnCode = -1, DebugMessage = "Already Registered" };
            }
            else
            {
                var registerRequest = new RegisterSubServer(serverPeer.Protocol, message);
                if (!registerRequest.IsValid)
                {
                    string eMessage = registerRequest.GetErrorMessage();
                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Invalid Register Request: {0}", eMessage);
                    }
                    operationResponse = new OperationResponse(message.Code) { DebugMessage = eMessage, ReturnCode = (short)ErrorCode.OperationInvalid };
                }
                else
                {

                    XmlSerializer mySerializer = new XmlSerializer(typeof(RegisterSubServerData));
                    StringReader inStream = new StringReader(registerRequest.RegisterSubServerOperation);
                    var registerData = (RegisterSubServerData)mySerializer.Deserialize(inStream);

                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Received register request: Adress={0}, UdpPort={1}, TcpPort={2}, Type={3}",
                            registerData.GameServerAdress, registerData.UdpPort, registerData.TcpPort, registerData.ServerType);
                    }

                    if (registerData.UdpPort.HasValue)
                    {
                        serverPeer.UdpAdress = registerData.GameServerAdress + ":" + registerData.UdpPort;
                    }
                    if (registerData.TcpPort.HasValue)
                    {
                        serverPeer.TcpAdress = registerData.GameServerAdress + ":" + registerData.TcpPort;
                    }

                    serverPeer.ServerId = registerData.ServerId;
                    serverPeer.ServerType = registerData.ServerType;

                    serverPeer.ApplicationName = registerData.ApplicationName;

                    Server.ConnectionCollection.OnConnect(serverPeer);

                    operationResponse = new OperationResponse(message.Code);
                }
            }

            serverPeer.SendOperationResponse(operationResponse, new SendParameters());
            return true;
        }
    }
}
