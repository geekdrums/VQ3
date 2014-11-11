using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IVoxNode
{
    float Radius();
    Transform Transform();
    IEnumerable<IVoxNode> LinkedNodes();
    void OnEdgeCreated( LineRenderer edge );
}