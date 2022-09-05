using System.Text;
using UnityEngine;
using DWA;

/*public class RLspaces : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}*/
namespace RLparam
{
    public class RL
    {
        // observation space
        // vehicle linear velocity, vehicle rotation velocity, goal left distance, goal left angle

        // action space
        // alpha, beta, gamma, N_predict

        // public static float, interacting with localDWA
        public static string msg = "";
        public RL()
        {

        }

        public string sendObs()
        {
            // expected results
            // point 3 under decimal limited
            // ex: 1.000 3.140 3.000 3.140 0 0
            // seperated by space
            string data = "";//"a";

            data += Mathf.Abs(DynamicWindowApproach.cur_vehicleCommand[0]).ToString("F3");
            data += " " + Mathf.Abs(DynamicWindowApproach.cur_vehicleCommand[1]).ToString("F3");

            float dist = Mathf.Sqrt(Mathf.Pow(DynamicWindowApproach.input_destination[0] - DynamicWindowApproach.cur_vehiclePos[0], 2) + Mathf.Pow(DynamicWindowApproach.input_destination[1] - DynamicWindowApproach.cur_vehiclePos[1], 2));
            float head = DynamicWindowApproach.calcHeading(DynamicWindowApproach.cur_vehiclePos);

            data += " " + Mathf.Abs(dist).ToString("F3");
            data += " " + Mathf.Abs(head).ToString("F3");


            /*if (Mathf.Sign(DynamicWindowApproach.cur_vehicleCommand[0]) < 0)    // -
                data += "-" + Mathf.Abs(DynamicWindowApproach.cur_vehicleCommand[0]).ToString("F3");
            else // +
                data += "+" + Mathf.Abs(DynamicWindowApproach.cur_vehicleCommand[0]).ToString("F3");

            if (Mathf.Sign(DynamicWindowApproach.cur_vehicleCommand[1]) < 0)    // -
                data += " -" + Mathf.Abs(DynamicWindowApproach.cur_vehicleCommand[1]).ToString("F3");
            else // +
                data += " +" + Mathf.Abs(DynamicWindowApproach.cur_vehicleCommand[1]).ToString("F3");*/

            // DynamicWindowApproach.cur_vehicleCommand[0].ToString("F3") + " " + DynamicWindowApproach.cur_vehicleCommand[1].ToString("F3");

            /*float dist = Mathf.Sqrt(Mathf.Pow(DynamicWindowApproach.input_destination[0] - DynamicWindowApproach.cur_vehiclePos[0], 2) + Mathf.Pow(DynamicWindowApproach.input_destination[1] - DynamicWindowApproach.cur_vehiclePos[1], 2));
            float head = DynamicWindowApproach.calcHeading(DynamicWindowApproach.cur_vehiclePos);

            if (Mathf.Sign(dist) < 0)    // -
                data += " " + Mathf.Abs(dist).ToString("F3");
            else // +
                data += " +" + Mathf.Abs(dist).ToString("F3");

            if (Mathf.Sign(head) < 0)    // -
                data += " -" + Mathf.Abs(head).ToString("F3");
            else // +
                data += " +" + Mathf.Abs(head).ToString("F3");
*/
            //data += " " + dist.ToString("F3") + " " + head.ToString("F3");


            if (DynamicWindowApproach.check_done)   // true
                data += " " + 1;
            else
                data += " " + 0;

            if (DynamicWindowApproach.check_collision) // true
                data += " " + 1;
            else
                data += " " + 0;// + "a";


            //int data_length = Encoding.Default.GetBytes(data).Length;
            data = "A" + data;
            // int length = data.Length;
            // Debug.Log(length);
            // data = length + data;
            // Debug.Log(data);
            return data;
        }

        public void recvAct()//(string act)
        {
            // expected to get input string divided by space
            // get 4 data
            string[] data;
            if (msg.Length != 0)
            {
                data = msg.Split(' ');
                for (int i = 0; i < 3; i++)
                    DynamicWindowApproach.v_objective[i] = float.Parse(data[i]);

                DynamicWindowApproach.v_N = Mathf.RoundToInt(float.Parse(data[3]));

                Debug.Log(DynamicWindowApproach.v_objective[0].ToString("F3") + " " + DynamicWindowApproach.v_objective[1].ToString("F3") + " " + DynamicWindowApproach.v_objective[2].ToString("F3") + " " + DynamicWindowApproach.v_N.ToString() + "  goal arrived: " + DynamicWindowApproach.check_done + ",  collision event: " + DynamicWindowApproach.check_collision);
            }

            /*for (int i = 0; i < 3; i++)
                DynamicWindowApproach.v_objective[i] = float.Parse(data[i]);

            DynamicWindowApproach.v_N = Mathf.RoundToInt(float.Parse(data[3]));

            Debug.Log(DynamicWindowApproach.v_objective[0].ToString("F3") + " " + DynamicWindowApproach.v_objective[1].ToString("F3") + " " + DynamicWindowApproach.v_objective[2].ToString("F3") + " " + DynamicWindowApproach.v_N.ToString());
            */
        }
    }
}
