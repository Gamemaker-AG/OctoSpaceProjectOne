using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{

    // Die Host-Informationen
    // Diese werden sowohl auf dem Server als auch auf dem Klienten gespeichert
    // 
    // We could make this different in the client and the server, by subclassing (for example) and 
    // creating a different one in the clients and the server.  
    public class PlayerInfo
    {
        // Netzwerkname des Spielers
        public NetworkPlayer networkPlayer;
        public string name;

        // Hier werden Informationen gespeichert, die jeder Client hat
        public double health;
        public Color color; //Just for fun

        // Das GameObject, das den verbundenen Spieler repraesentiert
        public GameObject go;

        public bool IsLocal()
        {
            //"-1" fuer den Fall, dass wir vom Server getrennt sind
            return (Network.player == networkPlayer || Network.player + "" == "-1");
        }
    }

    //Trickloesung um von anderen Skripten aus, auf dieses Object zuzugreifen
    public static NetworkManager SP;

    // Prefabs. Ein fuer den Klienten und einen fuer den Server.
    // Alle Prefabs die generiert werden sollen, muessen hier aufgelistet werden.
    public GameObject clientControlledPrefab;
    public GameObject cubePrefab;
    public GameObject clientHitCount;
    public GameObject serverHitCount;

    // Dictionary's fuer Player Information Objects und Liste der Netzwerkgenerierten Objekte
    public Dictionary<NetworkPlayer, PlayerInfo> playerList = new Dictionary<NetworkPlayer, PlayerInfo>();
    public Dictionary<NetworkViewID, GameObject> myGOList = new Dictionary<NetworkViewID, GameObject>();

    // Pointer auf dieses Objekt und das Netzwerk beleben
    void Awake()
    {
        SP = this;
        Network.isMessageQueueRunning = true;
    }

    #region  Player verwalten

    [RPC]
    void AddPlayer(NetworkPlayer networkPlayer, string playerName, Vector3 color)
    {
        Debug.Log("Adding " + networkPlayer + " with name=" + playerName);
        if (playerList.ContainsKey(networkPlayer))
        {
            Debug.LogError("AddPlayer failed: Player " + networkPlayer + " already exists!");
            return;
        }

        PlayerInfo pla = new PlayerInfo();
        pla.networkPlayer = networkPlayer;
        pla.name = playerName;
        pla.health = 0;
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
                // Spieler-Objekt loeschen
                // Fuer den Fall, dass wir das Objekt erstellt haben, was der Fall ist, wenn wir der Server sind, loeschen wir es
                // Fuer den Fall, dass wir der Klient sind, dann zerstoeren wir das ganze Objekt, das wir keine
                // Befehle mehr vom Server erhalten ( da wir uns trennen )
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




    #region Methodenaufrufe fuer externe Skripte

    public static void PlayerTakeDamage(NetworkPlayer networkPlayer, double damage)
    {
        PlayerInfo pInfo;
        if (SP.playerList.TryGetValue(networkPlayer, out pInfo))
        {
            pInfo.health -= damage;
        }
        else
        {
            Debug.Log("PlayerTakeDamage could not find networkPlayer " + networkPlayer.ToString());
        }
    }
    
    #endregion

    #region Shutdown des Servers

    public void ShutDownServer()
    {
        if (Network.isServer)
        {
            Debug.Log("ShutDownServer");

            // Eine Liste der Spielobjekte erstellen um sie einzeln zu loeschen.
            // Zum schluss alles Verweise entfernen
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

    #endregion

    #region Shutdown des Klienten

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

    #region UnityNetworkEvents fuer den Server

    //When der Server lokal geladen wird
    void OnServerInitialized()
    {
        // Jeder hat den Server in seiner PlayerList und damit auch sich neue Spieler diesen haben, wird es in den Buffer geladen
        networkView.RPC(
            "AddPlayer",
            RPCMode.AllBuffered,
            Network.player,
            PlayerPrefs.GetString("playerName"),
            new Vector3(Random.Range(0.1f, 0.4f), Random.Range(0.5f, 0.75f), Random.Range(0.25f, 1f))
            );

        // create some server content, with two different behaviors, once the server is up and running
        SpawnGameContent(1, Network.player);
        //SpawnGameContent(1, Network.player);
        //SpawnGameContent(1, Network.player);
    }

    // Diese Funktion wird im Skript nicht verwendet, wir warten auf die Ausfuehrung des "AddPlayer"-RPCs
    void OnPlayerConnected(NetworkPlayer player){ }

    // Hinter dem Spieler aufrauemen
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        networkView.RPC("RemovePlayer", RPCMode.All, player);
    }

    #endregion

    // Ein SpielerSpezifisches Objekt erstellen
    // Der RPC wird vom Server ausgefuehrt, damit das Objekt hier liegt und vom Server gemanaged werden kann
    // und damit alle Spieler das Objekt erhalten

    [RPC]
    void CreatePlayerSpecificContent(NetworkPlayer networkPlayer)
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
        SpawnGameContent(2, networkPlayer);
    }

    #region UnityNetworkingEvents fuer den Klienten

    // Nach erfolgreichem Verbinden mit dem Server
    void OnConnectedToServer()
    {
        // Zur Liste der Spieler hinzufuegen. Gebuffert sodass auch neue Spieler diesen erhalten
        //Der Server wird diesen aus der Liste entfernen, sobald der Spieler sich trennt.
        networkView.RPC("AddPlayer", RPCMode.AllBuffered, Network.player,
            PlayerPrefs.GetString("playerName"),
            new Vector3(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.1f, 0.5f)));

        // Dem Server sagen, dass er eine neue Instanz erstellen soll
        // Da der AddPlayer RPC jedoch gebuffert ist, wuerde er verschwinden wenn wir uns trennen
        networkView.RPC("CreatePlayerSpecificContent", RPCMode.Server, Network.player);

        // Klientensachen... TO DO 
    }

    #endregion

    #region Erstellen und Loeschen von Spielinhalt
 
    // Zwei Wege diese Aufgabe zu erledigen:
    // 1) SpawnGameContent und DestroyGameContent werden lokal aufgerufen. 
    //    Diese werden dann die Generierung des Objektes uebernehmen. Danach leitet es das Ojbjekt an den RPC weiter.
    // 2) SpawnOnNetwork und DestroyOnNetwork werden lokal und Serverseitig aufgerufen, sind gebuffert, und mit einem Flag zur Identifikation ausgestattet.
    //    So werden sie auf allen, auch den neuen Klienten ausgefuehrt. 
    void SpawnGameContent(int prefabID, NetworkPlayer player)
    {
        //Spieler spawnen
        Debug.Log("SpawnGameContent ");

        //Zufaelligen SpawnPoint auswaehlen
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
        GameObject theGO = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = theGO.transform.position;
        Quaternion rot = theGO.transform.rotation;

        // Die viewID eindeutig bestimmen, damit sie im RPC uebergeben werden kann und dort an das generierte Objekt gebunden wird
        // damit es nur an diesen Spieler
        NetworkViewID id1 = Network.AllocateViewID();


        // Alles Lokal erstellen und sichergehen, dass die Befehle an ander Spieler gesendet werden
        // Auf amOwner achten. Lokal sind wir der Owner, muessen aber an den Server "false"
        // uebergeben, damit der Server nicht die Kontrolle erhaelt

        SpawnOnNetwork(pos, rot, id1, true, player, prefabID);
        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, pos, rot, id1, false, player, prefabID);
    }

    [RPC]
    void SpawnOnNetwork(Vector3 pos, Quaternion rot, NetworkViewID id1, bool amOwner, NetworkPlayer networkPlayer, int prefabId)
    {
        GameObject newObject;
        PlayerInfo pNode;
        if (!playerList.TryGetValue(networkPlayer, out pNode))
        {
            //Wenn du hier landest, hast du definitiv Mist gebaut
            Debug.Log("SpawnOnNetwork of object #" + prefabId + " with NetWorkID " + id1.ToString() + " for NetworkPlayer " + networkPlayer.ToString() + " failed, because network Player doesn't exist");
            return;
        }

        // Der Zaubercode, leider noch nicht sehr weit. 
        // Hier soll ziwschen den zu Spawnenden Instanzen unterschieden werden und fuer jede Entitaet koennen diverse Eigeschaften festgelegt werden.

        // Hier ein Beispiel fuer lustige kleine Wuerfel
        switch (prefabId)
        {
            case 1:
                // Serverobjekt erstellen
                newObject = Instantiate(cubePrefab, pos, rot) as GameObject;
                newObject.renderer.material.color = pNode.color;
                newObject.name = "cube" + id1.ToString();

                // Ein kleinen Offset generieren, damit die Cubes nicht alle dasselbe zur selben zeit machen
                // Nur fuer dieses Beispiel notwendig
                if (networkPlayer == Network.player)
                {
                    // Nur fuer das Serverobjekt notwendig
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
                // SpielerAvatar
                newObject = Instantiate(clientControlledPrefab, pos, rot) as GameObject;
                newObject.renderer.material.color = pNode.color;
                newObject.name = "Player" + networkPlayer.ToString();

                // SpielerAvatar in SpielerListe festhalten
                pNode.go = newObject;

                // MeinenAvatar mir Zuordenen (da auf Server erstellt)
                StreamToClients playerControlledObject = newObject.GetComponent<StreamToClients>();
                if (playerControlledObject)
                {
                    playerControlledObject.player = networkPlayer;
                    playerControlledObject.isMyPlayer = (networkPlayer == Network.player);
                }
                else
                {
                    Debug.Log("Player Object does not have PlayerControlledObjectScript attached");
                }
                break;

            default:
                Debug.Log("Invalid prefab ID = " + prefabId);
                return;
        }

        // Set networkviewID everywhere in the game object. Just set the one, but it could be the case that eventually we
        // pass more than one along, and then need to set it in here.
        SetNetworkViewIDs(newObject.gameObject, id1);

        // keep track of our network game objects in a dictionary, so we can rapidly find objects based on their network
        // view ID.
        // In the "authoritative server" setup here, most (or all) network objects (especially those that can be moved around
        // by the game) are created by the server, but you could imagine the client creating other objects
        // that only they control

        myGOList.Add(id1, newObject);
    }

    // Alles was zum Objekt gehoert wird zerstoert, auch die gebufferten RPCs
    void DestroyGameContent(GameObject go)
    {
        //Nur vom Besitzer zu zerstoeren
        if (go.networkView.isMine)
        {
            NetworkViewID viewID = go.networkView.viewID;

            Network.RemoveRPCs(viewID);   // alle RPCs zu diesem NetworkView entfernen

            DestroyOnNetwork(viewID, true);
            networkView.RPC("DestroyOnNetwork", RPCMode.OthersBuffered, viewID, false);
        }
        else
        {
            Debug.Log("DestroyGameContent: not owner of object " + go.ToString());
        }
    }

    [RPC]
    void DestroyOnNetwork(NetworkViewID id, bool amOwner)
    {
        GameObject go;
        // Das Objekt muss noch existieren, damit wir es loeschen koennen.
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

    // A Networkview ist 0 bei initialisierung, so ist es nichtz verwendbar
    // Daher wird die Id, an das Objekt gekoppelt.
    void SetNetworkViewIDs(GameObject go, NetworkViewID id1)
    {
        Component[] nViews = go.GetComponentsInChildren<NetworkView>();

        foreach (NetworkView nv in nViews)
        {
            nv.viewID = id1;
            break;  // just set the first
        }
    }

#endregion


    #region Server ODER Klient: Disconnect

    IEnumerator OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("... Disconnected From Server ...");
        yield return 0;//Wait for actual disconnect

        //Alle Spieler loeschen. Auf auf in den GBC
        playerList.Clear();

        // Jedes GO einzeln zerstoeren
        foreach (KeyValuePair<NetworkViewID, GameObject> nvGO in myGOList)
        {
            Destroy(nvGO.Value);
        }
        myGOList.Clear();

 
        if (Network.isServer)
        {
            //Hier den Masterserver stoppen    

        }
        else
        {
            if (info == NetworkDisconnection.LostConnection)
            {
                //Debug.LogWarning("Client Lost connection to the server");
            }
            else
            {
                //Debug.LogWarning("Client Successfully disconnected from the server");
            }
        }
    }

    #endregion

}
