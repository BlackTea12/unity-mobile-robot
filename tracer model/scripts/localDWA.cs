using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWA;

namespace DWA
{    
    public class DynamicWindowApproach 
    { 
        // reinforcement training parametrs
        public static float[] v_objective = new float[3];  // v_head, v_vel, v_dist
        public static float input_target_head;
        public static int v_N = 2; // [sec]

        // interacting with Unity vehicle (idealControl)
        public static float[] cur_vehiclePos = new float[3];  // [x, z, yaw] : [m], [m], [rad]
        public static float[] cur_vehicleCommand = new float[2];  // [v, w] : [m/s], [rad/s], [rad]
        public static float[] g_result_dwa_ctr = new float[2] { 0.0f, 0.0f };  // control result from dwa
        public static float[] input_destination = new float[2];   // [x, z] : [m], [m]
        public static bool check_collision = false;
        public static bool check_done = false;
        public static bool check_reset = false;

        // RL connected
        public static bool client_check = false;

        // constant value
        private const float COLLIDE_DIST = 2.0f;    // vehicle C.O.M to obj detected hit point
        private const float MAX_LIN_VEL = 0.8f; // maximum linear velocity [m/s]
        private const float MAX_ROT_VEL = Mathf.PI / 2.0f;  // maximum rotation velocity [rad/s]
        private const float INTV_LIN_VEL = 0.05f;
        private const float INTV_ROT_VEL = Mathf.Deg2Rad * 3.0f;

        // update through frame
        // have to change updateParameter() function same
        private float MAX_LIN_ACC = 0.3f; // maximum linear acceleration [m/s^2]
        private float MAX_ROT_ACC = Mathf.Deg2Rad * 60.0f;    // maximum rotation acceleration [rad/s^2]
        public DynamicWindowApproach()
        {
            // Instantiate
            v_objective[0] = 0.1f;   // var, heading
            v_objective[1] = 0.1f;  // var, velocity
            v_objective[2] = 0.3f;  // var, distance
            input_target_head = 0.0f;   // [rad]
            input_destination = new float[2] { 5.8f, 1.73f };
        }

        public static void reset_env()
        {
            DynamicWindowApproach.v_objective[0] = 0.1f;   // var, heading
            DynamicWindowApproach.v_objective[1] = 0.1f;  // var, velocity
            DynamicWindowApproach.v_objective[2] = 0.3f;  // var, distance
            DynamicWindowApproach.v_N = 2; // [sec]
            DynamicWindowApproach.check_collision = false;
            DynamicWindowApproach.check_done = false;
            input_target_head = 0.0f;   // [rad]
            input_destination = new float[2] { 5.8f, 1.73f };
        }

        private void updateParameter()
        {
            MAX_LIN_ACC = 0.3f / Time.deltaTime; // maximum linear acceleration [m/s^2]
            MAX_ROT_ACC = Mathf.Deg2Rad * 60.0f / Time.deltaTime;    // maximum rotation acceleration [rad/s^2]
        }

