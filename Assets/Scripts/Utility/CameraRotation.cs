using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UIElements;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook freeLookCam;
    [SerializeField] private Transform cameraLookAt;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float scrollSpeed = 10f;

    private Camera cam;
    private Vector3 previousPosition;

    private void Start()
    {
        cam = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        RotateCamera();
    }

    public void RotateCamera()
    {
        if (Input.GetMouseButtonDown(2))
        {
           previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 mousePos = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

            freeLookCam.m_XAxis.Value -= mousePos.x * 180;
            freeLookCam.m_YAxis.Value += mousePos.y;

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if(Mathf.Abs(Input.mouseScrollDelta.y) > 0)
        {
            for(int i = 0; i < freeLookCam.m_Orbits.Length; i++)
            {
                freeLookCam.m_Orbits[i].m_Radius -= Input.mouseScrollDelta.y * scrollSpeed * (1 + i/2);
                freeLookCam.m_Orbits[i].m_Height -= Input.mouseScrollDelta.y * scrollSpeed * (1 - i/2);
            }
        }

    }

    private void FixedUpdate()
    {
        float pVerticalInput = Input.GetAxis("Vertical");
        float pHorizontalInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(pVerticalInput) + Mathf.Abs(pHorizontalInput) > 0)
        {

            Vector3 forward = cam.transform.forward;
            Vector3 right = cam.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();


            Vector3 forwardRelIn = pVerticalInput * forward;
            Vector3 rightRelIn = pHorizontalInput * right;

            Vector3 cameraRelIn = forwardRelIn + rightRelIn;

            cameraLookAt.Translate(cameraRelIn * moveSpeed, Space.World);
        }
    }

}
