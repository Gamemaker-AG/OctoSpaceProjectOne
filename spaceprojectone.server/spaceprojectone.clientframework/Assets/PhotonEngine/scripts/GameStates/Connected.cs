public class Connected : GameState
{
    public Connected(PhotonEngine engine) : base(engine) { }

    public override void OnUpdate()
    {
        _engine.Peer.Service();
    }

    public override void SendOperation(ExitGames.Client.Photon.OperationRequest request, bool sendReliable, byte channelId, bool encrypt)
    {
        _engine.Peer.OpCustom(request, sendReliable, channelId, encrypt);
    }
}
