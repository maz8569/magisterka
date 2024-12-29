using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModuleData))]
public class ModuleDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModuleData moduleData = (ModuleData)target;

        int count = moduleData != null ? moduleData.modulesParent.transform.childCount : 0;
        GUILayout.Label(count + " Modules");

        EditorGUILayout.HelpBox("Create a transform that contains one child for each module prototype and save it as a prefab. Drag it into the Prototypes property above and click \"Create module data\".", MessageType.Info);

        if (GUILayout.Button("Create module data"))
        {
            moduleData.CreateModules();
        }
    }
}
