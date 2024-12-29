using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Prototype : MonoBehaviour
{
    [System.Serializable]
    public abstract class FaceDetails
    {
        public int Connector;

        public virtual void ResetConnector()
        {
            Connector = 0;
        }
    }

    [System.Serializable]
    public class HorizontalFaceDetails : FaceDetails
    {
        public bool Symmetric;
        public bool Flipped;

        public override string ToString()
        {
            return Connector.ToString() + (Symmetric ? "s" : (Flipped ? "F" : ""));
        }

        public override void ResetConnector()
        {
            base.ResetConnector();
            Symmetric = false;
            Flipped = false;
        }
    }

    [System.Serializable]
    public class VerticalFaceDetails : FaceDetails
    {
        public bool Invariant;
        public int Rotation;

        public override string ToString()
        {
            return Connector.ToString() + (Invariant ? "i" : (Rotation != 0 ? "_bcd".ElementAt(Rotation).ToString() : ""));
        }

        public override void ResetConnector()
        {
            base.ResetConnector();
            Invariant = false;
            Rotation = 0;
        }
    }

    public FaceDetails[] Faces
    {
        get
        {
            return new FaceDetails[] {
                Left,
                Down,
                Backward,
                Right,
                Up,
                Forward
            };
        }
    }

    public Mesh GetMesh(bool createEmptyFallbackMesh = true)
    {
        var meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            return meshFilter.sharedMesh;
        }
        if (createEmptyFallbackMesh)
        {
            var mesh = new Mesh();
            return mesh;
        }
        return null;
    }

    public float Probability = 1.0f;
    public bool isWalkable = true;
    public HorizontalFaceDetails Left;
    public VerticalFaceDetails Down;
    public HorizontalFaceDetails Backward;
    public HorizontalFaceDetails Right;
    public VerticalFaceDetails Up;
    public HorizontalFaceDetails Forward;

#if UNITY_EDITOR
    private static PrototypeEditorData editorData;
    private static GUIStyle style;

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void DrawGizmo(Prototype modulePrototype, GizmoType gizmoType)
    {
        var transform = modulePrototype.transform;
        Vector3 position = transform.position;
        var rotation = transform.rotation;

        if (editorData == null || editorData.prototype != modulePrototype)
        {
            editorData = new PrototypeEditorData(modulePrototype);
        }

        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        if ((gizmoType & GizmoType.Selected) != 0)
        {
            for (int i = 0; i < 6; i++)
            {
                var hint = editorData.GetConnectorHint(i);
                if (hint.Mesh != null)
                {
                    Gizmos.DrawMesh(hint.Mesh,
                        position + rotation * Orientations.Direction[i].ToVector3() * AbstractMap.BLOCK_SIZE,
                        rotation * Quaternion.Euler(Vector3.up * 90f * hint.Rotation));
                }
            }
        }
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position + Vector3.down * 0.1f, position + rotation * Orientations.Rotations[3] * Vector3.forward * AbstractMap.BLOCK_SIZE * 0.5f + Vector3.down * 0.1f);
        /*for (int i = 0; i < 6; i++)
        {
            if (modulePrototype.Faces[i].Walkable)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(position + Vector3.down * 0.1f, position + rotation * Orientations.Rotations[i] * Vector3.forward * AbstractMap.BLOCK_SIZE * 0.5f + Vector3.down * 0.1f);
            }
        }*/

        if (style == null)
        {
            style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
        }

        style.normal.textColor = Color.black;
        for (int i = 0; i < 6; i++)
        {
            var face = modulePrototype.Faces[i];
            Handles.Label(position + rotation * Orientations.Rotations[i] * Vector3.forward * AbstractMap.BLOCK_SIZE / 2f, face.ToString(), style);
        }
    }

    public void GenerateRotations()
    {
        HorizontalFaceDetails tempL = Left;
        HorizontalFaceDetails tempB = Backward;
        HorizontalFaceDetails tempR = Right;
        HorizontalFaceDetails tempF = Forward;
        for(int i = 1; i < 4; i++)
        {
            GameObject temp = Instantiate(gameObject, transform.position + new Vector3(4 * i, 0, 0), transform.rotation, transform.parent);
            temp.transform.GetChild(0).rotation = Quaternion.Euler(0, 90 * i, 0);
            temp.name = gameObject.name + "_" + i.ToString();
            Prototype tempPro = temp.GetComponent<Prototype>();
            tempPro.Left = tempB;
            tempPro.Right = tempF;
            tempPro.Forward = tempL;
            tempPro.Backward = tempR;

            tempL = tempPro.Left;
            tempR = tempPro.Right;
            tempB = tempPro.Backward;
            tempF = tempPro.Forward;

            if (!tempPro.Down.Invariant)
            {
                tempPro.Down.Rotation = (tempPro.Down.Rotation + i) % 4;
            }

            if (!tempPro.Up.Invariant)
            {
                tempPro.Up.Rotation = (tempPro.Up.Rotation + i) % 4;
            }
        }
        name += "_0";
    }
#endif

    public void Reset()
    {
        Left = new HorizontalFaceDetails();
        Up = new VerticalFaceDetails();
        Right = new HorizontalFaceDetails();
        Backward = new HorizontalFaceDetails();
        Down = new VerticalFaceDetails();
        Forward = new HorizontalFaceDetails();
    }
}