        public void findPath(float[] obj)   // main
        {
            // input obj position x1, y1, x2, y2, ...
            // ouput vel: v[m/s], w[rad/s]
            float[] command = new float[2];
            
            updateParameter();
            float[] Vr = calcDyanmicWindow();
            //Debug.Log(Vr[0].ToString("F3") + ",  " + Vr[1].ToString("F3") + ",  " + Vr[2].ToString("F3")+ ",  " + Vr[3].ToString("F3"));
            //float[] Vr = new float[4] { 0.0f, 0.5f, -Mathf.PI / 6.0f, Mathf.PI / 6.0f };
            (float[,] EVAL, float[,] TRAJ, int eval_length, int traj_length) = SearchSpace(Vr, obj);  // evalulation matrix with all trajectories from dwa
            //Debug.Log("Vr = " + string.Join(" ", new List<float>(Vr).ConvertAll(i => i.ToString()).ToArray()));
            //Debug.Log(traj_length.ToString());
            /*for (int i = 0; i < traj_length; i++)//for (int i = 0; i < traj_length; i++)
            {
                //Debug.Log(TRAJ[i, 2].ToString("F3"));
                Debug.DrawLine(new Vector3(cur_vehiclePos[0], 0.3f, cur_vehiclePos[1]), new Vector3(TRAJ[i, 0], 0.3f, TRAJ[i, 1]), Color.blue);
            }*/

            int ctr_index = normalizeEval(EVAL, eval_length);    // normalize and sum to make objective function then find maximum value and return index
            
            //Debug.Log(ctr_index);
            
            g_result_dwa_ctr[0] = EVAL[ctr_index, 0];    // v
            g_result_dwa_ctr[1] = EVAL[ctr_index, 1];    // w
            //Debug.Log("v: "+ EVAL[ctr_index, 0]+ "w: " + EVAL[ctr_index, 1]+ "head: "+ EVAL[ctr_index, 2]+ "vel: "+EVAL[ctr_index, 3] + "dist:" + EVAL[ctr_index, 4]);
            /*float x = cur_vehiclePos[0] + Mathf.Cos(cur_vehiclePos[2]) * Time.deltaTime * g_result_dwa_ctr[0];   // x
            float z = cur_vehiclePos[1] + Mathf.Sin(cur_vehiclePos[2]) * Time.deltaTime * g_result_dwa_ctr[0];   // z
            //Debug.Log(x.ToString("F3") + " / " + z.ToString("F3"));
            Debug.DrawLine(new Vector3(cur_vehiclePos[0], 0.0f, cur_vehiclePos[1]), new Vector3(3 * x, 0.0f, 3 * z), Color.blue);*/
            //Debug.DrawLine(new Vector3(cur_vehiclePos[0], 0.3f, cur_vehiclePos[1]), new Vector3(1.5f * TRAJ[ctr_index, 0], 0.3f, 1.5f * TRAJ[ctr_index, 1]), Color.green);

            checkDone();
        }

        private void checkDone()
        {
            // destination arrived
            float pos = Mathf.Sqrt(Mathf.Pow(cur_vehiclePos[0] - input_destination[0], 2) + Mathf.Pow(cur_vehiclePos[1] - input_destination[1], 2));
            if (pos < 0.25f)
                DynamicWindowApproach.check_done = true;
            /*else
                check_done = false;*/
        }

