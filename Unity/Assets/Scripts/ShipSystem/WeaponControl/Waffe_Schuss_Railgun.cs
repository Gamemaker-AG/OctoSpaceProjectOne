using UnityEngine;
using System.Collections;

public class Waffe_Schuss_Railgun : MonoBehaviour {


		
		
		public float TESTEnergieregen;
		public Rigidbody bullet; //prefab das geschosses
		public Transform weaponEnd; //spawn des geschosses
		private float cooldownTime = 1;
		public float MAXMunition; //maximale Munition
		//public float speed;
		public float munition; //geladene munition
		public double Energie;
		public float Energieverbrauch;
		public float Regenzeit;
		public float MAXEnergie;
		public Transform MGDrehung;
		public float fireRate=0.5f;
		public float nextFire=0.0f;
		
		//Vector3 bewegung;
		
		
		void Update () {
			
			Schuss ();
			reload ();
			
		}
		
		void FixedUpdate()
		{
			EnergieREGEN ();
		}
		



		
		
		
		void Schuss(){
			
		if(Input.GetButton ("Fire2") && munition > 0)      //wenn Fire2 verwendet wird ( siehe -> Edit -> Project Settings -> Input) führe folgendes aus:
			{
			schuss ();
			}


		
		}
		
		void reload(){
			
			if(Input.GetButtonDown("Reload") && Energie >= Energieverbrauch){
				
				
				
				munition = MAXMunition; //nachladen
				Energie = Energie - Energieverbrauch;
				
			}
			
			
			
		}
		void EnergieREGEN(){
			
			
			if (cooldownTime <= Time.time && Energie > 0 && Energie < MAXEnergie) {
				
				cooldownTime = Time.time + Regenzeit;
				
				
				Energie = TESTEnergieregen + Energie; //Regeneriert alle 10 Sekunden TESTEnergieregen
				
			} 
			
			if (Energie < 1) {
				
				Energie = 1;
				
			}
		}

			void schuss(){

		if (Time.time > fireRate) 
			{
						Rigidbody bulletInstanz;      //definiert bulletInstanz als einen Rigidbody
						bulletInstanz = Instantiate (bullet, weaponEnd.position, weaponEnd.rotation) as Rigidbody; //definiert bulletInstanz und verwendet "Instantiate" 
						//um gewaehlte Rigidbodys zu generieren 
						//(Rigidbody, Position des Spawns, Rotation des Spawns)
						munition = munition - 1; //munitionsverbrauch einer Waffe
						//cooldownTime = Time.time + 1.0f; //cooldown nach einem schuss
						//bulletInstanz.AddForce(weaponEnd.forward * speed);
						// wendet "AddForce" an um auf das generierte Objekt um es forwaerts zu bewegen mit "speed" als geschwindigkeitsangabe.
				
						//MGDrehung.Rotate (0f, 20f, 0f * Time.deltaTime); 
						fireRate = Time.time + 0.1f;
				}
		}
		
	}
