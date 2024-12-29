using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public enum DimensionHandle
{
    X,
    Z
}

public class SizeHandles : MonoBehaviour
{
    private bool isActive = false;
    private Ray ray;
    private RaycastHit hit;

    public DimensionHandle handle;
    public Transform otherHandle;
    public GameObject generatorObject;
    public ILevelGenerator generator;

    private void Start()
    {
        generator = generatorObject.GetComponent<ILevelGenerator>();
    }

    public void StartListen()
    {
        isActive = true;
    }

    public void StopListen() { isActive = false; }

    private void Update()
    {
        if (isActive)
        {
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 100f))
                {
                    var mousePos = hit.point;
                    if (handle == DimensionHandle.X && (int)mousePos.x % 2 == 0 && (int)mousePos.x >= 8 && (int)mousePos.x <= 22)
                    {
                        transform.position = new Vector3((int)mousePos.x, transform.position.y, transform.position.z);
                        generator.SetXSize(((int)mousePos.x) / 2 + 1);
                    }

                    if (handle == DimensionHandle.Z && (int)mousePos.z % 2 == 0 && (int)mousePos.z >= 8 && (int)mousePos.z <= 22)
                    {
                        transform.position = new Vector3(transform.position.x, transform.position.y, (int)mousePos.z);
                        generator.SetZSize(((int)mousePos.z) / 2 + 1);
                    }
                }
            }
        }
        else
        {
            if (handle == DimensionHandle.X) transform.position = new Vector3(transform.position.x, transform.position.y, otherHandle.position.z / 2);
            if (handle == DimensionHandle.Z) transform.position = new Vector3(otherHandle.position.x / 2, transform.position.y, transform.position.z);
        }
    }
}
