using UnityEngine;
using System.Collections;

public class DynamicObjectScript : MonoBehaviour {
//This is mostly copied from tut 2B 

	public float scale = 0f;
	public float offset = 0f;

	// the server modifies these game objects
	void Update (){
		if(networkView.isMine){
			//Only the owner can move the cube!	
			//(Ok this check is a bit overkill since we did already disable the script in Awake)	
			float noiseX = Mathf.PerlinNoise(Time.time + offset, 0) - 0.5f;
			float noiseZ = Mathf.PerlinNoise(Time.time - offset, 0) - 0.5f;
			Vector3 moveDirection = new Vector3(noiseX * scale, 0, noiseZ * scale);
			transform.Translate(moveDirection * Time.deltaTime);
		}
	}
	
	// changes to these server-controlled objects are streamed to the clients
	void OnSerializeNetworkView ( BitStream stream ,   NetworkMessageInfo info  ){
		if (stream.isWriting){
			//Executed on the owner of this networkview; 
			
			// send the offset generated locally:  ideally we could use this to run the sim independently, but the one
			// we are running is far to local-simulation-dependent
			stream.Serialize(ref offset);
			//The server sends it's position over the network
			Vector3 pos = transform.position;		
			stream.Serialize(ref pos);//"Encode" it, and send it
					
		}else{
			//Executed on the others; 

			//receive the offset and a position and set the object to it
			stream.Serialize(ref offset);			
			Vector3 posReceive = Vector3.zero;
			stream.Serialize(ref posReceive); //"Decode" it and receive it
			transform.position = posReceive;
			
		}
	}
}
