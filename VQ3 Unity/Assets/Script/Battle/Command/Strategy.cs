using UnityEngine;
using System;
using System.Collections.Generic;

public enum EStrategy
{
	Attack,
	Magic,
	Break,
	Count
}

public class Strategy : MonoBehaviour
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
            c.SetExp( Exp );
        }
    }
    /*
		CommandList = new List<ECommand[]>();
		foreach ( string str in CommandStr )
		{
			CommandList.Add( Parse( str ) );
		}
	}

	public static ECommand[] Parse( string CommandStr )
	{
		ECommand[] res = new ECommand[4];
		string[] commandStrings = CommandStr.Split( Utils.space );
		for( int i=0;i<commandStrings.Length; ++i )
		{
			res[i] = (ECommand)Enum.Parse( typeof( ECommand ), commandStrings[i] );
		}
		return res;
	}
    */
}

