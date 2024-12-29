using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineContorl : MonoBehaviour
{
    public Transform xHandle;
    public Transform zHandle;
    private LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(1, new Vector3(0, 0, zHandle.position.z));
        lineRenderer.SetPosition(2, new Vector3(xHandle.position.x, 0, zHandle.position.z));
        lineRenderer.SetPosition(3, new Vector3(xHandle.position.x, 0, 0));
    }
}
