using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoEnv : MonoBehaviour
{
    private const int MAX_HUMAN = 8;
    private const float AVG_WALK_SPEED = 0.9f;
    private const int MAX_BOX = 3;
    private const int BOX_W = 1;
    private const float BOX_L = 0.5f;
    private const float WORLD_RANGE = 9.0f;   // x, z

    private Vector3 rr, rl, ll, lr;

    // gameobject that will recieve ready-made prefab
    private GameObject envParent;

    // instantiate prefabs
    public GameObject box;
    private GameObject actor;

    private List<motionController> acts = new List<motionController>();    // controller containers
    private List<GameObject> humans = new List<GameObject>();    // gameobject containers

    private float time = 0.0f;
    private bool reset = false;

    // Start is called before the first frame update
    void Awake()
    {
        envParent = GameObject.Find("AutoEnvGenerate");
        actor = Resources.Load("Prefabs/actor") as GameObject;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var a in acts)
            a.move(Directions.Front);
    }

    // two human walking straight
    private void H2WS()
    {
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
        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, -5.0f);
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

    // three human: 2 walking left straight, 1 walking right straight
    private void H2WLS_H1WRS()
    {
        // instantiate objects
        Vector3 pos = new Vector3(0.0f, 0.0f, -5.0f);
        GameObject h1 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h1.transform.Rotate(new Vector3(0.0f, -60.0f, 0.0f));
        h1.transform.SetParent(envParent.transform);
        humans.Add(h1);
        acts.Add(new motionController(h1.GetComponent<Animator>(), h1));

        pos = new Vector3(0.0f, 0.0f, 5.0f);
        GameObject h2 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h2.transform.Rotate(new Vector3(0.0f, -150.0f, 0.0f));
        h2.transform.SetParent(envParent.transform);
        humans.Add(h2);
        acts.Add(new motionController(h2.GetComponent<Animator>(), h2));

        pos = new Vector3(0.0f, 0.0f, 6.0f);
        GameObject h3 = Instantiate(actor, pos, Quaternion.identity) as GameObject;
        h3.transform.Rotate(new Vector3(0.0f, -120.0f, 0.0f));
        h3.transform.SetParent(envParent.transform);
        humans.Add(h3);
        acts.Add(new motionController(h3.GetComponent<Animator>(), h3));
    }

    private void removeGameObj()
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

    private void generateBoxes()
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
}
