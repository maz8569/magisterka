using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGenerator))]
[CanEditMultipleObjects]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelGenerator levelGenerator = (LevelGenerator)target;

        if (GUILayout.Button("Show level"))
        {
            levelGenerator.StopExecution();
            levelGenerator.VisualizeMap();
        }

        if (GUILayout.Button("Check Connect"))
        {
            levelGenerator.CheckConnect();
        }
    }
}
