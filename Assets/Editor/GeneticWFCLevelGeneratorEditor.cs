using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GeneticWFCLevelGenerator))]
[CanEditMultipleObjects]
public class GeneticWFCLevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GeneticWFCLevelGenerator geneticWFCLevelGenerator = (GeneticWFCLevelGenerator)target;

        if (GUILayout.Button("Novelty"))
        {
            Debug.Log(geneticWFCLevelGenerator.CheckNovelty(geneticWFCLevelGenerator.slots));
        }

#if UNITY_EDITOR

        if (GUILayout.Button("Regenerate Level"))
        {
            geneticWFCLevelGenerator.RegenerateLevel();
        }
#endif

    }
}
