using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWA;

public class AutoEnv : MonoBehaviour
{
    // private const int MAX_HUMAN = 8;
    // private const float AVG_WALK_SPEED = 0.9f;
    // private const int MAX_BOX = 3;
    // private const int BOX_W = 1;
    // private const float BOX_L = 0.5f;
    // private const float WORLD_RANGE = 9.0f;   // x, z

    // private Vector3 rr, rl, ll, lr;

    // gameobject that will recieve ready-made prefab
    private GameObject envParent;

    // instantiate prefabs
    public GameObject box;
    private GameObject actor;
    private GameObject goal;

    private List<motionController> acts = new List<motionController>();    // controller containers
    private List<GameObject> humans = new List<GameObject>();    // gameobject containers

    // human walk timer check
    private float timer_start = 0.0f;
    private float time_current = 0.0f;
    // all accessable
    public static bool reset_scenario = false;
    public static Vector3 vehicle_startPos = new Vector3();

    // private float time = 0.0f;
    // private bool reset = false;
    public int num = 0;

    // Start is called before the first frame update
    void Awake()
    {
        envParent = GameObject.Find("AutoEnvGenerate");
        actor = Resources.Load("Prefabs/actor") as GameObject;
        goal = GameObject.Find("SoftStar");
        
        //Generate(Random.Range(0, 5));
        Generate(num);
    }

    // Update is called once per frame
    void Update()
    {
        // temporary scenario check
        // checkEnvLooks();


        if (reset_scenario)
        {
            DestroyEnv();
            Generate(Random.Range(0, 6));
            reset_scenario = false;
        }

        time_current = Time.time - timer_start;

        if (time_current < 20.0f && time_current > 0.5f)
            Play(Directions.Front);
        else if (time_current > 20.0f)
        {
            DynamicWindowApproach.check_done = true;
            Play();
        }
        else
            Play();
    }

