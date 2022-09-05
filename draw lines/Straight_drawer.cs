using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Straight_drawer : MonoBehaviour
{
    public LineRenderer StraightRenderer;

    // Start is called before the first frame update
    void Start()
    {
        Draw_straight(100,100);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void Draw_straight(int steps, float length)
    {
        StraightRenderer.positionCount = steps;

        for(int currentStep = 0; currentStep<steps; currentStep++)
        {

            float progress = (float)steps/currentStep;
            float x = 2.0f;
            float z =  length/progress;

            Vector3 currentPosition = new Vector3(x,0,z);

            StraightRenderer.SetPosition(currentStep, currentPosition);
        }
    }
}
