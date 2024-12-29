using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathfindingAgent))]
[CanEditMultipleObjects]
public class PathfindingAgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PathfindingAgent agent = (PathfindingAgent)target;

        //if (GUILayout.Button("Start"))
        //{
           // agent.BeginMove();
        //}
    }
}
