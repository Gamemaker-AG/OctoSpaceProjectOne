// from 
// http://forum.unity3d.com/threads/55149-Synchronising-Network-time
//
// NetworkStartTime.instance.time  can be used anywhere to get the offset

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class NetworkStartTime : MonoBehaviour
{
    public static NetworkStartTime instance;
    private float deltaTime;
    public float time;

	void Awake()
	{
    	instance = this;
		deltaTime = 0;
	}

	// when we start the server
    void OnServerInitialized()
	{
		deltaTime = -(float)Network.time;
	}
	
	// when we connect to the server, 
    void OnConnectedToServer()
    {
		networkView.RPC("GetServerTime",RPCMode.Server);    
	}
	
	// keep a network version of the time set
    void Update()
    {
        time = (float)Network.time + deltaTime;     
    }

    //server function 
    [RPC]
    void GetServerTime(NetworkMessageInfo info){
        networkView.RPC("SetDeltaTime", info.sender, time); 
    } 
	
    [RPC]
    void SetDeltaTime (float serverTime, NetworkMessageInfo info)
    {
       deltaTime = serverTime - (float)info.timestamp; 
       Debug.Log("Network.time " + Network.time + " Time " + Time.time + " Delta " + deltaTime + "  serverTime =  " + serverTime.ToString());
    }
}