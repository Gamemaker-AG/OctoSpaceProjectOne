using Autofac;
using ComplexServer;
using ComplexServerCommon;
using Photon.SocketServer;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;
using SubServerCommon;
using SubServerCommon.Data;
using SubServerCommon.Handlers;
using SubServerCommon.Operations;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace LoginServer
{
    public class LoginServer : PhotonApplication
    {
        private readonly IPAddress _publicIPAdress = IPAddress.Parse("127.0.0.1");
        private readonly IPEndPoint _masterEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4520);

        public override byte SubCodeParameterCode
        {
            get { return (byte)ClientParameterCode.SubOperationCode; }
        }

        public override System.Net.IPEndPoint MasterEndPoint
        {
            get { return _masterEndPoint; }
        }

        public override int? TcpPort
        {
            get { return 4531; }
        }

        public override int? UdpPort
        {
            get { return 5056; }
        }

        public override System.Net.IPAddress PublicIpAdress
        {
            get { return _publicIPAdress; }
        }

        public override int ServerType
        {
            get { return (int)SubServerCommon.ServerType.Login; }
        }

        protected override int ConnectRetryIntervalSeconds
        {
            get { return 14; }
        }

        protected override bool ConnectsToMaster
        {
            get { return true; }
        }

        protected override void RegisterContainerObjects(ContainerBuilder builder)
        {
            builder.RegisterType<ErrorEventForwardHandler>().As<DefaultEventHandler>().SingleInstance();
            builder.RegisterType<ErrorRequestForwardHandler>().As<DefaultRequestHandler>().SingleInstance();
            builder.RegisterType<ErrorResponseForwardHandler>().As<DefaultResponseHandler>().SingleInstance();
            builder.RegisterType<ComplexConnectionCollection>().As<PhotonConnectionCollection>().SingleInstance();
            builder.RegisterInstance(this).As<PhotonApplication>().SingleInstance();
        }

        protected override void ResolveParameters(IContainer container)
        {

        }

        public override void Register(SpaceProjectOne.Photon.Server.PhotonServerPeer peer)
        {
            var registerSubServerOperation = new RegisterSubServerData()
                {
                    GameServerAdress = PublicIpAdress.ToString(),
                    TcpPort = TcpPort,
                    UdpPort = UdpPort,
                    ServerId = ServerId,
                    ApplicationName = ApplicationName

                };
            XmlSerializer mySerializer = new XmlSerializer(typeof(RegisterSubServerData));
            StringWriter outString = new StringWriter();
            mySerializer.Serialize(outString, registerSubServerOperation);

            peer.SendOperationRequest(
                new OperationRequest( (byte)ServerOperationCode.RegisterSubServer, 
                new RegisterSubServer(){RegisterSubServerOperation = outString.ToString()}), new SendParameters());
        }
    }
}
