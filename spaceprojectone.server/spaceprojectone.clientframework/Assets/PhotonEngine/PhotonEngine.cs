using ExitGames.Client.Photon;
using UnityEngine;
public class PhotonEngine : MonoBehaviour, IPhotonPeerListener
{
    public PhotonPeer Peer { get; protected set; }
    public GameState State { get; protected set; }
    public ViewController Controller { get; set;} 

    public string ServerAdress;
    public string ApplicationName;

    private static PhotonEngine _instance;
    public void Awake()
    {
        _instance = this;
    }

    public void Start()
    {
        DontDestroyOnLoad(this);
        State = new Disconnected(_instance);
        Application.runInBackground = true;
        Initialize();
    }

    public static PhotonEngine Instance
    {
        get { return _instance; }
    }

    public void Initialize()
    {
        Peer = new PhotonPeer(this, ConnectionProtocol.Udp);
        Peer.Connect(ServerAdress, ApplicationName);
        State = new WaitingForConnection(_instance);
    }

    public void Disconnect()
    {
        if(Peer != null)
        {
            Peer.Disconnect();
        }
        State = new Disconnected(_instance);
    }

    public void Update()
    {
        State.OnUpdate();
    }


    public void SendOp(OperationRequest request, bool sendReliable, byte channelId, bool encrypt)
    {
        State.SendOperation(request, sendReliable, channelId, encrypt);
    }


    public static void UseExistingOrCreateNewPhotonEngine(string serverAdress, string applicationName)
    {
        GameObject tempEngine;
        PhotonEngine myEngine;

        tempEngine = GameObject.Find("PhotonEngine");
        if(tempEngine == null)
        {
            tempEngine = new GameObject("PhotonEngine");
            tempEngine.AddComponent<PhotonEngine>();

        }

        myEngine = tempEngine.GetComponent<PhotonEngine>();
        myEngine.ApplicationName = applicationName;
        myEngine.ServerAdress = serverAdress;
    }

    #region Implementation of IPhotonPeerListener
    public void DebugReturn(DebugLevel level, string message)
    {
        Controller.DebugReturn(level, message);
    }

    public void OnEvent(EventData eventData)
    {
        Controller.OnEvent(eventData);
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        Controller.OnOperationResponse(operationResponse);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                Peer.EstablishEncryption();
                break;
            case StatusCode.Disconnect:
            case StatusCode.DisconnectByServer:
            case StatusCode.DisconnectByServerLogic:
            case StatusCode.DisconnectByServerUserLimit:
            case StatusCode.Exception:
            case StatusCode.ExceptionOnConnect:
            case StatusCode.TimeoutDisconnect:
                Controller.OnDisconnected(" " + statusCode);
                State = new Disconnected(_instance);
                break;
            case StatusCode.EncryptionEstablished:
                State = new Connected(_instance);
                break;
            default:
                Controller.OnUnexpectedStatusCode(statusCode);
                State = new Disconnected(_instance);
                break;

        }
    }
    #endregion
}
