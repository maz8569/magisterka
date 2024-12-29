using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleData : MonoBehaviour
{
    public GameObject modulesParent;
    public GameObject modulesPrefab;

    public void CreateModules()
    {
        if (modulesParent.transform.childCount > 0)
        {
            for (var i = modulesParent.transform.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                DestroyImmediate(modulesParent.transform.GetChild(i).gameObject);
#else
                Destroy(modulesParent.transform.GetChild(i).gameObject); 
#endif
            }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            GameObject temp = Instantiate(modulesPrefab, child.position + new Vector3(0, 6, 0), child.GetChild(0).rotation, modulesParent.transform);
            temp.name = child.name;
            Prototype childPro = child.GetComponent<Prototype>();
            temp.GetComponent<MeshFilter>().mesh = childPro.GetMesh();
            temp.GetComponent<MeshRenderer>().materials = child.GetChild(0).GetComponent<MeshRenderer>().sharedMaterials;
            temp.GetComponent<Module>().isWalkable = childPro.isWalkable;
        }

        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Prototype childPro = transform.GetChild(i).GetComponent<Prototype>();
            Module firstModule = modulesParent.transform.GetChild(i).GetComponent<Module>();

            for (int j = i; j < transform.childCount; j++)
            {
                Prototype secondPro = transform.GetChild(j).GetComponent<Prototype>();
                Module secondModule = modulesParent.transform.GetChild(j).GetComponent<Module>();

                if (childPro.Forward.Connector == secondPro.Backward.Connector && ((childPro.Forward.Symmetric && secondPro.Backward.Symmetric) || secondPro.Backward.Flipped != childPro.Forward.Flipped))
                {
                    firstModule.forwardNeighbours.Add(secondModule);
                    if(!childPro.gameObject.Equals(secondPro.gameObject)) secondModule.backwardNeighbours.Add(firstModule);

                }
                if (childPro.Backward.Connector == secondPro.Forward.Connector && ((childPro.Backward.Symmetric && secondPro.Forward.Symmetric) || secondPro.Forward.Flipped != childPro.Backward.Flipped))
                {
                    firstModule.backwardNeighbours.Add(secondModule);
                    if (!childPro.gameObject.Equals(secondPro.gameObject)) secondModule.forwardNeighbours.Add(firstModule);
                }
                if (childPro.Right.Connector == secondPro.Left.Connector && ((childPro.Right.Symmetric && secondPro.Left.Symmetric) || secondPro.Left.Flipped != childPro.Right.Flipped))
                {
                    firstModule.rightNeighbours.Add(secondModule);
                    if (!childPro.gameObject.Equals(secondPro.gameObject)) secondModule.leftNeighbours.Add(firstModule);
                }
                if (childPro.Left.Connector == secondPro.Right.Connector && ((childPro.Left.Symmetric && secondPro.Right.Symmetric) || secondPro.Right.Flipped != childPro.Left.Flipped))
                {
                    firstModule.leftNeighbours.Add(secondModule);
                    if (!childPro.gameObject.Equals(secondPro.gameObject)) secondModule.rightNeighbours.Add(firstModule);
                }

                if (childPro.Up.Connector == secondPro.Down.Connector && childPro.Up.Rotation == secondPro.Down.Rotation)
                {
                    firstModule.upNeighbours.Add(secondModule);
                    if (!childPro.gameObject.Equals(secondPro.gameObject)) secondModule.downNeighbours.Add(firstModule);
                }

                if (childPro.Down.Connector == secondPro.Up.Connector && childPro.Down.Rotation == secondPro.Up.Rotation)
                {
                    firstModule.downNeighbours.Add(secondModule);
                    if (!childPro.gameObject.Equals(secondPro.gameObject)) secondModule.upNeighbours.Add(firstModule);
                }
            }

            if (firstModule.downNeighbours.Count == 0)
            {
                firstModule.downNeighbours.Add(modulesParent.transform.GetChild(0).GetComponent<Module>());
            }
        }
    }
}
