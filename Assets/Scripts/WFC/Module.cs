using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Module : MonoBehaviour
{
    public List<Module> rightNeighbours;
    public List<Module> leftNeighbours;
    public List<Module> forwardNeighbours;
    public List<Module> backwardNeighbours;
    public List<Module> upNeighbours;
    public List<Module> downNeighbours;
    public bool isWalkable;
    public float Probability = 1.0f;

    public List<Module> GetNeighboursFromDirection(Vector3Int direction)
    {
        if (direction.magnitude > 1) return new List<Module>();

        if (direction.x == 1) return rightNeighbours;
        if (direction.x == -1) return leftNeighbours;
        if (direction.z == 1) return forwardNeighbours;
        if (direction.z == -1) return backwardNeighbours;
        if (direction.y == 1) return upNeighbours;

        return downNeighbours;
    }

    public override string ToString()
    {
        return name;
    }
}
