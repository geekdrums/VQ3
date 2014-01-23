using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MonoNode : MonoBehaviour
{
    public List<MonoNode> links;
    public float radius = 1.0f;

    public IEnumerable<MonoNode> LinkedNodes
    {
        get
        {
            return links;
        }
    }

    public bool IsLinkedTo( MonoNode node )
    {
        return LinkedNodes.Contains<MonoNode>( node );
    }
}