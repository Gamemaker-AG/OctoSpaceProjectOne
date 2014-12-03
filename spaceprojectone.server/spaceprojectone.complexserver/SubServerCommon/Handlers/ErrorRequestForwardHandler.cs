using ComplexServerCommon;
using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;

namespace SubServerCommon.Handlers
{
    public class ErrorRequestForwardHandler : DefaultRequestHandler
    {
        public ErrorRequestForwardHandler(PhotonApplication application) : base(application)
        { }

        public override MessageType Type
        {
            get { return MessageType.Request; }
        }

        public override byte Code
        {
            get { return (byte)(ClientOperationCode.Chat |ClientOperationCode.Login |ClientOperationCode.Region ); }
        }

        public override int? SubCode
        {
            get { return null; }
        }

        protected override bool OnHandleMessage(IMessage message, PhotonServerPeer serverPeer)
        {
            Log.ErrorFormat("No existing Request Handler.");
            return true;
        }
    }
}
