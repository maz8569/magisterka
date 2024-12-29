using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot
{
    public Vector3Int position {  get; private set; }
    public Module[] modules;
    public bool collapsed;

    public Slot(Vector3Int position, Module[] modules, bool collapsed)
    {
        this.position = position;
        this.modules = modules;
        this.collapsed = collapsed;
    }
}
