// script for a player-controlled object (their avatar, usually).
// Here we send the player movement commands to the server.  The server will then send back the proper
// position, based on the centralized game simulation.  If the network update isn't fast enough, this would result in
// sluggish movement.  Ideally, you'd want to keep some history, run locally and then jump if there are conflicts 
// (i.e., think how SecondLife does it)

using UnityEngine;
using System.Collections;

public class PlayerControlledObjectScript : MonoBehaviour {
	// our movement vector.



	// who this represents, and is it me? 
	public bool isMyPlayer = false;
	public NetworkPlayer player; 
	

	void OnSerializeNetworkView ( BitStream stream ,   NetworkMessageInfo info  ){
		if (stream.isWriting){
			//Executed on the owner of this networkview; 
			//The server sends it's position over the network
			Vector3 pos = transform.position;
		    Quaternion rot = transform.rotation;
			stream.Serialize(ref pos);//"Encode" it, and send it
            stream.Serialize(ref rot);
					
		} else {
			//Executed on the others; 
			//receive a position and set the object to it
		    Quaternion rotReceive = Quaternion.identity;
			Vector3 posReceive = Vector3.zero;
			stream.Serialize(ref posReceive); //"Decode" it and receive it
		    stream.Serialize(ref rotReceive);
			transform.position = posReceive;
		    transform.rotation = rotReceive;
		}
	}
}