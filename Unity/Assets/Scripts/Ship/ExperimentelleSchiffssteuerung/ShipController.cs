using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour
{

    public float speed;
    public Vector3 thrust;									//Total thrust per axis
    public float snapThreshold;								//used to tweak how smoothly it will stabilize; .01 is probably as low as you want to go; lower is smoother, but more drift/jittering may occur.  I use 1.
	public float maxAngVel;

	private Vector3 targetVelocity;							//user input determines how fast user wants ship to rotate
    private Vector3 tActivation = Vector3.zero;			    //switch vector to indicate which axes need thrust and in which direction (values are: -1, 0, or 1)
    private Vector3 inputs;									//just holds input axis values
    private Vector3 torques;								//the amount of torque available for each axis, based on thrust
    private Vector3 rates;									//the rates of angular acceleration for each axis, based on the torque available and ship mass
    private float maxRate;									//just holds the Physics.angularVelocity value; you can use other values here (for example: different ships may have different max rotation rates based on integrity of it's hull)
    private Vector3 curVelocity;							//holds the rigidbody.angularVelocity converted from world space to local

    // Use this for initialization
    void Start()
    {
		rigidbody.maxAngularVelocity = maxAngVel;
    }

    void Awake()
    {
        //this is where the bounding box is used to create pseudo-realistic torque;  If you want more detail, just ask.
		Vector3 shipExtents = GetComponent<MeshFilter> ().mesh.bounds.extents;
        //var shipExtents = transform.Find("Player").GetComponent(MeshFilter).mesh.bounds.extents; 
		Vector2 shipExtx= new Vector2 (shipExtents.y, shipExtents.z);
		Vector2 shipExty= new Vector2 (shipExtents.x, shipExtents.y);
		Vector2 shipExtz= new Vector2 (shipExtents.x, shipExtents.z);

		//torques.x = Vector2(shipExtents.y, shipExtents.z).magnitude * thrust.x;
		//torques.y = Vector2(shipExtents.x, shipExtents.y).magnitude * thrust.y;	//normally would be x and z, but mesh is rotated 90 degrees in mine.  
        //torques.z = Vector2(shipExtents.x, shipExtents.z).magnitude * thrust.z;	//normally would be x and y, but mesh is rotated 90 degrees in mine.

		torques.x = shipExtx.magnitude * thrust.x;
		torques.y = shipExty.magnitude * thrust.y;
		torques.z = shipExtz.magnitude * thrust.z;


        maxRate = Physics.maxAngularVelocity;

    }

    void FixedUpdate()
    {
        RCS();

        float moveY = Input.GetAxis ("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

		if (Input.GetKey ("space")) {
			StopMachine();
				}
        Vector3 movement = new Vector3(0.0f, moveY, moveZ);
        //rigidbody.AddForce(movement * speed * Time.deltaTime);
		//rigidbody.AddRelativeForce (movement.z * speed * Time.deltaTime);
		rigidbody.AddRelativeForce (movement * (speed * Time.deltaTime));


		Debug.Log (rigidbody.velocity.ToString(), gameObject);

    }

    // Update is called once per frame
    void Update()
    {


    }

	public void StopMachine()
	{
		rigidbody.velocity = rigidbody.velocity * 0.9f;

	}

    public void RCS()
    {
        //angular acceleration = torque/mass
        rates = torques / rigidbody.mass;

        //determine targer rates of rotation based on user input as a percentage of the maximum angular velocity
        //targetVelocity = Vector3(Input.GetAxis("Pitch")*maxRate,Input.GetAxis("Yaw")*maxRate,Input.GetAxis("Roll")*maxRate);
		float axisX = Input.GetAxis ("VerticalWS") * maxRate;
		float axisY = 0;//Input.GetAxis ("Mouse X") * maxRate;
		float axisZ = Input.GetAxis("HorizontalAD") * -1 * maxRate;
        targetVelocity = new Vector3(axisX, axisY, axisZ);

        //take the rigidbody.angularVelocity and convert it to local space; we need this for comparison to target rotation velocities
        curVelocity = transform.InverseTransformDirection(rigidbody.angularVelocity);

        //****************************************************************************************************************
        //For each axis:  If the ship's current rate of rotation does not equal the desired rate of rotation, first check to see
        //if it is a matter of drift or "jittering", which is when it keeps jumping from positive to negative to positive thrust because the
        //values are so close to zero (to see what I mean, set  snapThreshold = 0, rotate the ship on multiple axes, then let it try
        //to come to a complete stop.  It won't.)  If it is just drift/jittering, turn off the thrust for the axis, and just set the current 
        //angular velocity to the target angular velocity.  Otherwise, the user is still giving input, and we haven't reached the 
        //desired rate of rotation.  In that case, we set the axis activation value = to the direction in which we need thrust.
        //****************************************************************************************************************

        if (curVelocity.x != targetVelocity.x)
            if (Mathf.Abs(targetVelocity.x - curVelocity.x) < rates.x * Time.deltaTime * snapThreshold)
            {
                tActivation.x = 0;
                curVelocity.x = targetVelocity.x;
            }
            else
                tActivation.x = Mathf.Sign(targetVelocity.x - curVelocity.x);

        if (curVelocity.y != targetVelocity.y)
            if (Mathf.Abs(targetVelocity.y - curVelocity.y) < rates.y * Time.deltaTime * snapThreshold)
            {
                tActivation.y = 0;
                curVelocity.y = targetVelocity.y;
            }
            else
                tActivation.y = Mathf.Sign(targetVelocity.y - curVelocity.y);

        if (curVelocity.z != targetVelocity.z)
            if (Mathf.Abs(targetVelocity.z - curVelocity.z) < rates.z * Time.deltaTime * snapThreshold)
            {
                tActivation.z = 0;
                curVelocity.z = targetVelocity.z;
            }
            else
                tActivation.z = Mathf.Sign(targetVelocity.z - curVelocity.z);


        //here, we manually set the rigidbody.angular velocity to the value of our current velocity.
        //this is done to effect the manual changes we may have made on any number of axes.
        //if we didn't do this, the jittering would continue to occur.
        rigidbody.angularVelocity = transform.TransformDirection(curVelocity);

        //call the function that actually handles applying the torque
        FireThrusters();

    }

    public void FireThrusters()
    {
        //for each axis, applies torque based on the torque available to the axis in the direction indicated by the activation value.
        //-1 means we are applying torque to effect a negative rotation.  +1 does just the opposite.  0 means no torque is needed.
        if (tActivation.x != 0)
            rigidbody.AddTorque(tActivation.x * transform.TransformDirection(Vector3.right) * torques.x * Time.deltaTime, ForceMode.Impulse);
        if (tActivation.y != 0)
            rigidbody.AddTorque(tActivation.y * transform.TransformDirection(Vector3.up) * torques.y * Time.deltaTime, ForceMode.Impulse);
        if (tActivation.z != 0)
            rigidbody.AddTorque(tActivation.z * transform.TransformDirection(Vector3.forward) * torques.z * Time.deltaTime, ForceMode.Impulse);
    }


}
