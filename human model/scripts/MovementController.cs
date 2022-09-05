using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Directions
{
    public const uint Front = 0;
    public const uint Back = 1;
    public const uint Left = 2;
    public const uint Right = 3;
    public const uint Idle = 4;
}


public class motionController 
{
    // because using an animator
    // human walking speed is observed as average of 0.9m/s
    private Animator animator;
    private GameObject model;

    private float turnspeed = 0.3f;
    private float walkspeed = 0.05f;
    private float stopspeed = 0.1f;
    private float walk = 0.75f;  // idle state

    /*private uint direction = Directions.Stop;
    private uint sideways = Directions.Stop;*/

    public motionController(Animator ani, GameObject gb)
    {
        animator = ani;
        model = gb;
    }

    public void move(uint direction = Directions.Idle, uint sideways = Directions.Idle)
    {
        //******* keyboard input *******//

        /* difference stride by time */
        // front
        if (direction == Directions.Front)
            walk += (1f - walk) * walkspeed;

        // back
        else if (direction == Directions.Back)
            walk -= (walk - 0.5f) * walkspeed;

        else if (direction == Directions.Idle)// idle
        {
            // front fast decrease
            if (walk > 0.75f)
                walk -= (walk - 0.75f) * stopspeed;

            // back fast decrease
            else if (walk < 0.75f)
                walk += (0.75f - walk) * stopspeed;


            else walk = 0.75f;
        }

        animator.SetFloat("Move", walk, 0.1f, Time.deltaTime);  // updating x-axis stride

        /* linear angular turn by time while walking */
        if (Mathf.Abs(walk - 0.75f) > 0.1f)
        {
            // left
            if (sideways == Directions.Left) model.transform.Rotate(0.0f, turnspeed, 0.0f);

            // right
            else if (sideways == Directions.Right) model.transform.Rotate(0.0f, -turnspeed, 0.0f);
        }


        return;
    }


}

/*public class MovementController : MonoBehaviour
{
    public motionController actor1;
    public float time = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        Animator ani = GetComponent<Animator>();
        GameObject gb = GameObject.Find("human01"); // finding human01 object model
        actor1 = new motionController(ani, gb);
    }

    // Update is called once per frame
    void Update()
    {
        if (time < 3.0f)
            actor1.move(Directions.Front, Directions.Left);
        else if (time < 6.0f)
            actor1.move(Directions.Back, Directions.Right);
        else
            actor1.move();

        time += Time.deltaTime;
        Debug.Log(time);
    }

    
}*/
