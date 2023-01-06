using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public WheelCollider[] WC;
    public GameObject[] Wheels;
    public float torque = 200;
    public float maxSteerAngle = 30;
    public float maxBrakeTorque = 500;
    
    void Start()
    {
    }
    
    void Update()
    {
        float a = Input.GetAxis("Vertical");
        float s = Input.GetAxis("Horizontal");
        float b = Input.GetAxis("Jump");

        Go(a,s,b);
    }

    void Go(float accel, float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;

        float thrustTorque = accel * torque;
        
        for (int i = 0; i < 4; i++)
        {
            WC[i].motorTorque = thrustTorque;

            if (i < 2)
                WC[i].steerAngle = steer;
            else
                WC[i].brakeTorque = brake;
            Quaternion quat;
            Vector3 position;

            WC[i].GetWorldPose(out position, out quat);

            Wheels[i].transform.position = position;
            Wheels[i].transform.rotation = quat;
        }
    }
}
