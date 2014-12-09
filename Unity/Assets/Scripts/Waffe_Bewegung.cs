using UnityEngine;
using System.Collections;

public class Waffe_Bewegung : MonoBehaviour {

	public float speed;
	public Transform waffe1;
	public Transform waffe2;
		

		

			
	public Texture2D marker;
	
	void onGUI()
	{
				GUI.DrawTexture (new Rect (Screen.width / 2 - 8, Screen.height / 2 - 8, 16, 16), marker);
		}



	void Update () {

				if (Input.GetKey ("d")) {      // Verwendet die Taste "d" für eine Drehung nach Rechts

						transform.Rotate (Vector3.up * speed * Time.deltaTime);
				}
				if (Input.GetKey ("a")) {
						
						transform.Rotate (Vector3.down * speed * Time.deltaTime);  //verwendet die Taste "a" für eie Drehung nach links
				}

				if (Input.GetKey ("w")) {
				
						waffe1.Rotate (Vector3.left * speed * Time.deltaTime);       //richtet Waffe 1 und Waffe 2 nach oben aus
						waffe2.Rotate (Vector3.left * speed * Time.deltaTime);       //wird verwendet wenn mehr als 2 Waffenlaeufe an einer Waffe verwendet werden.
				}

				if (Input.GetKey ("s")) {
				
						waffe1.Rotate (Vector3.right * speed * Time.deltaTime);       //richtet Waffe 1 und Waffe 2 nach unten aus
						waffe2.Rotate (Vector3.right * speed * Time.deltaTime);
				}



		}

}
