using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public WheelCollider wr, wl;
    [SerializeField] private float power = 5.0f;
    public GameObject rightMesh, leftMesh;
    public Rigidbody rb;
    [SerializeField] private float verticalInput;

    // Start is called before the first frame update
    void Start()
    {
        rb.centerOfMass = new Vector3(0f, -1f, 0f);
    }

    private void FixedUpdate()
    {
        // differential wheel motor torque only
        wr.motorTorque = power*verticalInput; //right
        wl.motorTorque = power* verticalInput; //left

        // visual update
        WheelAnimation();
    }
    // Update is called once per frame
    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
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
