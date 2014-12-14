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

        // Spielerinfos können hier abgespeichert werden
        public double clickTime;
        public Color color;

        // Das SpielerObjekt
        public GameObject go;

        public bool IsLocal()
        {
            //"-1" wenn wir nicht verbunden sind
            return (Network.player == networkPlayer || Network.player + "" == "-1");
        }
    }

    public GameObject clientControlledPrefab;
    public GameObject dynamicPrefab;
    public static GameManager GameManagerObject;


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

    public void ShutDownClient()
    {
        if (Network.isClient)
        {
            Debug.Log("ShutDownClient");

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
        // Jedem Spieler die Serverinstanz zukommen lassen. Buffered, damit auch neue Spieler dies bekommen
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

        // Hier kann man AI, Umgebungs oder andere vom Server gesteuerte Objekte erzeugen
        // SpawnGameContent(1, Network.player, networkView.viewID);
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

        networkView.RPC("CreatePlayerSpecificContent", RPCMode.Server, Network.player, Network.AllocateViewID());

        //Wie immer, hier den Kram initiieren, der NUR auf der SpielerInstanz läuft
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

        //Manuell die viewID vom Server Festlegen, damit ist gewährleistet, das alle dieselbe für das Objekt erhalten und die Besitzrechte geklärt sind
        NetworkViewID id1 = Network.AllocateViewID();

        // Den ganzen Kram lokal erstellen und dann an den Klienten senden.
        // amOwner == true -> läuft auf Server || amOwner == false -> läuft auf Client (wird hier nicht sooft benötigt)

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
                nv.viewID = id1; //Die ViewID des Servers
            }
            else if (nv.observed == go.GetComponent<StreamInput>())
            {
                nv.viewID = viewId; //Die PlayerView für den InputStream
            }
            else
            {
                nv.stateSynchronization = NetworkStateSynchronization.Off;
                nv.enabled = false;

                //Fehlverhalten von nicht korrekt eingestellen networkViews abfangen
            }
        }


    }

    void DestroyGameContent(GameObject go)
    {
        // Nur der Besitzer darf sein Objekt zerstören
        if (go.networkView.isMine)
        {

            NetworkViewID viewID = go.networkView.viewID;

            Network.RemoveRPCs(viewID);   // Alle RPC Calls löschen

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

        //Momentan nur das Schiff spawnen
        SpawnGameContent(2, networkPlayer, viewId);
    }


    [RPC]
    void SpawnOnNetwork(Vector3 pos, Quaternion rot, NetworkViewID id1, NetworkViewID viewId, bool amOwner, NetworkPlayer np, int prefabID)
    {
        GameObject newObject;
        PlayerInfo pNode;
        if (!playerList.TryGetValue(np, out pNode))
        {
            Debug.Log("SpawnOnNetwork of object #" + prefabID + " with NetWorkID " + id1.ToString() + " for NetworkPlayer " + np.ToString() + " failed, because network Player doesn't exist");
            return;
        }

        switch (prefabID)
        {
            #region case 1: WIRD MOMENTAN NICHT BENÖTIGT
            case 1:
                newObject = Instantiate(dynamicPrefab, pos, rot) as GameObject;
                newObject.renderer.material.color = pNode.color;
                newObject.name = "cube" + id1.ToString();

                if (np == Network.player)
                {
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
            #endregion

            case 2:

                newObject = Instantiate(clientControlledPrefab, pos, rot) as GameObject;
                newObject.renderer.material.color = pNode.color;
                newObject.name = "Player" + np.ToString();


                pNode.go = newObject;


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


        SetNetworkViewIDs(newObject.gameObject, id1, viewId);
        myGOList.Add(id1, newObject);
    }

    [RPC]
    void DestroyOnNetwork(NetworkViewID id, bool amOwner)
    {
        GameObject go;
        //Erst checken ob es noch existier, dann löschen
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
