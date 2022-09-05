using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderDWA : MonoBehaviour
{
    private LineRenderer line;
    private Vector3 mousePos;
    private Vector3 startPos;
    private Vector3 endPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (line == null)
                createLine();
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            line.SetPosition(0, mousePos);
            line.SetPosition(1, mousePos);
            startPos = mousePos;
        }
        else if (Input.GetMouseButtonUp(0) && line)
        {
            if (line)
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                line.SetPosition(1, mousePos);
            }
        }
    }

    private void createLine()
    {
        line = new GameObject("VisualDWA").AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Diffuse"));
        line.positionCount = 2;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.useWorldSpace = true;
    }
    
    /*private void addColliderToLine()
    {

    }*/
}
