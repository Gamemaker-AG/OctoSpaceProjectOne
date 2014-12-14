using UnityEngine;
using System.Collections;

public class StreamInput : MonoBehaviour
{
    private Vector3 _moveDir;
    private Vector3 _turnDirection;
    public float TurnSpeed = 100f;

    private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            if (gameObject.GetComponent<PlayerControlledObjectScript>().isMyPlayer)
            {
                Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                Vector3 turnDirection = new Vector3(0, Input.GetAxis("HorizontalTurn"), Input.GetAxis("VerticalTurn"));

                stream.Serialize(ref moveDirection);
                stream.Serialize(ref turnDirection);

            }
        }
        else
        {
            stream.Serialize(ref _moveDir);
            stream.Serialize(ref _turnDirection);

        }
    }

    void FixedUpdate()
    {
        if (Network.isServer)
        {
            if (_moveDir.magnitude > 0.001)
            {
                rigidbody.AddRelativeForce(100f * _moveDir * 10 * Time.deltaTime);
            }
            if (_turnDirection.magnitude > 0.001)
            {
                rigidbody.AddRelativeTorque(_turnDirection * Time.deltaTime * TurnSpeed);
            }
        }
    }

    void Update()
    {
        if (Input.GetKey("space"))
        {
            networkView.RPC("StopPlayer", RPCMode.Server);
        }
    }

    [RPC]
    void StopPlayer()
    {
        rigidbody.velocity = Vector3.zero;
    }
}
