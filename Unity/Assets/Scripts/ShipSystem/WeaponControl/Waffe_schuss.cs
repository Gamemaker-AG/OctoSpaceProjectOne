using UnityEngine;
using System.Collections;

public class Waffe_schuss : MonoBehaviour 

	{


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

		if(Input.GetButtonDown ("Fire2") && munition > 0) //&& cooldownTime <= Time.time && Energie > 0)      //wenn Fire2 verwendet wird ( siehe -> Edit -> Project Settings -> Input) führe folgendes aus:
		{
			
			Rigidbody bulletInstanz;      //definiert bulletInstanz als einen Rigidbody
			bulletInstanz = Instantiate(bullet, weaponEnd.position, weaponEnd.rotation) as Rigidbody; //definiert bulletInstanz und verwendet "Instantiate" 
			//um gewaehlte Rigidbodys zu generieren 
			//(Rigidbody, Position des Spawns, Rotation des Spawns)
			munition = munition - 1; //munitionsverbrauch einer Waffe
			//cooldownTime = Time.time + 1.0f; //cooldown nach einem schuss
			//bulletInstanz.AddForce(weaponEnd.forward * speed);				 // wendet "AddForce" an um auf das generierte Objekt um es forwaerts zu bewegen mit "speed" als geschwindigkeitsangabe.
		}


	}

	void reload(){

		if(Input.GetButtonDown("Reload") && Energie > Energieverbrauch){



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

}
	
	
	
	