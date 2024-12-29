using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

public static class CheckConnectivity
{
    public static float CheckIsAll(int gridX, int gridY, int2 startPosition, int2[] unwalkable)
    {
        int numberOfWalkable = gridX * gridY - unwalkable.Length;
        int2 gridSize = new int2(gridX, gridY);

        NativeArray<ConnectivityNode> pathNodeArray = new NativeArray<ConnectivityNode>(gridSize.x * gridSize.y, Allocator.TempJob);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                ConnectivityNode pathNode = new ConnectivityNode
                {
                    x = x,
                    y = y,
                    index = CalculateIndex(x, y, gridSize.x),

                };
                pathNode.isWalkable = true;
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        for (int i = 0; i < unwalkable.Length; i++)
        {
            ConnectivityNode unwalkablePathNode = pathNodeArray[CalculateIndex(unwalkable[i].x, unwalkable[i].y, gridSize.x)];
            unwalkablePathNode.SetIsWalkable(false);
            pathNodeArray[CalculateIndex(unwalkable[i].x, unwalkable[i].y, gridSize.x)] = unwalkablePathNode;
        }

        NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.TempJob);
        neighbourOffsetArray[0] = new int2(-1, 0);
        neighbourOffsetArray[1] = new int2(1, 0);
        neighbourOffsetArray[2] = new int2(0, -1);
        neighbourOffsetArray[3] = new int2(0, 1);
        neighbourOffsetArray[4] = new int2(-1, -1);
        neighbourOffsetArray[5] = new int2(-1, 1);
        neighbourOffsetArray[6] = new int2(1, 1);
        neighbourOffsetArray[7] = new int2(1, 1);

        ConnectivityNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
        pathNodeArray[startNode.index] = startNode;

        NativeList<int> openList = new NativeList<int>(Allocator.TempJob);
        NativeList<int> closedList = new NativeList<int>(Allocator.TempJob);

        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            int currentNodeIndex = openList[0];
            ConnectivityNode currentNode = pathNodeArray[currentNodeIndex];

            // Remove current node from Open List
            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            closedList.Add(currentNodeIndex);

            for (int i = 0; i < neighbourOffsetArray.Length; i++)
            {
                int2 neighbourOffset = neighbourOffsetArray[i];
                int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                {
                    // Neighbour not valid position
                    continue;
                }

                int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                if (closedList.Contains(neighbourNodeIndex))
                {
                    // Already searched this node
                    continue;
                }

                ConnectivityNode neighbourNode = pathNodeArray[neighbourNodeIndex];

                if (!neighbourNode.isWalkable)
                {
                    // Isn't walkable
                    continue;
                }
                int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                if (neighbourNode.cameFromNodeIndex == -1)
                {
                    neighbourNode.cameFromNodeIndex = currentNodeIndex;
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;

                    if (!openList.Contains(neighbourNode.index))
                    {
                        openList.Add(neighbourNode.index);
                    }
                }

            }

        }

        float result = closedList.Length / (float)numberOfWalkable;


        pathNodeArray.Dispose();
        neighbourOffsetArray.Dispose();
        openList.Dispose();
        closedList.Dispose();
        return result;
    }

    private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
    {
        return
            gridPosition.x >= 0 &&
            gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x &&
            gridPosition.y < gridSize.y;
    }

    private static int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }

}

public struct ConnectivityNode
{
    public int x;
    public int y;

    public int index;

    public bool isWalkable;

    public int cameFromNodeIndex;

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }
}
