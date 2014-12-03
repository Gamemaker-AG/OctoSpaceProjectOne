using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;
using System;

namespace ComplexServer.Handlers
{
    public class RequestForwardHandler : DefaultRequestHandler
    {
        public RequestForwardHandler(PhotonApplication application) : base(application)
        {

        }
        public override MessageType Type
        {
            get { return MessageType.Request; }
        }

        public override byte Code
        {
            get { throw new NotImplementedException(); }
        }

        public override int? SubCode
        {
            get { return null; }
        }

        protected override bool OnHandleMessage(SpaceProjectOne.Framework.IMessage message, PhotonServerPeer serverPeer)
        {
            Log.ErrorFormat("No Existing RequestHandler");
            return true;
        }
    }
}
