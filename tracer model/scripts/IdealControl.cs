using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWA;

public class IdealControl : MonoBehaviour
{
    // Force moving -> wheel visual independent script is attached
    // pleas unattach physics based visual script for wheel
    // CW : w(+), CCW : w(-)

    private GameObject vehicle;
    private GameObject WheelLeftVisual, WheelRightVisual;  // visual wheel

    [Header("Global Vehicle Velocity Control")]
    public float g_v = 0.0f; // [m/s]
    public float g_w = 0.0f; //[rad/s]

    // static constants of vehicle
    private const float WHEELRADIUS = 0.0418f;  // wheel's radius [m]
    private const float TRACK = 0.36f;   // vehicle track's value [m]
    private const float VEHICLE_LENGTH = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        vehicle = GameObject.Find("Tracer");
        WheelLeftVisual = GameObject.Find("leftWheel");
        WheelRightVisual = GameObject.Find("rightWheel");
        
        DynamicWindowApproach.cur_vehicleCommand = new float[2] { 0.0f, 0.0f };
        DynamicWindowApproach.cur_vehiclePos = new float[3] { vehicle.transform.position.x, vehicle.transform.position.z, rotationGet(vehicle.transform.rotation.eulerAngles.y * Mathf.Deg2Rad) };

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(DynamicWindowApproach.client_check);
        if (DynamicWindowApproach.client_check)
        {
            if (DynamicWindowApproach.check_reset)
            {
                VehicelInitialize();
                DynamicWindowApproach.check_reset = false;
            }
            else
            {
                g_v = DynamicWindowApproach.g_result_dwa_ctr[0];
                g_w = DynamicWindowApproach.g_result_dwa_ctr[1];
                //Debug.Log(vehicle.transform.rotation.eulerAngles.y);
                VehicleForceMove();

                float[] wheelvel = refvel2refwheel(g_v, g_w);   // convert vehicle's linear & angular velocity to wheel angular input
                WheelAnimation(wheelvel[0], wheelvel[1]);   // animation update

                // local planner
                /*DynamicWindowApproach.cur_vehiclePos = new float[3] { vehicle.transform.position.x, vehicle.transform.position.z, Mathf.PI * 2.0f - vehicle.transform.rotation.eulerAngles.y * Mathf.Deg2Rad };*/
                DynamicWindowApproach.cur_vehiclePos = new float[3] { vehicle.transform.position.x, vehicle.transform.position.z, rotationGet(vehicle.transform.rotation.eulerAngles.y * Mathf.Deg2Rad) };
                DynamicWindowApproach.cur_vehicleCommand = new float[2] { g_v, g_w };
            }
        }

    }

    void VehicelInitialize()
    {
        // initializing vehicle to new position
        vehicle.transform.position = new Vector3(0.0f, 0.02f, 1.7f);
        vehicle.transform.rotation = Quaternion.identity;
        DynamicWindowApproach.check_collision = false;
        DynamicWindowApproach.check_done = false;
    }

    void VehicleForceMove()
    {
        /*float x = vehicle.transform.position.x + g_v * Mathf.Cos(Mathf.PI * 2.0f - vehicle.transform.rotation.eulerAngles.y * Mathf.Deg2Rad) * Time.deltaTime;
        float z = vehicle.transform.position.z + g_v * Mathf.Sin(Mathf.PI * 2.0f - vehicle.transform.rotation.eulerAngles.y * Mathf.Deg2Rad) * Time.deltaTime;*/
        float x = vehicle.transform.position.x + g_v * Mathf.Cos(rotationGet(vehicle.transform.rotation.eulerAngles.y * Mathf.Deg2Rad)) * Time.deltaTime;
        float z = vehicle.transform.position.z + g_v * Mathf.Sin(rotationGet(vehicle.transform.rotation.eulerAngles.y * Mathf.Deg2Rad)) * Time.deltaTime;
        //float heading = -(vehicle.transform.rotation.eulerAngles.y + Mathf.Rad2Deg * g_w * Time.deltaTime);

        vehicle.transform.Rotate(new Vector3(0.0f, Mathf.Rad2Deg * (-g_w) * Time.deltaTime, 0.0f));// update rotation
        vehicle.transform.position = new Vector3(x, 0.0f, z);  // update position

        //Debug.Log(vehicle.transform.rotation.eulerAngles.y.ToString("F3"));
    }

    float rotationGet(float yaw)    //rad
    {
        float result = 0.0f;   // first set the right direction

        if (yaw > Mathf.PI)
            result = Mathf.PI * 2.0f - yaw;
        else
            result = -yaw;

        return result;
    }
    float[] refvel2refwheel(float linear_vel = 0.0f, float angular_vel = 0.0f)
    {
        // input : linear_vel[m/s], angular_velocity[rad/s
        // output : each wheel's rotating velocity [rad/s]

        float[] wheelsVel = new float[2] { 0.0f, 0.0f };
        wheelsVel[0] = (2 * linear_vel + angular_vel * TRACK) / (2 * WHEELRADIUS);  // left wheel [rad/s]
        wheelsVel[1] = (2 * linear_vel - angular_vel * TRACK) / (2 * WHEELRADIUS);  // right wheel [rad/s]

        return wheelsVel;
    }

    void WheelAnimation(float wl, float wr)
    {
        wl *= -1.0f;
        wr *= -1.0f;
        WheelLeftVisual.transform.Rotate(new Vector3(0.0f, Mathf.Rad2Deg * wl * Time.deltaTime, 0.0f));
        WheelRightVisual.transform.Rotate(new Vector3(0.0f, Mathf.Rad2Deg * wr * Time.deltaTime, 0.0f));
    }

}
