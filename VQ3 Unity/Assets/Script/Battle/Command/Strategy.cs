using UnityEngine;
using System;
using System.Collections.Generic;

public enum EStrategy
{
    None,
	Attack,
	Magic,
    Pilgrim,
	Vox,
	Count
}

public class Strategy : MonoNode
{
    public int Exp;
    public List<Command> Commands;

	public EStrategy StrategyName { get; protected set; }

    void Awake()
    {
        this.StrategyName = (EStrategy)Enum.Parse( typeof( EStrategy ), name.Remove( name.IndexOf( "Strategy" ) ) );
        Vector3 sumPosition = Vector3.zero;
        foreach( Command c in Commands )
        {
            c.SetParent( this );
            sumPosition += c.transform.localPosition;
        }
        transform.localPosition = sumPosition / Commands.Count;
        transform.localRotation = Quaternion.LookRotation( -transform.localPosition );
    }

    public IEnumerable<Command> LinkedCommands
    {
        get
        {
            foreach( Command c in Commands )
            {
                yield return c;
            }
            foreach( MonoNode link in links )
            {
                Strategy linkedStrategy = link as Strategy;
                Command linkedCommand = link as Command;
                if( linkedStrategy != null )
                {
                    foreach( Command c in linkedStrategy.Commands )
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

