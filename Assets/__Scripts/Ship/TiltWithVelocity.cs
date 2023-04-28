using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltWithVelocity : MonoBehaviour
{
    [Tooltip("The number of degrees that the ship will tilt at its maximum speed.")]
    public int      degrees = 30;
    public bool     tiltTowards = true;

    private int     prevDegrees = int.MaxValue;
    private float   tan;

    Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        
        if (degrees != prevDegrees)
        {
            prevDegrees = degrees;
            tan = Mathf.Tan(Mathf.Deg2Rad * degrees);
        }
        Vector3 pitchDir = (tiltTowards) ? -rigid.velocity : rigid.velocity;
        pitchDir += Vector3.forward / tan * PlayerShip.MAX_SPEED;
        transform.LookAt(transform.position + pitchDir);
    }
}
