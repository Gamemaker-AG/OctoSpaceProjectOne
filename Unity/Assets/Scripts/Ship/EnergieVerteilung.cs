using UnityEngine;
using System.Collections;

public class EnergieVerteilung : MonoBehaviour
{


		public float gesammtEnergie = 200000f;
		public float energieRegeneration;
		private float cooldownTime = 1f;
		public float Regenzeit;
		public float maximaleEnergie;
		public float gegebeneEnergie;
		public float energieDerSchilde = 5000f;
		private EnemyTakeDMG enemytakedmg;
		private float energiederschilde;

		void Awake ()
		{
				enemytakedmg = GetComponent<EnemyTakeDMG> ();
		}

		void Start ()
		{

	
		}
	
		// Update is called once per frame
		void Update ()
		{
				EnergieREGEN ();
				energiederschilde = enemytakedmg.energiederschilde;
				energieDerSchilde = energiederschilde;
				gebeEnergieAnShild ();
	
		}



		void EnergieREGEN ()
		{
		
		
				if (cooldownTime <= Time.time && gesammtEnergie > 0 && gesammtEnergie < maximaleEnergie) {
			
						cooldownTime = Time.time + Regenzeit;
						gesammtEnergie = energieRegeneration + gesammtEnergie; //Regeneriert alle 10 Sekunden TESTEnergieregen
			
				} 
		

		}

		void gebeEnergieAnShild ()
		{

				if (Input.GetKeyDown (KeyCode.Z) && gegebeneEnergie < gesammtEnergie) {

						gesammtEnergie -= gegebeneEnergie;
						energieDerSchilde += gegebeneEnergie;


				}

		}



}
