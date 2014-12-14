using UnityEngine;
using System.Collections;

public class EnableAndDisableShild : MonoBehaviour
{


		public GameObject ship;
		private EnemyTakeDMG enemytakedmg;
		public float _SHD;

		// Use this for initialization

		void Awake ()
		{

				enemytakedmg = ship.GetComponent<EnemyTakeDMG> ();

		}

		void Start ()
		{



		}

	
		// Update is called once per frame
		void Update ()
		{

				_SHD = enemytakedmg.shield;
				shildOnlineAndOffline ();

		
	
		}

		void shildOnlineAndOffline ()
		{
				if (_SHD == 0) {
						gameObject.renderer.enabled = false;	
				} else {
						gameObject.renderer.enabled = true;
				}
		}
}
