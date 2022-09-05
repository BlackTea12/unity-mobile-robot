using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*namespace VehicleStatus
{
    class diff2wheel
    {
        public float[] current_state;  
        public float[] error;
        public float[] error_dt;
        private int ss = 3;   // state size
        private int pl = 2;   // past length
        
        public diff2wheel(int state_size = 3, int past_length = 2)  // normally x, y, heading angle
        {
            // index lower: current val
            current_state = new float[state_size * past_length];   
            error = new float[state_size * past_length];
            error_dt = new float[state_size * past_length];
            
            ss = state_size;
            pl = past_length;
        }

        // (ss1 ss2 ss3) -> pl1,  (ss11 ss22 ss33) -> pl2, ... pl early means current value
 *//*       public void update(float dt)
        {
            for (int i = 0; i < pl; i++)
                for (int j = 0; j < ss; ss++)
                {
                    current_state[(i + 1) * ss + j] = current_state[i * ss + j];
                    error[(i + 1) * ss + j] = error[i * ss + j];
                    error_dt
                }
        }*//*

    }
}*/
public class VehicleController : MonoBehaviour
{
    // static constants
    private const float WHEELRADIUS = 0.0418f;  // wheel's radius [m]
    private const float TRACK = 0.05f;   // vehicle track's value [m]
    private const float VEHICLE_LENGTH = 0.5f;

    // Start is called before the first frame update
    public Rigidbody model;
    public WheelCollider wl, wr;
    public float desiredVel = 1.0f;

    // reference wheel rotation follow PID controller gain 'follow_wheel_ref()'
    private float w_P = 0.05f, w_I = 0.01f, w_D = 0.005f;
    // public float User_ref_wl = 24.0f, User_ref_wr = 24.0f;
    private float g_wl_err_I = 0.0f; // left wheel error global integration
    private float g_wr_err_I = 0.0f; // right wheel error global integration

    // controller gain
    public float SMC_KI = 0.25f;
    public float SMC_BETA = 0.0f;//2 * VEHICLE_LENGTH / 2.0f * (1 - Mathf.Sign(velocity));
    public float SMC_K = 0.1f;

    // SMC : globa error theta_e dot
    private float g_SMC_Z = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void FixedUpdate()
    {
        float[] torque = new float[2], ref_wheel = new float[2], ref_vel = new float[2], ref_point = new float[3];

        ref_point = TargetPointCalc();
        ref_vel = SMC(ref_point);   // choose a classical controller for this system
        ref_wheel = refvel2refwheel(ref_vel[0], ref_vel[1]);  // control reference linear 
        torque = follow_wheel_ref(ref_wheel[0], ref_wheel[1]);    // approximately 1m/s reference

        // limiting torque
        wl.motorTorque = Mathf.Clamp(torque[0], -5.0f, 5.0f);
        wr.motorTorque = Mathf.Clamp(torque[1], -5.0f, 5.0f);

    }

    // Update is called once per frame
    void Update()
    {

    }

    float[] follow_wheel_ref(float ref_lw = 24.0f, float ref_rw = 24.0f)
    {
        // input reference wheel rotation velocity [rad/s]
        // controlled by pid controller
        // in testfield, there is no need to implement this category

        float[] torque = new float[2] { 0.0f, 0.0f };

        // local error calculation
        float wl_err = ref_lw - wl.rpm * Mathf.PI / 30.0f;  // rpm -> rad/s
        float wr_err = ref_rw - wr.rpm * Mathf.PI / 30.0f;

        g_wl_err_I += wl_err;
        g_wr_err_I += wr_err;
        g_wl_err_I = Mathf.Clamp(g_wl_err_I, -30.0f, 30.0f);
        g_wr_err_I = Mathf.Clamp(g_wr_err_I, -30.0f, 30.0f);

        // compute torque PID
        /*torque[0] = w_P * wl_err[0] + w_I * (wl_err[0] + wl_err[1] + wl_err[2]) + w_D * (wl_err[1] - wl_err[0]) / Time.deltaTime;
        torque[1] = w_P * wr_err[0] + w_I * (wr_err[0] + wr_err[1] + wr_err[2]) + w_D * (wr_err[1] - wr_err[0]) / Time.deltaTime;*/
        torque[0] = w_P * (wl_err + w_I * g_wl_err_I + w_D * wl_err / Time.deltaTime);
        torque[1] = w_P * (wr_err + w_I * g_wr_err_I + w_D * wr_err / Time.deltaTime);

        // string checkMessage = wl_err[0].ToString("F4") + " / " + wr_err[0].ToString("F4");
        /*string checkMessage = wl_err.ToString("F4") + " / " + wr_err.ToString("F4");
        Debug.Log(checkMessage);*/
        return torque;
    }

    float[] SMC(float[] refPoint)
    {
        // input : global x, y, heading angle
        // specific input : for the reference journal, distance[1] and angular error[2] is input
        // output : reference constant linear and angular velocity [m/s], [rad/s]
        // only angular velocity is computed with constan linear velocity input
        float[] result_vel = new float[2] { 1.0f, 0.0f }; // [m/s], [rad/s]

        float z = SMC_KI * refPoint[1];

        result_vel[1] = result_vel[0] * (Mathf.Sin(refPoint[2]) + 5 * SMC_BETA);
        result_vel[1] -= SMC_KI * refPoint[1];
        result_vel[1] *= 1 / (VEHICLE_LENGTH / 2.0f - SMC_BETA);
        result_vel[1] -= SMC_K / (VEHICLE_LENGTH / 2.0f - SMC_BETA) * Mathf.Sign(SMC_BETA * refPoint[2] + g_SMC_Z + refPoint[1]);
        result_vel[1] = -result_vel[1];
        // update global smc variable
        g_SMC_Z += z * Time.deltaTime;

        Debug.Log(result_vel[1].ToString("F4"));
        return result_vel;
    }

    void PID()
    {
        // input : global x, y, heading angle
        // output : reference 
    }

    float[] TargetPointCalc(float Ld = 1)
    {
        // constant look ahead distance is set to 1[m] (default)
        // output making reference point x, y, heading angle for the controller
        // input vehicle's body position x, y and heading angle
        float[] refPoint = new float[3];

        // based on the current vehicle's velocity
        // it will decide which point to move on area Ld * velocity
        // road function or following trajectory should be given

        // 1. straight path (slightly away from the x = 0)
        // x = 3, z = moves 
        refPoint[0] = model.transform.position.z + Time.deltaTime * Ld;
        refPoint[1] = 0.0f;
        refPoint[2] = Mathf.Atan2(3, refPoint[0]);

        // ** SMC particular input
        // use refPoint[2] as angular error
        // use refPoint[1] as distance error
        refPoint[1] = Mathf.Sqrt(Mathf.Pow(3-model.transform.position.x, 2.0f) + Mathf.Pow(Time.deltaTime * Ld, 2f));
        return refPoint;
    }

    float[] refvel2refwheel(float linear_vel, float angular_vel)
    {
        // input : linear_vel[m/s], angular_velocity[rad/s
        // output : each wheel's rotating velocity [rad/s]

        float[] wheelsVel = new float[2] { 0.0f, 0.0f };
        wheelsVel[0] = (2 * linear_vel + angular_vel * TRACK) / (2 * WHEELRADIUS);  // left wheel [rad/s]
        wheelsVel[1] = (2 * linear_vel - angular_vel * TRACK) / (2 * WHEELRADIUS);  // right wheel [rad/s]

        return wheelsVel;
    }
}
