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

    public Transform skidTrailPrefab;
    Transform[] skidTrails = new Transform[4];

    public AudioSource skidSound;
    
    public ParticleSystem smokePrefab;
    ParticleSystem[] skidSmoke = new ParticleSystem[4];

    public GameObject brakeLight;

    public void StartSkidTrail(int i) 
    {
        if (skidTrails[i] == null)
        {
            skidTrails[i] = Instantiate(skidTrailPrefab);
        }
        skidTrails[i].parent = WC[i].transform;
        skidTrails[i].localRotation = Quaternion.Euler(90, 0, 0);
        skidTrails[i].localPosition = -Vector3.up * WC[i].radius;
    }

    public void EndSkidTrail(int i)
    {
        if (skidTrails[i] == null)
        {
            return;
        }
        Transform holder = skidTrails[i];
        skidTrails[i] = null;
        holder.parent = null;
        holder.localRotation = Quaternion.Euler(90, 0, 0);
        Destroy(holder.gameObject, 30);
    }

    void Go(float accel, float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;

        if (brake != 0)
        {
            brakeLight.SetActive(true);
        }
        else
        {
            brakeLight.SetActive(false);
        }

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

    void CheckForSkid()
    {
        int numSkidding = 0;
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelHit;
            WC[i].GetGroundHit(out wheelHit);

            if (Mathf.Abs(wheelHit.forwardSlip) >= 0.5f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.5f)
            {
                numSkidding++;
                if (!skidSound.isPlaying)
                {
                    skidSound.Play();
                }
                //StartSkidTrail(i);
                skidSmoke[i].transform.position = WC[i].transform.position - WC[i].transform.up * WC[i].radius;
                skidSmoke[i].Emit(1);
            }
            else
            {
                //EndSkidTrail(i);
            }
        }

        if (numSkidding == 0 && skidSound.isPlaying)
            skidSound.Stop();
    }

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            skidSmoke[i] = Instantiate(smokePrefab);
            skidSmoke[i].Stop();
        }

        brakeLight.SetActive(false);
    }

    void Update()
    {
        float a = Input.GetAxis("Vertical");
        float s = Input.GetAxis("Horizontal");
        float b = Input.GetAxis("Jump");

        Go(a, s, b);

        CheckForSkid();
    }
}
