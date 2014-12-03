using ExitGames.Client.Photon;
public class GameState : IGameState
    {

        protected PhotonEngine _engine;

        protected GameState(PhotonEngine engine)
        {
            _engine = engine;
        }
        public virtual void OnUpdate()
        {
            //do nothing
        }

        public virtual void SendOperation(ExitGames.Client.Photon.OperationRequest request, bool sendReliable, byte channelId, bool encrypt)
        {
            //do nothing
        }
    }

