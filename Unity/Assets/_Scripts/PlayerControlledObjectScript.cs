using UnityEngine;
using System.Collections;

public class PlayerControlledObjectScript : MonoBehaviour
{
    // Der Bewegungsvektor
    private Vector3 moveDirection = Vector3.zero;

    public float speed = 10f;
    private bool lastNonZero = false;

    // Eigentumsverhaeltnisse
    public bool isMyPlayer = false;
    public NetworkPlayer player;

        private Vector3 latestCorrectPos;
    private Vector3 onUpdatePos;
    private float fraction;

    void Awake()
    {
        latestCorrectPos = transform.position;
        onUpdatePos = transform.position;
    }
    void Start()
    {
    }

    // Bewegungsbefehle an den Server senden
    void Update()
    {
        if (isMyPlayer)
        {
            float xm = Input.GetAxis("Horizontal");
            float ym = Input.GetAxis("Vertical");

            // Bewegungsvektor sofort an den Server senden!
            moveDirection = new Vector3(xm, 0, ym);

            // Wenn wir uns aufhoeren zu bewegen, dann senden wir einen (0|0|0) Vektor
            if (xm != 0f || ym != 0f)
            {
                // Die Bewegung wird auf dem Server ausgefuehrt, nicht dem Klienten
                networkView.RPC("movePlayer", RPCMode.Server, moveDirection);
                lastNonZero = true;
            }
            else if (lastNonZero)
            {
                lastNonZero = false;
                networkView.RPC("movePlayer", RPCMode.Server, moveDirection);
            }




        }




    }

    // Klient und Server fuehren die Bewegung durch. Wird vom Server ein Positionsupdate an den Klienten gesendet
    // wird die Position des Klienten mit der des Servers ueberschrieben
    void FixedUpdate()
    {
        if (Network.isServer)
        {
            if (moveDirection.magnitude > 0.001)
            {
                rigidbody.AddForce(100f * moveDirection * speed * Time.deltaTime);
            }
        }
        else
        {

            fraction = fraction + Time.deltaTime * 9;
            Debug.Log("onupdate " + onUpdatePos.ToString());
            Debug.Log("latestcorrect " + latestCorrectPos.ToString());
            transform.localPosition = Vector3.Lerp(onUpdatePos,latestCorrectPos, fraction);   
        }
    }

    // Daten an den Klienten senden
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            Debug.Log("StreamWriting");
        }
        else
        {
            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            latestCorrectPos = pos;                 // save this to move towards it in FixedUpdate()
            onUpdatePos = transform.localPosition;  // we interpolate from here to latestCorrectPos
            fraction = 0;                           // reset the fraction we alreay moved. see Update()

            transform.localRotation = rot;          // this sample doesn't smooth rotation

            Debug.Log("StreamReading");
        }
    }
 

    // KeyboardInput vom Klienten verwenden
    [RPC]
    void movePlayer(Vector3 dir, NetworkMessageInfo info)
    {
        moveDirection = dir;
    }

    // Sichergehen, dass auch alle Objekte beim trennen des Spielers zerstoert werden
    void OnDestroy()
    {
        //if (Unterobjekt)
        //Destroy(Unterobjekt);
    }
}