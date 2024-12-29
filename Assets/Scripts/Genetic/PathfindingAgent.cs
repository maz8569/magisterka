using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PathfindingAgent : MonoBehaviour
{
    [SerializeField] private GameObject LevelGenerator;
    [Header("Movement parameters")]
    [SerializeField]
    private float moveSpeed = 12f;
    private Vector3 targetPosition;
    private Vector3 startPosition;

    public bool isMoving = false;
    private List<Vector3> positions;
    public int currentPos;

    private UnityAction<object> moveTo;

    private void OnEnable()
    {
        EventManager.StartListening("MoveTo", moveTo);
    }

    private void OnDisable()
    {
        EventManager.StopListening("MoveTo", moveTo);
    }

    private void Awake()
    {
        moveTo = new UnityAction<object>(MoveTo);
    }

    public void MoveTo(object data)
    {
        int2 start;
        if (positions?.Count > 0) start = new int2((int)(positions[positions.Count - 1].x / 2), (int)(positions[positions.Count - 1].z / 2));
        else start = new int2((int)(transform.position.x / 2), (int)(transform.position.z / 2));
        int2 end = (int2)data;
        positions = LevelGenerator.GetComponent<ILevelGenerator>().FindPath(start, end);
        if(positions.Count > 0 )
        {
            foreach( var position in positions)
            {
                Debug.Log(position);
            }
            currentPos = 0;
            startPosition = transform.localPosition;
            targetPosition = positions[currentPos];
            isMoving = true;
        }
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            if (Vector3.Distance(targetPosition, transform.localPosition) < 0.1f)
            {
                transform.localPosition = targetPosition;
                startPosition = targetPosition;
                currentPos++;
                if (currentPos == positions.Count)
                {
                    currentPos = 0;
                    isMoving = false;
                }
                targetPosition = positions[currentPos];
            }

            transform.localPosition += moveSpeed * Time.fixedDeltaTime * (targetPosition - startPosition).normalized;
        }

    }
}
