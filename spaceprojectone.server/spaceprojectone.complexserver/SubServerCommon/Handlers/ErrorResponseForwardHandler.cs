using ComplexServerCommon;
using SpaceProjectOne.Framework;
using SpaceProjectOne.Photon.Application;
using SpaceProjectOne.Photon.Server;

namespace SubServerCommon.Handlers
{
    public class ErrorResponseForwardHandler : DefaultResponseHandler
    {
        public ErrorResponseForwardHandler(PhotonApplication application)
            : base(application)
        { }

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
            get { return null; }
        }

        protected override bool OnHandleMessage(IMessage message, PhotonServerPeer serverPeer)
        {
            Log.ErrorFormat("No existing Response Handler. {0} - {1}", message.Code, message.SubCode);
            return true;
        }
    }
}
