using UnityEngine;
using System.Collections;
 

public class Waffe_Projektil : MonoBehaviour {

	public float speed = 1f;
	public float DetoTime = 2f;

	// Use this for initialization
	void Start () {
		Destroy (gameObject, DetoTime);
	}

	void FixedUpdate()

	{
		bewegeProjektil ();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Enemy")
		{
			Destroy (gameObject);
		}


	}

	void bewegeProjektil(){
		transform.Translate (0f, 0f, speed * Time.deltaTime);

		}

}
