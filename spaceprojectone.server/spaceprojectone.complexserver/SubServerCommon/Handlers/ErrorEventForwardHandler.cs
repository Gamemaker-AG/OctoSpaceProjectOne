using ComplexServerCommon;
using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;

namespace SubServerCommon.Handlers
{
    public class ErrorEventForwardHandler : DefaultEventHandler
    {
        public ErrorEventForwardHandler(PhotonApplication application) : base(application)
        { }

        public override MessageType Type
        {
            get { return MessageType.Async; }
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
            Log.ErrorFormat("No existing Event Handler.");
            return true;
        }
    }
}
