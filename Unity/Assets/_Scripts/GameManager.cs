using System.Collections.Generic;
using UnityEngine;
using System.Collections;

//DiesesObjekt liegt auf JEDEM Clienten/Server mit der networkViewID = 1

public class GameManager : MonoBehaviour
{
    public class PlayerInfo
    {
        // network name of the player
        public NetworkPlayer networkPlayer;
        public string name;

        // we might want to store a bunch of content per client
        public double clickTime;
        public Color color;

        // the GameObject that represents this players avatar in this example.  Could obviously store more info here.
        public GameObject go;

        public bool IsLocal()
        {
            //If disconnected we are "-1"
            return (Network.player == networkPlayer || Network.player + "" == "-1");
        }
    }

    public GameObject clientControlledPrefab;
    public GameObject dynamicPrefab;
    public static GameManager GameManagerObject;


    // Dictionary's for player information objects, and the list of networked game objects
    public Dictionary<NetworkPlayer, PlayerInfo> playerList = new Dictionary<NetworkPlayer, PlayerInfo>();
    public Dictionary<NetworkViewID, GameObject> myGOList = new Dictionary<NetworkViewID, GameObject>();

    // Verweis auf mich selbst erstellen und zusehen, dass das netzwerk läuft
    void Start()
    {
        GameManagerObject = this;
        Network.isMessageQueueRunning = true;
    }


    #region Handling Shutdown of Client and Server

    public void ShutDownServer()
    {
        if (Network.isServer)
        {
            Debug.Log("ShutDownServer");

            // create a separate list from the value contents of the dict, so we can delete elements from the dict
            // as we step through if we wanted (ended up not doing that here, just clear all at end).
            GameObject[] arr = new GameObject[myGOList.Count];
            myGOList.Values.CopyTo(arr, 0);

            foreach (GameObject go in arr)
            {
                if (go.networkView.isMine)
                    DestroyGameContent(go);
                else
                    Destroy(go);
            }
            myGOList.Clear();
        }
        else
        {
            Debug.Log("Called ShutDownServer on CLIENT.  Bad Programmer!");
        }
    }

    // shutdown client.  Clean up any data objects created by the server, any other client 
    // cleanup code could be here.
    public void ShutDownClient()
    {
        if (Network.isClient)
        {
            Debug.Log("ShutDownClient");

            // In theory, could call "RemovePlayer" as shown below, but don't need to because the server will call it when
            // we disconnect
            //networkView.RPC("RemovePlayer", RPCMode.All, Network.player);

            // create a separate list from the value contents of the dict, so we can delete elements from the dict
            // as we step through.  In particular, DestroyGameContent will remove things from the Dictionary.

            // do a network destroy of all the objects I created, and a normal destroy of the others.
            // We do the local Destroy of the others, because the server isn't going to destroy these.
            // The server WILL destroy our avatar object BUT we will probably have disconnected from the server
            // before we get the message.  
            GameObject[] arr = new GameObject[myGOList.Count];
            myGOList.Values.CopyTo(arr, 0);

            foreach (GameObject go in arr)
            {
                if (go.networkView.isMine)
                {
                    DestroyGameContent(go);
                }
                else
                {
                    // ideally we d
                    Destroy(go);
                }
            }
            myGOList.Clear();
        }
        else
        {
            Debug.Log("Called ShutDownClient on SERVER.  Bad Programmer!");
        }
    }

    #endregion

    #region Handling PlayerConnect and Disconnect

    [RPC]
    void AddPlayer(NetworkPlayer networkPlayer, string pname, Vector3 color)
    {
        Debug.Log("AddPlayer " + networkPlayer + " name=" + pname);
        if (playerList.ContainsKey(networkPlayer))
        {
            Debug.LogError("AddPlayer: Player " + networkPlayer + " already exists!");
            return;
        }

        PlayerInfo pla = new PlayerInfo();
        pla.networkPlayer = networkPlayer;
        pla.name = pname;
        pla.clickTime = 0;
        pla.go = null;
        pla.color = new Color(color.x, color.y, color.z);
        playerList[networkPlayer] = pla;
    }


