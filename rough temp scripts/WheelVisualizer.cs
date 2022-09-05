using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelVisualizer : MonoBehaviour
{
    public WheelCollider wr, wl;
    public GameObject rightMesh, leftMesh;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        WheelAnimation();
    }

    void WheelAnimation()
    {
        Quaternion wheelRot = Quaternion.identity;
        Vector3 wheelPos = Vector3.zero;
        Quaternion dotGain = Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));

        wr.GetWorldPose(out wheelPos, out wheelRot);
        //rightMesh.transform.Rotate(0.0f, wheelRot.eulerAngles.x, 0.0f);
        rightMesh.transform.rotation = wheelRot * dotGain;

        wl.GetWorldPose(out wheelPos, out wheelRot);
        //leftMesh.transform.Rotate(0.0f, wheelRot.eulerAngles.x, 0.0f);
        leftMesh.transform.rotation = wheelRot * dotGain;
    }
}
