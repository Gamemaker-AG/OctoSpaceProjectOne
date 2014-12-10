using UnityEngine;
using System.Collections;

public class Connect : MonoBehaviour
{
	// we'll use Unity's game lobby
    private const string typeName = "SPO UHH GMAG";
    private const string gameName = "The Masters Fantastic Game";
	public int connectPort = 25001;

    private bool isRefreshingHostList = false;
    private HostData[] hostList;
	
	private string debugText;
	

	// Supports both client and server setup
	void OnGUI ()
	{
   
		if (Network.peerType == NetworkPeerType.Disconnected)
		{
			// Currently disconnected: Neither client nor server
			GUILayout.Label("Connection status: Disconnected");
		
			int.TryParse(GUILayout.TextField(connectPort.ToString()), out connectPort);
		
			GUILayout.BeginVertical();
			if (GUILayout.Button ("Start Server"))
			{
                StartServer();
			}

			if (GUILayout.Button ("Refresh Hosts"))
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
		} else {
			// One or more connection(s)!		

			if (Network.peerType == NetworkPeerType.Connecting)
			{	
				GUILayout.Label("Connection status: Connecting");		
			} 
			else if (Network.peerType == NetworkPeerType.Client)
			{
				GUILayout.Label("Connection status: Client!");
				GUILayout.Label("Ping to server: " + Network.GetAveragePing(Network.connections[0] ) );
			} 
			else if (Network.peerType == NetworkPeerType.Server)
			{
				GUILayout.Label("Connection status: Server!");
				GUILayout.Label("Connections: " + Network.connections.Length);
				if (Network.connections.Length>=1) 
				{
					GUILayout.Label("Ping to first player: "+Network.GetAveragePing(  Network.connections[0] ) );
				}			
			}

			if (GUILayout.Button ("Disconnect"))
			{
				//NetworkManager GM = GameObject.Find("code").GetComponent<NetworkManager>();
				if (Network.isServer)
				{
					if (NetworkManager.SP != null) NetworkManager.SP.ShutDownServer();
				} else {
					if (NetworkManager.SP != null) NetworkManager.SP.ShutDownClient();
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
		Debug.Log ("This CLIENT has connected to a server");
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) 
	{
		Debug.Log("This CLIENT has disconnected from a server OR this SERVER was just shut down");
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		Debug.Log("Could not connect to server: "+ error);
	}

	// second, on the server
	void OnPlayerConnected(NetworkPlayer player) 
	{
		Debug.Log("Player connected from: " + player.ipAddress +":" + player.port);
	}

	void OnServerInitialized() 
	{
		Debug.Log("Server initialized and ready");
	}

	void OnPlayerDisconnected(NetworkPlayer player) 
	{
		Debug.Log("Player disconnected from: " + player.ipAddress+":" + player.port);
	}

	// other network callbacks
	void OnFailedToConnectToMasterServer(NetworkConnectionError info) 
	{
		Debug.Log("Could not connect to master server: "+ info);
	}

	void OnNetworkInstantiate (NetworkMessageInfo info) 
	{
		Debug.Log("New object instantiated by " + info.sender);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		//Custom code here (your code!)
	}
}