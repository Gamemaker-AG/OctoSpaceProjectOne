// CS4455 networking sample.  Blair MacIntyre, 2013.
// 
// Demonstrates a strict authoritative server, where all content is created
// by the server, and interactions between objects are only reacted to on the server (with results sent 
// to the clients via RPC calls).
//
// Also demonstrates a simple use of the game lobby for setup.
//
// Some networking content drawn from the "Ultimate Unity networking project" by M2H (http://www.M2H.nl) and 
// from http://www.paladinstudios.com/2013/07/10/how-to-create-an-online-multiplayer-game-with-unity/
//

using UnityEngine;
using System.Collections;

public class Connect : MonoBehaviour
{
    // we'll use Unity's game lobby
    private const string typeName = "CS4455F13 Network Sample";
    private const string gameName = "Blair's Game";
    public int connectPort = 25001;

    private bool isRefreshingHostList = false;
    private HostData[] hostList;

    private string debugText;
    private bool hasShipSelected = false;
    private string MyShipId;

    // Supports both client and server setup
    void OnGUI()
    {

        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            // Currently disconnected: Neither client nor server
            GUILayout.Label("Connection status: Disconnected");

            int.TryParse(GUILayout.TextField(connectPort.ToString()), out connectPort);

            GUILayout.BeginVertical();
            if (GUILayout.Button("Start Server"))
            {
                StartServer();
            }

            if (GUILayout.Button("Refresh Hosts"))
            {
                // Connect to the "connectToIP" and "connectPort" as entered via the GUI
                RefreshHostList();
            }

            if (hostList != null)
            {
                for (int i = 0; i < hostList.Length; i++)
                {
                    if (GUI.Button(new Rect(200, 25 + (35 * i), 200, 30), hostList[i].gameName))
                        JoinServer(hostList[i]);
                }
            }
            GUILayout.EndVertical();
        }
        else
        {
            // One or more connection(s)!		

            if (Network.peerType == NetworkPeerType.Connecting)
            {
                GUILayout.Label("Connection status: Connecting");
            }
            else if (Network.peerType == NetworkPeerType.Client)
            {
                GUILayout.Label("Connection status: Client!");
                GUILayout.Label("Ping to server: " + Network.GetAveragePing(Network.connections[0]));


                if (GameManager.GameManagerObject != null)
                {

                    if (!hasShipSelected)
                    {
                        GameObject[] ships = GameObject.FindGameObjectsWithTag("Ship");

                        if (ships.Length > 0)
                        {
                            GUILayout.BeginVertical();
                            foreach (GameObject ship in ships)
                            {
                                if (GUILayout.Button("Connect to : " + ship.name))
                                {
                                    MyShipId = ship.name;
                                    hasShipSelected = true;
                                }
                            }
                            GUILayout.EndVertical();
                        }
                    }
                    else
                    {

                        GameManager.PlayerInfo pInfo;

                        GameManager.GameManagerObject.playerList.TryGetValue(Network.player, out pInfo);

                        if (pInfo == null)
                        {
                            GUILayout.BeginVertical();
                            if (GUILayout.Button("Connect as Helm"))
                            {
                                GameManager.GameManagerObject.ConnectToServer(1, MyShipId);
                            }
                            if (GUILayout.Button("Connect as Weapon"))
                            {
                                GameManager.GameManagerObject.ConnectToServer(2, MyShipId);
                            }
                            if (GUILayout.Button("Connect as Engineer"))
                            {
                                GameManager.GameManagerObject.ConnectToServer(3, MyShipId);
                            }
                            GUILayout.EndVertical();

                        }
                    }

                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Select other ship"))
                    {
                        MyShipId = null;
                        hasShipSelected = false;
                    }
                    GUILayout.EndVertical();
                }

            }
            else if (Network.peerType == NetworkPeerType.Server)
            {
                GUILayout.Label("Connection status: Server!");
                GUILayout.Label("Connections: " + Network.connections.Length);
                if (Network.connections.Length >= 1)
                {
                    GUILayout.Label("Ping to first player: " + Network.GetAveragePing(Network.connections[0]));
                }
            }

            if (GUILayout.Button("Disconnect"))
            {
                //GameManager GM = GameObject.Find("code").GetComponent<GameManager>();
                if (Network.isServer)
                {
                    if (GameManager.GameManagerObject != null) GameManager.GameManagerObject.ShutDownServer();
                }
                else
                {
                    if (GameManager.GameManagerObject != null) GameManager.GameManagerObject.ShutDownClient();
                }
                Network.Disconnect();
            }
        }
    }

    private void StartServer()
    {
        Network.InitializeServer(10, connectPort, !Network.HavePublicAddress());
        MasterServer.dedicatedServer = true;
        MasterServer.RegisterHost(typeName, gameName);
    }

    void Update()
    {
        if (isRefreshingHostList && MasterServer.PollHostList().Length > 0)
        {
            isRefreshingHostList = false;
            hostList = MasterServer.PollHostList();
        }
    }

    private void RefreshHostList()
    {
        if (!isRefreshingHostList)
        {
            isRefreshingHostList = true;
            MasterServer.RequestHostList(typeName);
        }
    }

    private void JoinServer(HostData hostData)
    {
        Network.Connect(hostData);
    }



    // some simple debugging

    // first, on the client
    void OnConnectedToServer()
    {
        Debug.Log("This CLIENT has connected to a server");
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("This CLIENT has disconnected from a server OR this SERVER was just shut down");
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Could not connect to server: " + error);
    }

    // second, on the server
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    void OnServerInitialized()
    {
        Debug.Log("Server initialized and ready");
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player disconnected from: " + player.ipAddress + ":" + player.port);
    }

    // other network callbacks
    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Debug.Log("Could not connect to master server: " + info);
    }

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        Debug.Log("New object instantiated by " + info.sender);
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        //Custom code here (your code!)
    }
}