        private (float[,], float[,], int, int) SearchSpace(float[] Vr, float[] obj)
        {
            //int col = Mathf.FloorToInt(((Vr[1] - Vr[0]) / INTV_LIN_VEL + 1) * ((Vr[3] - Vr[2]) / INTV_ROT_VEL + 1));
            int tr_col = Mathf.RoundToInt(v_N / Time.deltaTime);   // for trajectory
            //float[,] EVAL = new float[col, 5], TRAJ = new float[tr_col * col, 3];
            int i = 0; //, j = 0;
            float[,] EVAL, TRAJ;
            // get the size of initiating variables
            for (float v = Vr[0]; v <= Vr[1]; v += INTV_LIN_VEL)
                for (float w = Vr[2]; w <= Vr[3]; w += INTV_ROT_VEL)
                    i += 1;
            if (i != 0)
            {
                EVAL = new float[i, 5];
                TRAJ = new float[tr_col * i, 3];
            
                //float[,] EVAL = new float[i, 5], TRAJ = new float[tr_col * i, 3];// TRAJ = new float[i, 3];//TRAJ = new float[tr_col * i, 3];

                i = 0;
                for (float v=Vr[0]; v <=Vr[1]; v += INTV_LIN_VEL)
                {
                    for (float w = Vr[2]; w <= Vr[3]; w += INTV_ROT_VEL)
                    {
                        float[,] trj = trajectoryMake(v, w);
                        //float[] h = new float[3] { trj[tr_col - 1, 0], trj[tr_col - 1, 1], trj[tr_col - 1, 2] };
                        //Debug.Log(h[0].ToString("F3") + " and " + h[1].ToString("F3"));
                        float heading = calcHeading(new float[3] { trj[tr_col - 1, 0], trj[tr_col - 1, 1], trj[tr_col - 1, 2] });   // input end predicted state
                        float velocity = calcVel(v);
                        float distance = calcDist(obj, new float[2] { trj[tr_col - 1, 0], trj[tr_col - 1, 1] });    // input end predicted state
                        //Debug.Log(i.ToString() + ", " + distance);
                        //Debug.Log(i.ToString() + ", " + h[0].ToString("F3") + " and " + h[1].ToString("F3") + " and " + h[2].ToString("F3")+" and " + distance);
                        EVAL[i, 0] = v;
                        EVAL[i, 1] = w;
                        EVAL[i, 2] = heading;
                        EVAL[i, 3] = velocity;
                        EVAL[i, 4] = distance;

                        int n = 0;
                        for (int m = tr_col * i; m < tr_col * (i + 1); m++)
                        {
                            TRAJ[m, 0] = trj[n, 0];
                            TRAJ[m, 1] = trj[n, 1];
                            TRAJ[m, 2] = trj[n, 2];
                            n += 1;
                        }
                        /*// end point only
                        TRAJ[i, 0] = trj[tr_col - 1, 0];
                        TRAJ[i, 1] = trj[tr_col - 1, 1];
                        TRAJ[i, 2] = trj[tr_col - 1, 2];*/
                        i += 1;
                    }                
                }
            }
            else
            {
                Debug.Log("no path");
                EVAL = new float[1, 5];
                TRAJ = trajectoryMake(0.0f, 0.0f);
                return (EVAL, TRAJ, 0, 3);
            }
            //Debug.Log(i.ToString() + ", " + col.ToString());
            return (EVAL, TRAJ, i, i*tr_col);
        }

        private float[,] trajectoryMake(float v, float w)
        {
            int col = Mathf.RoundToInt(v_N / Time.deltaTime);
            // x, y, heading container
            float[,] traj = new float[col, 3];

            int dt = 0;
            while (true)
            {
                if (dt == col)
                    break;

                if(dt == 0)
                {
                    traj[dt, 0] = cur_vehiclePos[0] + Mathf.Cos(cur_vehiclePos[2]) * Time.deltaTime * v;   // x
                    traj[dt, 1] = cur_vehiclePos[1] + Mathf.Sin(cur_vehiclePos[2]) * Time.deltaTime * v;   // z
                    traj[dt, 2] = cur_vehiclePos[2] + Time.deltaTime * w;   // heading
                }
                else
                {
                    traj[dt, 0] = traj[dt - 1, 0] + Mathf.Cos(traj[dt - 1, 2]) * Time.deltaTime * v;   // x
                    traj[dt, 1] = traj[dt - 1, 1] + Mathf.Sin(traj[dt - 1, 2]) * Time.deltaTime * v;   // z
                    traj[dt, 2] = traj[dt - 1, 2] + Time.deltaTime * w;   // heading
                }
                
                dt += 1;
            }
            // Debug.Log(col.ToString() + " , " + Time.deltaTime);
            //Debug.Log(v.ToString("F3") + " , " + w.ToString("F3"));
            //Debug.Log("x: "+traj[dt-1, 0].ToString("F3")+ "z: " + traj[dt - 1, 1].ToString("F3"));

            return traj;
        }

        private float[] calcDyanmicWindow()
        {
            float[] Vr = new float[4] ;  // vmin, vmax, wmin, wmax
            Vr[0] = Mathf.Max(0, cur_vehicleCommand[0] - MAX_LIN_ACC * Time.deltaTime);
            Vr[1] = Mathf.Min(MAX_LIN_VEL, cur_vehicleCommand[0] + MAX_LIN_ACC * Time.deltaTime);
            Vr[2] = Mathf.Max(-MAX_ROT_VEL, cur_vehicleCommand[1] - MAX_ROT_ACC * Time.deltaTime);
            Vr[3] = Mathf.Min(MAX_ROT_VEL, cur_vehicleCommand[1] + MAX_ROT_ACC * Time.deltaTime);
            return Vr;
        }                                                                                                                                   

