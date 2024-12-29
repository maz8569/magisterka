using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 ToVector3(this Vector3Int vector)
    {
        return (Vector3)(vector);
    }
}
