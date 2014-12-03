using ComplexServer.ClientData;
using ComplexServer.Codes;
using ComplexServerCommon;
using Photon.SocketServer;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Client;
using SpaceProjectOne.Photon.Server;
using SubServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComplexServer
{
    public class ComplexConnectionCollection : PhotonConnectionCollection
    {

        public PhotonServerPeer LoginServer { get; protected set; }
        public PhotonServerPeer ChatServer { get; protected set; }


        public ComplexConnectionCollection()
        {
            LoginServer = null;
            ChatServer = null;
        }

        public override void Disconnect(PhotonServerPeer serverPeer)
        {
            if (serverPeer.ServerId.HasValue)
            {
                if (ChatServer != null && serverPeer.ServerId.Value == ChatServer.ServerId)
                {
                    ChatServer = null;
                }
                if (LoginServer != null && serverPeer.ServerId.Value == LoginServer.ServerId)
                {
                    LoginServer = null;
                }
            }
        }

        public override void Connect(PhotonServerPeer serverPeer)
        {
            if ((serverPeer.ServerType & (int)ServerType.Region) != 0)
            {
                Dictionary<byte, object> parameters = new Dictionary<byte, object>();
                Dictionary<string, string> serverList = Servers.Where(
                    incomingSubServerPeer =>
                        incomingSubServerPeer.Value.ServerId.HasValue
                        && !incomingSubServerPeer.Value.ServerId.Equals(serverPeer.ServerId)
                        && (incomingSubServerPeer.Value.ServerType & (int)ServerType.Region) != 0).
                        ToDictionary(
                        incomingSubServerPeer => incomingSubServerPeer.Value.ApplicationName,
                        incomingSubServerPeer => incomingSubServerPeer.Value.TcpAdress);

                if (serverList.Count > 0)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Sending lis of {0} connected sub servers", serverList.Count);
                    }
                    parameters.Add((byte)ComplexServer.Codes.ServerParameterCode.SubServerDictionary, serverList);
                    serverPeer.SendEvent(new EventData((byte)ComplexServer.Codes.ServerEventCode.SubServerList, parameters), new SendParameters());
                }
            }
        }

        public override void ClientConnect(PhotonClientPeer clientPeer)
        {
            if(clientPeer.ClientData<CharacterData>().CharacterId.HasValue)
            {
                var parameter = new Dictionary<byte, object>
                {
                    {(byte)ClientParameterCode.CharacterId, clientPeer.ClientData<CharacterData>().CharacterId.Value},
                    {(byte)ClientParameterCode.PeerId, clientPeer.PeerId}
                };

                ChatServer.SendEvent(new EventData((byte)ComplexServer.Codes.ServerEventCode.CharacterChatServerRegister, parameter), new SendParameters() );

                clientPeer.CurrentServer.SendEvent(new EventData((byte)ComplexServer.Codes.ServerEventCode.CharacterRegionServerRegister, parameter), new SendParameters());
            }
        }

        public override void ClientDisconnect(PhotonClientPeer clientPeer)
        {
            //Log.DebugFormat("Trying to disconnect client {0} : {1}", clientPeer.PeerId, clientPeer.ClientData<CharacterData>().CharacterId.Value);
            if(clientPeer.ClientData<CharacterData>().CharacterId.HasValue)
            {
                var parameter = new Dictionary<byte, object> {{(byte) ClientParameterCode.PeerId, clientPeer.PeerId}};
                Log.DebugFormat("Sending disconnect for client {0} : {1}", clientPeer.PeerId, clientPeer.ClientData<CharacterData>().CharacterId.Value);

                ChatServer.SendEvent(new EventData((byte)ComplexServer.Codes.ServerEventCode.CharacterChatServerDeregister, parameter), new SendParameters());
                clientPeer.CurrentServer.SendEvent(new EventData((byte)ComplexServer.Codes.ServerEventCode.CharacterRegionServerDeregister, parameter), new SendParameters());
            }
        }

        public override void ResetServers()
        {
            if (ChatServer != null && ChatServer.ServerType != (int)ServerType.Chat)
            {
                PhotonServerPeer peer = Servers.Values.Where(subServerPeer => subServerPeer.ServerType == (int)ServerType.Chat).FirstOrDefault();
                if (peer != null)
                {
                    ChatServer = peer;
                }
            }

            if (LoginServer != null && LoginServer.ServerType != (int)ServerType.Login)
            {
                PhotonServerPeer peer = Servers.Values.Where(subServerPeer => subServerPeer.ServerType == (int)ServerType.Login).FirstOrDefault();
                if (peer != null)
                {
                    LoginServer = peer;
                }
            }


            if (ChatServer == null && ChatServer.ServerId == null)
            {
                ChatServer = Servers.Values.Where(subServerPeer => subServerPeer.ServerType == (int)ServerType.Chat).FirstOrDefault() ??
                    Servers.Values.Where(subServerPeer => (subServerPeer.ServerType & (int)ServerType.Chat) != 0).FirstOrDefault();
            }

            if (LoginServer == null && LoginServer.ServerId == null)
            {
                LoginServer = Servers.Values.Where(subServerPeer => subServerPeer.ServerType == (int)ServerType.Login).FirstOrDefault() ??
                    Servers.Values.Where(subServerPeer => (subServerPeer.ServerType & (int)ServerType.Login) != 0).FirstOrDefault();
            }


            if (LoginServer != null)
            {
                Log.DebugFormat("Login Server: {0}", LoginServer.ConnectionId);
            }

            if (ChatServer != null)
            {
                Log.DebugFormat("Chat Server: {0}", ChatServer.ConnectionId);
            }
        }

        public override bool IsServerPeer(InitRequest initRequest)
        {
            Log.DebugFormat("Received init Request to {0}:{1} - {2}", initRequest.LocalIP, initRequest.LocalPort, initRequest);
            if(initRequest.LocalPort == 4520)
            {
                return true;
            }
            return false;
        }

        public override PhotonServerPeer OnGetServerByType(int serverType)
        {
            PhotonServerPeer server = null;
            switch((ServerType)Enum.ToObject(typeof(ServerType), serverType))
            {
                case ServerType.Login:
                    if(LoginServer != null)
                    {
                        Log.DebugFormat("Found Login Server");
                        server = LoginServer;
                    }
                    break;
                case ServerType.Chat:
                    if (ChatServer != null)
                    {
                        Log.DebugFormat("Found Chat Server");
                        server = ChatServer;

                    }
                    break;
            }

            return server;
        }
    }
}