        private int normalizeEval(float[,] oldEval, int lengthSize)
        {
            // oldEVAL has  2, 3, 4 element refers to heading, velocity, distance
            float[] newEval = new float[lengthSize];
            float[] sumElements = new float[3] { 0.0f, 0.0f, 0.0f };
            float[,] normEval = oldEval;    // make a copy
            int maxCol = 0;
            float maxVal = 0.0f;

            // get sum of heading, velocity, distance elements
            for (int i = 0; i < lengthSize; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    sumElements[j] += oldEval[i, j + 2];
                }
            }

            /*if(sumElements[0] != 0) // heading
            {
                for (int i = 0; i < oldEval.GetLength(0); i++)
                {
                    normEval[i, 2] /= sumElements[0];
                }
            }
            if (sumElements[1] != 0) // velocity
            {
                for (int i = 0; i < oldEval.GetLength(0); i++)
                {
                    normEval[i, 3] /= sumElements[1];
                }
            }
            if (sumElements[2] != 0) // distance
            {
                for (int i = 0; i < oldEval.GetLength(0); i++)
                {
                    normEval[i, 4] /= sumElements[2];
                }
            }

            // final eval
            for (int i = 0; i < oldEval.GetLength(0); i++)
            {
                newEval[i] = v_objective[0] * normEval[i, 0] + v_objective[1] * normEval[i, 1] + v_objective[2] * normEval[i, 2];
                if (maxVal < newEval[i])
                {
                    maxVal = newEval[i];
                    maxCol = i;
                }
            }*/
            //Debug.Log(oldEval[maxCol,2].ToString("F5") + ", " + oldEval[maxCol, 3].ToString("F5") + ", " + oldEval[maxCol, 4].ToString("F5"));
            for (int i = 0; i < lengthSize; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (sumElements[j] != 0.0f)
                    {
                        newEval[i] += v_objective[j] * oldEval[i, j + 2] / sumElements[j];
                    }
                    else
                    {
                        newEval[i] += v_objective[j] * oldEval[i, j + 2];
                    }
                }
                //Debug.Log("ith( "+i.ToString() + " ) : " + newEval[i].ToString() + ", heading: " + oldEval[i,2].ToString("F3") + ", velocity: " + oldEval[i, 3].ToString("F3") + ", distance: " + oldEval[i, 4].ToString("F3"));
                if (maxVal < newEval[i])
                {
                    maxVal = newEval[i];
                    maxCol = i;
                    //Debug.Log(oldEval[maxCol, 1]);
                }
            }
            //Debug.Log(maxCol);
            // find maximum value of 
            return maxCol;
        }

        private float calcVel(float v)
        {
            float result_vel = Mathf.Abs(v);
            return result_vel;
        }

        public static float calcHeading(float[] VehiclePoint)
        {
            float head = 0.0f;

            float diff_angle = Mathf.Atan2(input_destination[1] - VehiclePoint[1], input_destination[0] - VehiclePoint[0]); // input_target_head - vehiclePos[3];   // [rad]
            //float check_angle = diff_angle * Mathf.Rad2Deg;
            //Debug.Log("Angle difference: " + check_angle.ToString("F3"));
            //VehiclePoint[2] = Mathf.PI * 2.0f -VehiclePoint[2];
            //Debug.Log("Angle difference: " + diff_angle.ToString("F3") + ",   vehicle: " + VehiclePoint[2].ToString("F3") + ",   x: " + VehiclePoint[0].ToString("F3") + ",   z: " + VehiclePoint[1].ToString("F3"));
            if (diff_angle > VehiclePoint[2])
                head = diff_angle - VehiclePoint[2];
            else
                head = VehiclePoint[2] - diff_angle;

            head = Mathf.PI - head;
            head = Mathf.Rad2Deg * head;
            //Debug.Log(head.ToString("F3"));
            return head;
        }

        private float calcDist(float[] obj, float[] VehiclePoint)
        {
            // input : objects that is detected within sensor range
            // [x1, z1, x2, z1, ...]
            float obj_min_dist = COLLIDE_DIST;
            for (int i = 0; i < obj.Length / 2; i++)
            {
                float tdist = Mathf.Sqrt(Mathf.Pow(VehiclePoint[0] - obj[2 * i], 2) + Mathf.Pow(VehiclePoint[1] - obj[2 * i + 1], 2));
                //Debug.Log(tdist);
                if (obj_min_dist > tdist)
                    obj_min_dist = tdist;
            }
            
            //manually
            /*float obj_min_dist = COLLIDE_DIST;
            float tdist = Mathf.Sqrt(Mathf.Pow(VehiclePoint[0] - 2.582f, 2) + Mathf.Pow(VehiclePoint[1] - 1.73f, 2));
            //Debug.Log(tdist);
            if (obj_min_dist > tdist)
                obj_min_dist = tdist;*/

            /*Debug.Log(obj.Length / 2);*/
            //Debug.Log(obj_min_dist + ",   " + tdist);
            return obj_min_dist;
        }
    }
}