    [RPC]
    void RemovePlayer(NetworkPlayer networkPlayer)
    {
        Debug.Log("RemovePlayer " + networkPlayer);
        PlayerInfo thePlayer;

        if (playerList.TryGetValue(networkPlayer, out thePlayer))
        {
            if (thePlayer.go != null)
            {
                // destroy the player go.
                // if we created it (which is if we are the server), destroy it everywhere
                // if we aren't the server (which is if we are disconnecting ourself), destroy the go because we
                // probably won't get the command back from the server when it executes RemovePlayer
                if (thePlayer.go.networkView.isMine)
                {
                    DestroyGameContent(thePlayer.go);
                }
                else
                {
                    myGOList.Remove(thePlayer.go.networkView.viewID);
                    Destroy(thePlayer.go);
                }
            }
            if (Network.isServer)
            {
                Network.RemoveRPCs(networkPlayer);
            }
            playerList.Remove(networkPlayer);
        }
        else
        {
            Debug.Log("RemovePlayer: player does not exist");
        }
    }

    #endregion

    #region UnityNetworkingEvents on Server

    //OnServerInitiation
    void OnServerInitialized()
    {
        // everyone has a copy of the server in their player list.  Buffered so new players get it.
        networkView.RPC
            (
                "AddPlayer",
                RPCMode.AllBuffered,
                Network.player,
                PlayerPrefs.GetString("playerName"),

                    new Vector3(
                        Random.Range(0.1f, 0.4f),
                        Random.Range(0.5f, 0.75f),
                        Random.Range(0.25f, 1f)
                                )
            );

        // create some server content, with two different behaviors, once the server is up and running
        //SpawnGameContent(1, Network.player, networkView.viewID);
        //SpawnGameContent(1, Network.player, networkView.viewID);
        //SpawnGameContent(1, Network.player, networkView.viewID);
    }

    //Nichts zu Tun hier, jeder Spieler fuegt sich selbst hinzu mit der AddPlayer Methode
    void OnPlayerConnected(NetworkPlayer player) { }


