using UnityEngine;
using System;
using System.Collections.Generic;

public enum EStrategy
{
	Attack,
	Magic,
    Pilgrim,
	Break,
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
        foreach( Command c in Commands )
        {
            c.Parse();
            c.SetParent( this );
        }
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