    private void checkEnvLooks()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DestroyEnv();
            num += 1;
            Generate(num);
            
        }

        time_current = Time.time - timer_start;

        if (time_current < 20.0f)
            Play(Directions.Front);
        else
            Play();
    }

    private void Play(uint dir = Directions.Idle, uint side = Directions.Idle)
    {
        
        foreach (var a in acts)
            a.move(dir, side);
    }

    private void Generate(int num = 0)
    {
        switch (num)
        {
            case 0:
                H2WS();
                break;
            case 1:
                H2WS2AMR();
                break;
            case 2:
                H1W2AMRG();
                break;
            case 3:
                H2W2AMRG();
                break;
            case 4:
                H2WLS_H1WRS();
                break;
            case 5:
                H4WRAND();
                break;
            default:
                H4WRAND();
                break;
        }
        timer_start = Time.time;
    }

    private void DestroyEnv()
    {
        // only has to be executed for once
        if (acts != null)
        {
            for (int i = acts.Count - 1; i > -1; i--)
                acts.RemoveAt(i);
        }

        if (humans != null)
        {
            for (int i = humans.Count - 1; i > -1; i--)
                Destroy(humans[i]);
        }
    }

    // two human walking straight
    private void H2WS()
    {
        // set goal position
        goal.transform.position = new Vector3(5.0f, 1.13f, 0.0f);
        DynamicWindowApproach.input_destination = new float[2] { 5.0f, 0.0f};
        vehicle_startPos = new Vector3(-5.5f, 0.02f, 0.0f);

        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, -1.3f);
        GameObject h1 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h1.transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
        h1.transform.SetParent(envParent.transform);
        humans.Add(h1);
        acts.Add(new motionController(h1.GetComponent<Animator>(), h1));

        pos = new Vector3(0.0f, 0.0f, 1.3f);
        GameObject h2 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h2.transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
        h2.transform.SetParent(envParent.transform);
        humans.Add(h2);
        acts.Add(new motionController(h2.GetComponent<Animator>(), h2));
    }

    // two human walking straight towards AMR
    private void H2WS2AMR()
    {
        // set goal position
        goal.transform.position = new Vector3(5.0f, 1.13f, 0.0f);
        DynamicWindowApproach.input_destination = new float[2] { 5.0f, 0.0f };
        vehicle_startPos = new Vector3(-5.5f, 0.02f, 0.0f);

        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, -2.0f);
        GameObject h1 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h1.transform.Rotate(new Vector3(0.0f, -60.0f, 0.0f));
        h1.transform.SetParent(envParent.transform);
        humans.Add(h1);
        acts.Add(new motionController(h1.GetComponent<Animator>(), h1));

        pos = new Vector3(0.0f, 0.0f, 2.0f);
        GameObject h2 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h2.transform.Rotate(new Vector3(0.0f, -120.0f, 0.0f));
        h2.transform.SetParent(envParent.transform);
        humans.Add(h2);
        acts.Add(new motionController(h2.GetComponent<Animator>(), h2));
    }

    // one human crossing path when AMR is traveling towards goal point
    private void H1W2AMRG()
    {
        // set goal position
        goal.transform.position = new Vector3(5.0f, 1.13f, 0.0f);
        DynamicWindowApproach.input_destination = new float[2] { 5.0f, 0.0f };
        vehicle_startPos = new Vector3(-5.5f, 0.02f, 0.0f);

        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, -6.0f);
        GameObject h1 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h1.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
        h1.transform.SetParent(envParent.transform);
        humans.Add(h1);
        acts.Add(new motionController(h1.GetComponent<Animator>(), h1));

        /*pos = new Vector3(0.0f, 0.0f, 9.0f);
        GameObject h2 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h2.transform.Rotate(new Vector3(0.0f, -180.0f, 0.0f));
        h2.transform.SetParent(envParent.transform);
        humans.Add(h2);
        acts.Add(new motionController(h2.GetComponent<Animator>(), h2));*/
    }


    // two human crossing path when AMR is traveling towards goal point
    private void H2W2AMRG()
    {
        // set goal position
        goal.transform.position = new Vector3(5.0f, 1.13f, 0.0f);
        DynamicWindowApproach.input_destination = new float[2] { 5.0f, 0.0f };
        vehicle_startPos = new Vector3(-5.5f, 0.02f, 0.0f);

        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, -6.0f);
        GameObject h1 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h1.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
        h1.transform.SetParent(envParent.transform);
        humans.Add(h1);
        acts.Add(new motionController(h1.GetComponent<Animator>(), h1));

        pos = new Vector3(1.2f, 0.0f, 6.8f);
        GameObject h2 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h2.transform.Rotate(new Vector3(0.0f, -180.0f, 0.0f));
        h2.transform.SetParent(envParent.transform);
        humans.Add(h2);
        acts.Add(new motionController(h2.GetComponent<Animator>(), h2));
    }

    // three human: 2 walking left straight, 1 walking right straight
    private void H2WLS_H1WRS()
    {
        // set goal position
        goal.transform.position = new Vector3(5.0f, 1.13f, 3.0f);
        DynamicWindowApproach.input_destination = new float[2] { 5.0f, 3.0f };
        vehicle_startPos = new Vector3(-5.5f, 0.02f, 0.0f);

        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, -4.0f);
        GameObject h1 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h1.transform.Rotate(new Vector3(0.0f, -60.0f, 0.0f));
        h1.transform.SetParent(envParent.transform);
        humans.Add(h1);
        acts.Add(new motionController(h1.GetComponent<Animator>(), h1));

        pos = new Vector3(0.0f, 0.0f, 3.0f);
        GameObject h2 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h2.transform.Rotate(new Vector3(0.0f, -150.0f, 0.0f));
        h2.transform.SetParent(envParent.transform);
        humans.Add(h2);
        acts.Add(new motionController(h2.GetComponent<Animator>(), h2));

        pos = new Vector3(0.0f, 0.0f, 4.2f);
        GameObject h3 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h3.transform.Rotate(new Vector3(0.0f, -120.0f, 0.0f));
        h3.transform.SetParent(envParent.transform);
        humans.Add(h3);
        acts.Add(new motionController(h3.GetComponent<Animator>(), h3));
    }

    private void H4WRAND()
    {
        // set goal position
        goal.transform.position = new Vector3(5.0f, 1.13f, 3.0f);
        DynamicWindowApproach.input_destination = new float[2] { 5.0f, 3.0f };
        vehicle_startPos = new Vector3(-5.5f, 0.02f, 0.0f);

        float randN = Random.Range(-1.0f, -4.0f);
        float randP = Random.Range(2.0f, 5.0f);
        float randNrot = Random.Range(-10.0f, -80.0f);
        float randProt = Random.Range(-100.0f, -170.0f);

        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, randN);
        GameObject h1 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h1.transform.Rotate(new Vector3(0.0f, randNrot, 0.0f));
        h1.transform.SetParent(envParent.transform);
        humans.Add(h1);
        acts.Add(new motionController(h1.GetComponent<Animator>(), h1));
        randNrot = Random.Range(-10.0f, -80.0f);

        pos = new Vector3(1.2f, 0.0f, randN-1.0f);
        GameObject h2 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h2.transform.Rotate(new Vector3(0.0f, randNrot, 0.0f));
        h2.transform.SetParent(envParent.transform);
        humans.Add(h2);
        acts.Add(new motionController(h2.GetComponent<Animator>(), h2));

        pos = new Vector3(-0.7f, 0.0f, randP);
        GameObject h3 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h3.transform.Rotate(new Vector3(0.0f, randProt, 0.0f));
        h3.transform.SetParent(envParent.transform);
        humans.Add(h3);
        acts.Add(new motionController(h3.GetComponent<Animator>(), h3));
        randProt = Random.Range(-100.0f, -170.0f);

        pos = new Vector3(2.0f, 0.0f, randP+1);
        GameObject h4 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h4.transform.Rotate(new Vector3(0.0f, randProt, 0.0f));
        h4.transform.SetParent(envParent.transform);
        humans.Add(h4);
        acts.Add(new motionController(h4.GetComponent<Animator>(), h4));
    }
    /*private void generateBoxes()
    {
        float[] rand_range = new float[2] { 0.0f, 0.0f };
        int num = Random.Range(1, MAX_BOX); // getting numbers of boxes to make
        List<Vector3> box_list = new List<Vector3>();

        // adding the first box
        Vector3 pos = new Vector3(Random.Range(-WORLD_RANGE, WORLD_RANGE), 0.0f, Random.Range(-WORLD_RANGE, WORLD_RANGE));
        box_list.Add(pos);
        var b_obj = Instantiate(box, pos, rotBoxes());
        b_obj.transform.SetParent(envParent.transform);

        for (int i=0; i<num-1; i++)
        {
            // for the next box
            pos = new Vector3(Random.Range(-WORLD_RANGE, WORLD_RANGE), 0.0f, Random.Range(-WORLD_RANGE, WORLD_RANGE));
            
            while(box_list.Exists(item => item == pos)) // if position already exists
            {

            }
            if(box_list.Exists(item => item==pos))  // should make another random position
            {

            }

        }
    }

    private void resetBoxes()
    {
        // destroying boxes

        // making new ones

    }

    private Quaternion rotBoxes()
    {
        if (Random.Range(0, 1) == 0)
            return Quaternion.identity;
        else
            return Quaternion.identity * Quaternion.Euler(new Vector3(0.0f, -1.0f, 0.0f));
    }
*/
}