    //Hinter dem Spieler aufräumen, dabei muss dieser Aufruf nicht gebuffert werden, da der "AddPlayer" call aus dem Buffer gelöscht wird.
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        networkView.RPC("RemovePlayer", RPCMode.All, player);
    }

    #endregion

    #region UnityNetworkinEvents on Client

    void OnConnectedToServer()
    {
        //Beim Server Registrieren, Buffern, damit später dazustoßende Spieler auch den Aufruf von mir erhalten
        networkView.RPC
            (
                "AddPlayer",
                RPCMode.AllBuffered,
                Network.player,
                PlayerPrefs.GetString("playerName"),

                new Vector3(
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(0.1f, 0.5f)
                    )
            );

        // tell the server to create our content.  We do this separately from "AddPlayer" because 
        // it might eventually vary per client, and we want the eventual calls to create the content items to be 
        // initiated by the server, not the client (AddPlayer will get called on all new clients while we
        // are connected, but those buffered RPCs will disappear when we do)
        networkView.RPC("CreatePlayerSpecificContent", RPCMode.Server, Network.player, Network.AllocateViewID());

        // here, we could do some client-specific things. 
    }

    #endregion

    #region Spawning and Destruction of GameContent

    void SpawnGameContent(int prefabID, NetworkPlayer player, NetworkViewID viewId)
    {
        //Spawn local player
        Debug.Log("SpawnGameContent ");

        //Get random spawnpoint
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
        GameObject theGO = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = theGO.transform.position;
        Quaternion rot = theGO.transform.rotation;

        // Manually allocate a NetworkViewID.  This setup assumes ALL networking for each conceptual 
        // entity is done via the GO created below in SpawnOnNetwork, since each ViewID should only be used in
        // one GO
        NetworkViewID id1 = Network.AllocateViewID();
        // setup things locally, then issue the commands that will be sent to the clients when they
        // connect (this is all being done when the server starts, before the clients connect!)
        // In this simple example, exactly the same thing is done.  BUT, note that we pass in a different
        // value for our parameter "amOwner", so that we can tell if this is being executed here on the server
        // or being executed on the clients after starting
        SpawnOnNetwork(pos, rot, id1, viewId, true, player, prefabID);
        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, pos, rot, id1, viewId, false, player, prefabID);
    }

    void SetNetworkViewIDs(GameObject go, NetworkViewID id1, NetworkViewID viewId)
    {
        Component[] nViews = go.GetComponentsInChildren<NetworkView>();

        foreach (NetworkView nv in nViews)
        {
            if (nv.observed != go.GetComponent<StreamInput>())
            {
                nv.viewID = id1;  // just set the first
            }
            else if (nv.observed == go.GetComponent<StreamInput>())
            {
                nv.viewID = viewId;
            }
            else
            {
                nv.stateSynchronization = NetworkStateSynchronization.Off;
                nv.enabled = false;
            }
        }


    }

    void DestroyGameContent(GameObject go)
    {
        // only the owner of the GameObject should destroy it
        if (go.networkView.isMine)
        {
            // will be in the NetGOList, but construction!
            NetworkViewID viewID = go.networkView.viewID;

            Network.RemoveRPCs(viewID);   // get rid of buffered RPC calls for this object, if any

            DestroyOnNetwork(viewID, true);
            networkView.RPC("DestroyOnNetwork", RPCMode.OthersBuffered, viewID, false);
        }
        else
        {
            Debug.Log("DestroyGameContent: not owner of object " + go.ToString());
        }
    }

    #endregion

    #region RPCs

    // create the content for this client.  The RPC call is made from the server, because all shared game objects are created
    // in the server and managed there (so the server has control over them!)
    [RPC]
    void CreatePlayerSpecificContent(NetworkPlayer networkPlayer, NetworkViewID viewId)
    {
        if (!Network.isServer)
        {
            Debug.Log("Called CreatePlayerSpecificContent on Client, instead of Server");
            return;
        }

        if (networkPlayer == Network.player)
        {
            Debug.Log("Passed Server's network id to CreatePlayerSpecificContent instead of Clients");
            return;
        }

        // For this demo, we just spawn my player
        SpawnGameContent(2, networkPlayer, viewId);
    }


    [RPC]
    void SpawnOnNetwork(Vector3 pos, Quaternion rot, NetworkViewID id1, NetworkViewID viewId, bool amOwner, NetworkPlayer np, int prefabID)
    {
        GameObject newObject;
        PlayerInfo pNode;
        if (!playerList.TryGetValue(np, out pNode))
        {
            // probably need to do something more drastic here:  this should NEVER happen
            Debug.Log("SpawnOnNetwork of object #" + prefabID + " with NetWorkID " + id1.ToString() + " for NetworkPlayer " + np.ToString() + " failed, because network Player doesn't exist");
            return;
        }

        // would eventually be significantly more complex, I would think.  Can do different things in the players and server,
        // and could also do different things in each client.  But, this allows each conceptual "entity" to be created/destroyed
        // by the server in a simple way.
        switch (prefabID)
        {
            case 1:
                // create a server controlled, wandering cube
                newObject = Instantiate(dynamicPrefab, pos, rot) as GameObject;
                newObject.renderer.material.color = pNode.color;
                newObject.name = "cube" + id1.ToString();

                // add an offset to the script time, so that it's not the same for each.  
                // NOTE:  this only gets used on the Server (look at the script)
                if (np == Network.player)
                {
                    // server controlled object, so only need to set the time offset here
                    DynamicObjectScript ds = newObject.GetComponent<DynamicObjectScript>();
                    if (ds)
                    {
                        ds.offset = Random.Range(0f, 100f);
                    }
                    else
                    {
                        Debug.Log("Dynamic Object does not have DynamicObjectScript attached");
                    }
                }
                break;

            case 2:
                // create the content for the player avatar (another Cube!)
                newObject = Instantiate(clientControlledPrefab, pos, rot) as GameObject;
                newObject.renderer.material.color = pNode.color;
                newObject.name = "Player" + np.ToString();

                // save the player avatar in the player list here on the server
                pNode.go = newObject;

                // make a note of if this is my player (even though it's created by the server)
                PlayerControlledObjectScript ps = newObject.GetComponent<PlayerControlledObjectScript>();
                if (ps)
                {
                    ps.player = np;
                    ps.isMyPlayer = (np == Network.player);
                }
                else
                {
                    Debug.Log("Player Object does not have PlayerControlledObjectScript attached");
                }
                break;

            default:
                Debug.Log("Invalid prefab ID = " + prefabID);
                return;
        }

        // Set networkviewID everywhere in the game object. Just set the one, but it could be the case that eventually we
        // pass more than one along, and then need to set it in here.
        SetNetworkViewIDs(newObject.gameObject, id1, viewId);

        // keep track of our network game objects in a dictionary, so we can rapidly find objects based on their network
        // view ID.
        // In the "authoritative server" setup here, most (or all) network objects (especially those that can be moved around
        // by the game) are created by the server, but you could imagine the client creating other objects
        // that only they control

        myGOList.Add(id1, newObject);
    }

    [RPC]
    void DestroyOnNetwork(NetworkViewID id, bool amOwner)
    {
        GameObject go;
        // make sure it still exists, just in case we destroyed it locally (e.g., because we shutdown the client
        // and this message got here before the shutdown finished!)
        if (myGOList.TryGetValue(id, out go))
        {
            myGOList.Remove(id);
            Destroy(go);
        }
        else
        {
            Debug.Log("Tried to destroy Non-existant object with viewID " + id.ToString());
        }
    }

    #endregion



}
