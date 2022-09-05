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
public class smc : MonoBehaviour
{
    // static constants of vehicle
    private const float WHEELRADIUS = 0.0418f;  // wheel's radius [m]
    private const float TRACK = 0.36f;   // vehicle track's value [m]
    private const float VEHICLE_LENGTH = 0.5f;
    // Start is called before the first frame update
    public Rigidbody model;
    public WheelCollider wl, wr;
    public float desiredVel = 1.0f;
    // reference wheel rotation follow PID controller gain 'follow_wheel_ref()'
    public float w_P = 0.05f, w_I = 0.01f, w_D = 0.005f;

    //wheel torque tuned gain
    private float P = 1175.0f;
    private float I = 300.0f;
    public float P_term_l = 0.0f;
    public float I_Term_l = 0.0f;
    public float P_term_r = 0.0f;
    public float I_Term_r = 0.0f;

    // public float User_ref_wl = 24.0f, User_ref_wr = 24.0f;
    private float g_wl_err_I = 0.0f; // left wheel error global integration
    private float g_wr_err_I = 0.0f; // right wheel error global integration
    // controller gain
    public float SMC_KI = 0.25f;
    public float SMC_BETA = 0.0f;//2 * VEHICLE_LENGTH / 2.0f * (1 - Mathf.Sign(velocity));
    public float SMC_K = 0.1f;
    // SMC : globa error theta_e dot
    private float g_SMC_Z = 0.0f;
    private const float SMC_LP = 1.5f;  // look ahead point [m]
    private const float SMC_C = 5.0f;   // road curvature
    // Start is called before the first frame update
    void Start()
    {
    }
    private void FixedUpdate()
    {
        float[] torque = new float[2], ref_wheel = new float[2], ref_vel = new float[2], ref_point = new float[3];
        ref_point = TargetPointCalc();
        ref_vel = SMC(ref_point);   // choose a classical controller for this system
        ref_vel[1] = -ref_vel[1];

        float angularvel = ref_vel[1] * Mathf.Rad2Deg;
        Debug.Log("linear: " + ref_vel[0].ToString("F3") + "[m/s],   angular: " + angularvel.ToString("F2") + "[deg/s] control input");
        ref_wheel = refvel2refwheel(ref_vel[0], ref_vel[1]);  // control reference linear
        //ref_wheel = refvel2refwheel(0.3f, 0.0f);
        torque = follow_wheel_ref(ref_wheel[0], ref_wheel[1]);    // approximately 1m/s reference
        // limiting torque
        /*wl.motorTorque = Mathf.Clamp(torque[0], -15.0f, 15.0f);
        wr.motorTorque = Mathf.Clamp(torque[1], -15.0f, 15.0f);*/
        //
        wl.motorTorque = Time.deltaTime * torque[0];
        wr.motorTorque = Time.deltaTime * torque[1];

        //Debug.Log("left torque: " + wl.motorTorque.ToString("F3") + "[N],   left torque: " + wr.motorTorque.ToString("F3") + "[N]");
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

        ref_lw *= 0.0418f;
        ref_rw *= 0.0418f;

        float L_tempERR = ref_lw - (wl.rpm * 0.1047f * wl.radius);
        float R_tempERR = ref_rw - (wr.rpm * 0.1047f * wr.radius);

        P_term_l = P * L_tempERR;
        I_Term_l += L_tempERR;
        I_Term_l = Mathf.Clamp(I_Term_l, 0, 1);
        R_tempERR = ref_rw - (wr.rpm * 0.1047f * wr.radius);
        P_term_r = P * R_tempERR;
        I_Term_r += R_tempERR;
        I_Term_r = Mathf.Clamp(I_Term_r, 0, 1);
        torque[1] = P_term_r + I * I_Term_r;
        torque[0] = P_term_l + I * I_Term_l;

        //Debug.Log(L_tempERR);

        return torque;
    }
    float[] SMC(float[] refPoint)
    {
        // input : global x, y, heading angle
        // specific input : for the reference journal, distance[1] and angular error[2] is input
        // output : reference constant linear and angular velocity [m/s], [rad/s]
        // only angular velocity is computed with constan linear velocity input
        /*refPoint[2] = model.transform.rotation.y - Mathf.PI / 2.0f;
        refPoint[1] = 0.05f * Mathf.Sin(refPoint[2]);*/
        float[] result_vel = new float[2] { 0.5f, 0.0f }; // [m/s], [rad/s]
        float z_dot = SMC_KI * refPoint[1];
        /*result_vel[1] = result_vel[0] * (Mathf.Sin(refPoint[2]) + 5 * SMC_BETA);
        result_vel[1] -= SMC_KI * refPoint[1];
        result_vel[1] *= 1 / (VEHICLE_LENGTH / 2.0f - SMC_BETA);
        result_vel[1] -= SMC_K / (VEHICLE_LENGTH / 2.0f - SMC_BETA) * Mathf.Sign(SMC_BETA * refPoint[2] + g_SMC_Z + refPoint[1]);
        result_vel[1] = -result_vel[1];*/
        float Ueq = 1 / (SMC_LP - SMC_BETA) * (result_vel[0] * (Mathf.Sin(refPoint[2]) + SMC_BETA * SMC_C) - SMC_KI * refPoint[1]);
        float sigma = g_SMC_Z + refPoint[1] + SMC_BETA * refPoint[2];
        // U
        result_vel[1] = Ueq - SMC_K * Mathf.Sign(sigma) / (SMC_LP - SMC_BETA);
        // update global smc variable
        g_SMC_Z += z_dot * Time.deltaTime;
        g_SMC_Z = Mathf.Clamp(g_SMC_Z, 0.0f, 10.0f);
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
        /*refPoint[0] = model.position.z + Time.deltaTime * Ld;
        refPoint[1] = 0.0f;
        refPoint[2] = Mathf.Atan2(3, refPoint[0]);*/
        // ** SMC particular input
        // use refPoint[2] as angular error
        // use refPoint[1] as distance error
        refPoint[2] = model.rotation.y;// - Mathf.PI / 2;   // 0
        refPoint[1] = 2.0f; // [m]
        float z_coord;  // smc reference trajectory : straight line
        float lp_x;
        float lp_z;
        if (model.position.x <=2.0f)
        {
            lp_x = model.position.x + SMC_LP * Mathf.Cos(model.rotation.y);
            lp_z = model.position.z + SMC_LP * Mathf.Sin(model.rotation.y);
        }
        else
        {
            lp_x = model.position.x - SMC_LP * Mathf.Cos(model.rotation.y);
            lp_z = model.position.z + SMC_LP * Mathf.Sin(model.rotation.y);
        }
        /*if (model.rotation.y < Mathf.Deg2Rad * 5.0f)
        {
            // z_coord = lp_z + (lp_x - refPoint[1]) / Mathf.Tan(model.rotation.y);
            // maybe aligned on the line
            z_coord = 0.0f;
        }
        else
        {
            //z_coord = lp_z + (lp_x - refPoint[1]) / Mathf.Tan(model.rotation.y);
            z_coord = (model.position.x + SMC_LP * Mathf.Sin(model.rotation.y) - refPoint[1]) / Mathf.Cos(model.rotation.y);
            z_coord = Mathf.Abs(z_coord);
        }*/
        /*refPoint[1] = Mathf.Sqrt(Mathf.Pow(lp_x - refPoint[1], 2) + Mathf.Pow(lp_z - z_coord, 2));
        refPoint[1] = Mathf.Sin(refPoint[2]) * SMC_LP;*/
        if (model.rotation.y == 0.0f) z_coord = 0.0f;
        else if(model.position.x <=2.0f)
        {
            z_coord = (model.position.x + SMC_LP * Mathf.Sin(model.rotation.y) - refPoint[1]) / Mathf.Cos(model.rotation.y);
            z_coord = Mathf.Abs(z_coord);
        }
        else
        {
            z_coord = (refPoint[1] - model.position.x - SMC_LP * Mathf.Sin(model.rotation.y)) / Mathf.Cos(model.rotation.y);
            //z_coord = Mathf.Abs(z_coord);
        }
        refPoint[1] = z_coord;
        return refPoint;
    }
    float[] refvel2refwheel(float linear_vel, float angular_vel)
    {
        // input : linear_vel[m/s], angular_velocity[rad/s
        // output : each wheel's rotating velocity [rad/s]
        float[] wheelsVel = new float[2] { 0.3f, 0.0f };
        angular_vel = Mathf.Clamp(angular_vel, -Mathf.PI / 2, Mathf.PI / 2);
        wheelsVel[0] = (2 * linear_vel + angular_vel * TRACK) / (2 * WHEELRADIUS);  // left wheel [rad/s]
        wheelsVel[1] = (2 * linear_vel - angular_vel * TRACK) / (2 * WHEELRADIUS);  // right wheel [rad/s]
        return wheelsVel;
    }
}