using System.Collections.Generic;
using UnityEngine;

public class PrototypeEditorData
{
    public readonly Prototype prototype;

    private readonly Prototype[] prototypes;

    private readonly Dictionary<Prototype, Mesh> meshes;

    public struct ConnectorHint
    {
        public readonly Mesh Mesh;
        public readonly int Rotation;

        public ConnectorHint(int rotation, Mesh mesh)
        {
            this.Rotation = rotation;
            this.Mesh = mesh;
        }
    }

    public PrototypeEditorData(Prototype prototype)
    {
        this.prototype = prototype;
        this.prototypes = prototype.transform.parent.GetComponentsInChildren<Prototype>();
        this.meshes = new Dictionary<Prototype, Mesh>();
    }

    private Mesh getMesh(Prototype prototype)
    {
        if (this.meshes.ContainsKey(prototype))
        {
            return this.meshes[prototype];
        }
        var mesh = prototype.GetMesh(false);
        this.meshes[prototype] = mesh;
        return mesh;
    }

    public ConnectorHint GetConnectorHint(int direction)
    {
        var face = this.prototype.Faces[direction];
        if (face is Prototype.HorizontalFaceDetails)
        {
            var horizontalFace = face as Prototype.HorizontalFaceDetails;

            foreach (var prototype in this.prototypes)
            {
                if (prototype == this.prototype)
                {
                    continue;
                }
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    var otherFace = prototype.Faces[Orientations.Rotate(direction, rotation + 2)] as Prototype.HorizontalFaceDetails;
                    if (otherFace.Connector == face.Connector && ((horizontalFace.Symmetric && otherFace.Symmetric) || otherFace.Flipped != horizontalFace.Flipped))
                    {
                        return new ConnectorHint(rotation, this.getMesh(prototype));
                    }
                }
            }
        }

        if (face is Prototype.VerticalFaceDetails)
        {
            var verticalFace = face as Prototype.VerticalFaceDetails;

            foreach (var prototype in this.prototypes)
            {
                if (prototype == this.prototype)
                {
                    continue;
                }
                var otherFace = prototype.Faces[(direction + 3) % 6] as Prototype.VerticalFaceDetails;
                if (otherFace.Connector != face.Connector)
                {
                    continue;
                }

                return new ConnectorHint(verticalFace.Rotation - otherFace.Rotation, this.getMesh(prototype));
            }
        }

        return new ConnectorHint();
    }
}
