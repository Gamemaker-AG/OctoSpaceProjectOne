using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;
using SimpleCommon;
using Autofac;


namespace SimpleServer
{
    public class SimpleServer : SpaceProjectOne.Photon.Application.PhotonApplication
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
            builder.RegisterInstance(this).As<PhotonApplication>().SingleInstance();
            builder.RegisterType<NullConnectionCollection>().As<PhotonConnectionCollection>().SingleInstance();
            
            
            //AddHandlers
            //builder.RegisterType<LoginRequestHandler>().As<IClientHandler>().SingleInstance();
        }

        protected override void ResolveParameters(IContainer container)
        {
            //do nothing
        }

        public override void Register(PhotonServerPeer peer)
        {
            //do nothing because reasons: Just 1 Server
        }
    }
}
