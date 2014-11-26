using UnityEngine;
using System.Collections;

public class CamControl : MonoBehaviour {

	public Transform target;
	public float smoothTime = 0.3F;
	private Vector3 velocity = Vector3.zero;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector3 targetPosition = target.TransformPoint(new Vector3(0, 250, 150));
		transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
	}
}
