using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum EStrategy
{
    None,
	Attack,
	Magic,
    Pilgrim,
	Vox,
	Count
}

public class Strategy : MonoBehaviour, IVoxNode
{
    public int Exp;
    public List<PlayerCommand> Commands;
    public List<MonoBehaviour> links;
    public float radius = 1.0f;
    public float Radius()
    {
        return radius;
    }
    public Transform Transform()
    {
        return transform;
    }
    public IEnumerable<IVoxNode> LinkedNodes()
    {
        return links.ConvertAll<IVoxNode>( ( MonoBehaviour mb ) => mb as IVoxNode );
    }
    public bool IsLinkedTo( IVoxNode node )
    {
        return LinkedNodes().Contains<IVoxNode>( node );
    }

	public EStrategy StrategyName { get; protected set; }


    void Awake()
    {
        this.StrategyName = (EStrategy)Enum.Parse( typeof( EStrategy ), name.Remove( name.IndexOf( "Strategy" ) ) );
        Vector3 sumPosition = Vector3.zero;
        foreach( PlayerCommand c in Commands )
        {
            c.SetParent( this );
            sumPosition += c.transform.localPosition;
        }
        transform.localPosition = sumPosition / Commands.Count;
        transform.localRotation = Quaternion.LookRotation( -transform.localPosition );
    }

    public IEnumerable<PlayerCommand> LinkedCommands
    {
        get
        {
            foreach( PlayerCommand c in Commands )
            {
                yield return c;
            }
            foreach( IVoxNode link in links )
            {
                Strategy linkedStrategy = link as Strategy;
                PlayerCommand linkedCommand = link as PlayerCommand;
                if( linkedStrategy != null )
                {
                    foreach( PlayerCommand c in linkedStrategy.Commands )
                    {
                        yield return c;
                    }
                }
                else if( linkedCommand != null )
                {
                    yield return linkedCommand;
                }
            }
        }
    }
}

