using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Prototype))]
public class PrototypeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Prototype modulePrototype = (Prototype)target;
        if (GUILayout.Button("Distribute"))
        {
            int i = 0;
            foreach (Transform transform in modulePrototype.transform.parent)
            {
                transform.localPosition = Vector3.forward * i * AbstractMap.BLOCK_SIZE * 2f;
                i++;
            }
        }

        if (GUILayout.Button("Distribute (Overview)"))
        {
            int w = Mathf.FloorToInt(Mathf.Sqrt(modulePrototype.transform.parent.childCount));
            int i = 0;
            foreach (Transform transform in modulePrototype.transform.parent)
            {
                transform.localPosition = (i / w) * 1.4f * AbstractMap.BLOCK_SIZE * Vector3.forward + Vector3.right * (i % w) * AbstractMap.BLOCK_SIZE * 1.4f;
                i++;
            }
        }

        if (GUILayout.Button("Reset connectors"))
        {
            foreach (var face in modulePrototype.Faces)
            {
                face.ResetConnector();
            }
        }

        if (GUILayout.Button("Generate Rotations"))
        {
            modulePrototype.GenerateRotations();
        }
    }
}
