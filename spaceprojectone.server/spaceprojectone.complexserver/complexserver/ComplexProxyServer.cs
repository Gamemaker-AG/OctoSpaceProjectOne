using Autofac;
using ComplexServer.ClientData;
using ComplexServer.Handlers;
using ComplexServerCommon;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;
using SubServerCommon.Handlers;
using System;

namespace ComplexServer
{
    public class ComplexProxyServer : PhotonApplication
    {
        public override byte SubCodeParameterCode
        {
            get { return (byte)ClientParameterCode.SubOperationCode; }
        }

        public override System.Net.IPEndPoint MasterEndPoint
        {
            get { throw new NotImplementedException(); }
        }

        public override int? TcpPort
        {
            get { throw new NotImplementedException(); }
        }

        public override int? UdpPort
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Net.IPAddress PublicIpAdress
        {
            get { throw new NotImplementedException(); }
        }

        public override int ServerType
        {
            get { throw new NotImplementedException(); }
        }

        protected override int ConnectRetryIntervalSeconds
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ConnectsToMaster
        {
            get { return false; }
        }

        protected override void RegisterContainerObjects(ContainerBuilder builder)
        {
            builder.RegisterType<ComplexConnectionCollection>().As<PhotonConnectionCollection>().SingleInstance();
            builder.RegisterInstance(this).As<PhotonApplication>().SingleInstance();
            builder.RegisterType<CharacterData>().As<SpaceProjectOne.Framework.ClientData>();
            builder.RegisterType<EventForwardHandler>().As<DefaultEventHandler>().SingleInstance();
            builder.RegisterType<RequestForwardHandler>().As<DefaultRequestHandler>().SingleInstance();
            builder.RegisterType<ResponseForwardHandler>().As<DefaultResponseHandler>().SingleInstance();
            builder.RegisterType<HandleServerRegistration>().As<PhotonServerHandler>().SingleInstance();
        }

        protected override void ResolveParameters(IContainer container)
        {

        }

        public override void Register(SpaceProjectOne.Photon.Server.PhotonServerPeer peer)
        {

        }
    }
}
