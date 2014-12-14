using System;
using UnityEngine;
using System.Collections;

public class Engineer : MonoBehaviour
{

    public GameObject MyShip { get; set; }


    // Use this for initialization
    void Start()
    {

    }

    void OnGUI()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Ship");

        GUILayout.Space(200);
        GUILayout.BeginVertical();

        for (int i = 0; i < gameObjects.Length; i++)
        {

            string shipGuid = gameObjects[i].GetComponent<Ship>().ShipID;

            if (GUILayout.Button("Select Ship with " + shipGuid))
            {
                networkView.RPC("RegisterOnShip", RPCMode.Server, shipGuid, Network.player);
                Debug.Log("Tryin Register on SHip RPC " + shipGuid);
            }
        }

        GUILayout.EndVertical();

    }
    // Update is called once per frame
    void Update()
    {

    }

    [RPC]
    void RegisterOnShip(string shipId, NetworkPlayer sender)
    {
        Debug.Log("RPC Called");
        if (Network.isServer)
        {
            Debug.Log("IsServer");
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Ship");

            foreach (GameObject shipGameObject in gameObjects)
            {
                Debug.Log("InForeachSchleife with " + shipGameObject.GetComponent<Ship>().ShipID);
                if (shipId == shipGameObject.GetComponent<Ship>().ShipID)
                {
                    
                    shipGameObject.GetComponent<Ship>().MyEngineer =
                        GameObject.Find("Player" + sender.ToString()).GetComponent<Engineer>();
                    Debug.Log("Assigned to Ship");
                    networkView.RPC("RegisterShip", RPCMode.Others);
                    return;
                }
            }
        }
    }

    [RPC]
    void RegisterShip()
    {
        
    }
}
