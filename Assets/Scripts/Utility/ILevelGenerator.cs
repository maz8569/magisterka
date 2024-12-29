using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public interface ILevelGenerator
{
    public List<Vector3> FindPath(int2 startPos, int2 endPos);
    void SetXSize(int x);
    void SetZSize(int z);
    void SetDesiredNovelty(float  desiredNovelty);
    void SetDesiredHeight(float desiredHeight);
}
