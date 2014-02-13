﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ActionSet : IEnumerable<IActionModule>
{
    List<IActionModule> Modules = new List<IActionModule>();

    public static ActionSet Parse( string ModuleStr )
    {
        ActionSet res = new ActionSet();
        string[] moduleStrings = ModuleStr.Split( Utils.comma );
        foreach( string str in moduleStrings )
        {
            string[] parameters = str.Split( Utils.space );
            if( str.StartsWith( "Anim " ) )
            {
                res.Modules.Add( new AnimModule( (TargetType)Enum.Parse( typeof( TargetType ), parameters[1] ), (parameters.Length > 2 ? parameters[2] : "") ) );
            }
			else if ( str.StartsWith( "Attack " ) )
            {
                res.Modules.Add( new AttackModule( int.Parse( parameters[1] ), (TargetType)Enum.Parse( typeof( TargetType ), parameters[2] ),
                    (parameters.Length > 3 ? int.Parse( parameters[3] ) : -1) ) );
            }
            else if( str.StartsWith( "Magic " ) )
            {
                res.Modules.Add( new MagicModule( int.Parse( parameters[1] ), int.Parse( parameters[2] ), (TargetType)Enum.Parse( typeof( TargetType ), parameters[3] ),
                    (parameters.Length > 4 ? int.Parse( parameters[4] ) : -1) ) );
            }
            else if( str.StartsWith( "Defend " ) )
            {
                res.Modules.Add( new DefendModule( int.Parse( parameters[1] ),
                    parameters.Length > 2 ? (TargetType)Enum.Parse( typeof( TargetType ), parameters[2] ) : TargetType.Self ) );
            }
            else if( str.StartsWith( "MagicDefend " ) )
            {
                res.Modules.Add( new MagicDefendModule( int.Parse( parameters[1] ),
                    parameters.Length > 2 ? (TargetType)Enum.Parse( typeof( TargetType ), parameters[2] ) : TargetType.Self ) );
            }
			else if ( str.StartsWith( "Heal " ) )
			{
				res.Modules.Add( new HealModule( int.Parse( parameters[1] ),
                    parameters.Length > 2 ? (TargetType)Enum.Parse( typeof( TargetType ), parameters[2] ) : TargetType.Self ) );
			}
        }
        return res;
    }
	public ActionSet( params IActionModule[] Modules )
	{
		this.Modules.AddRange( Modules );
	}

    public T GetModule<T>()
        where T : class, IActionModule
    {
        T ResModule;
        foreach( IActionModule module in Modules )
        {
            ResModule = (module as T);
            if( ResModule != null ) return ResModule;
        }
        return null;
    }
	public IEnumerator<IActionModule> GetEnumerator()
	{
		foreach ( IActionModule module in Modules ) yield return module;
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