public class localDWA : MonoBehaviour
{
    public DynamicWindowApproach localPlanner;

    [Header("Sensors")]
    private float sensor_length = 6.0f;    // [m]
    private int sensor_resolution = 81;
    private float sensor_range = 80.0f;      // [degree]

    private GameObject base_obj;
    // Start is called before the first frame update
    void Start()
    {
        localPlanner = new DynamicWindowApproach();
        base_obj = GameObject.Find("localDWA");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float[] obj = Sensors();
        localPlanner.findPath(obj); //obj position x1, y1, x2, y2, ...
    }

    private void OnTriggerEnter(Collider other)
    {
        DynamicWindowApproach.check_collision = true;
    }

    /*private void OnTriggerExit(Collider other)
    {
        DynamicWindowApproach.check_collision = false;
    }*/

    private float[] Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartingPos = base_obj.transform.position;
        Vector3[] sensor_rays = new Vector3[sensor_resolution];
        List<float> objList = new List<float>();

        /*// front sensor
        if(Physics.Raycast(sensorStartingPos, Quaternion.AngleAxis(90.0f, transform.up)* base_obj.transform.right, out hit, sensor_length))
        {
            Debug.Log("distance: " + hit.distance);
        }*/
        float resolution = sensor_range / (sensor_resolution - 1);

        for (int i = 0; i < sensor_resolution; i++)
        {
            if (i < (sensor_resolution - 1) / 2)
            {
                if (Physics.Raycast(sensorStartingPos, Quaternion.AngleAxis(90.0f - resolution * (i+1), transform.up) * base_obj.transform.right, out hit, sensor_length))
                {
                    // Debug.Log("distance: " + hit.distance);
                    //Debug.DrawLine(sensorStartingPos, hit.point, Color.red);
                    objList.Add(hit.point.x);
                    objList.Add(hit.point.z);
                }
            }
            else if (i == (sensor_resolution - 1) / 2)
            {
                if (Physics.Raycast(sensorStartingPos, Quaternion.AngleAxis(90.0f, transform.up) * base_obj.transform.right, out hit, sensor_length))
                {
                    // Debug.Log("distance: " + hit.distance);
                    //Debug.DrawLine(sensorStartingPos, hit.point, Color.red);
                    objList.Add(hit.point.x);
                    objList.Add(hit.point.z);
                }
            }

            else
            {
                if (Physics.Raycast(sensorStartingPos, Quaternion.AngleAxis(90.0f + resolution * (sensor_rays.Length - i - 1), transform.up) * base_obj.transform.right, out hit, sensor_length))
                {
                    // Debug.Log("distance: " + hit.distance);
                    //Debug.DrawLine(sensorStartingPos, hit.point, Color.red);
                    objList.Add(hit.point.x);
                    objList.Add(hit.point.z);
                }
            }
        }

        float[] objs = objList.ToArray();

        //Debug.Log(objs.Length);
        return objs;
    }

}
