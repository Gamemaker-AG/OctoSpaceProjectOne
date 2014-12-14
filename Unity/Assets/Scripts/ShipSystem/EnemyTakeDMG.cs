
using UnityEngine;
using System.Collections;

public class EnemyTakeDMG : MonoBehaviour
{
	

		public GameObject schild;
		public float health;
		public float shield;
		public float MAXshield;
		public float shieldregen;
		public float SHDRegenzeit;
		private float cooldownTime = 1;
		public bool shieldDEAD = false;
		public float bulletDMG;
		public float laserDMG;
		public float shildabsorb;
		private EnergieVerteilung energieverteilung;
		public float energiederschilde;
		public float energieverbrauch;
		public float Verbrauchszeit;
		public float energieverbrauchprozeit;
		private float pause = 1;
		public float regtimeafter0 = 5;

		void Awake ()
		{

				energieverteilung = GetComponent<EnergieVerteilung> ();

		}

		void Start ()
		{


		}

		void OnTriggerEnter (Collider other)
		{
				if (other.gameObject.tag == "Bullet") {
						health -= bulletDMG;
				}
		
				if (other.gameObject.tag == "Laser" && shield > 0) {
			
						float dmgOnHP;
						float dmgOnShild;
			
			
						dmgOnHP = laserDMG * shildabsorb;
						dmgOnShild = laserDMG;
						shield -= dmgOnShild;
						health -= dmgOnHP;
			
				}
		
				if (other.gameObject.tag == "Laser" && shield == 0) {

						health -= laserDMG;
				}
		
		
		
				if (health <= 0) {
			
						DestroyObject (other.gameObject);
						Destroy (gameObject);
			
				}
		



		
		}

		void Update ()
		{

				energiederschilde = energieverteilung.energieDerSchilde;

				SHDREGEN ();
				Energieverbrauch ();
				entferneSchild ();

		}

		void SHDREGEN ()
		{
		
		
				if (cooldownTime <= Time.time && shield < MAXshield && energiederschilde > 0) {
			
						cooldownTime = Time.time + SHDRegenzeit;
						
						if (shield > 0) {
								shield += shieldregen; //Regeneriert schild
								energiederschilde -= energieverbrauch;

				
						} else {

								if (pause <= Time.time && shield == 0) {
			
								


										pause = Time.time + regtimeafter0;
										shield += shieldregen;
								}
					

						}


						//verbraucht bei regenration energie
			

		
						if (shield < 0) {
			
								shield = 0;
			
						}

						if (shield > MAXshield) {
				
								shield = MAXshield;
		
						}

						if (energiederschilde < 0) {
								energiederschilde = 0;
						}
				}
		}

		void entferneSchild ()
		{
				if (shield == 0 || energiederschilde == 0) {
						shieldDEAD = true;
						shield = 0;
				}

		}

		void Energieverbrauch ()
		{
				if (cooldownTime <= Time.time && energiederschilde > 0) {
			
						cooldownTime = Time.time + Verbrauchszeit;
			
			
						energiederschilde -= energieverbrauchprozeit; //Regeneriert schild

				}
		}
}
	