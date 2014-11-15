using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ActionSet : IEnumerable<IActionModule>
{
    List<IActionModule> Modules = new List<IActionModule>();

    public static ActionSet Parse( string ModuleStr, Skill ownerSkill )
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
                res.Modules.Add( new AttackModule( int.Parse( parameters[1] ), int.Parse( parameters[2] ), int.Parse( parameters[3] ), true,
                    (TargetType)Enum.Parse( typeof( TargetType ), parameters[4] ), (parameters.Length > 5 ? int.Parse( parameters[5] ) : -1) ) );
            }
            else if( str.StartsWith( "Magic " ) )
            {
                res.Modules.Add( new AttackModule( int.Parse( parameters[1] ), int.Parse( parameters[2] ), int.Parse( parameters[3] ), false,
                    (TargetType)Enum.Parse( typeof( TargetType ), parameters[4] ), (parameters.Length > 5 ? int.Parse( parameters[5] ) : -1) ) );
            }
            /*
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
            */
			else if ( str.StartsWith( "Heal " ) )
			{
				res.Modules.Add( new HealModule( int.Parse( parameters[1] ),
                    parameters.Length > 2 ? (TargetType)Enum.Parse( typeof( TargetType ), parameters[2] ) : TargetType.Self ) );
            }
            else if( str.StartsWith( "Enhance " ) )
            {
                res.Modules.Add( new EnhanceModule( (EnhanceParamType)Enum.Parse( typeof( EnhanceParamType ), parameters[1] ), int.Parse( parameters[2] ), int.Parse( parameters[3] ),
                    parameters.Length > 4 ? (TargetType)Enum.Parse( typeof( TargetType ), parameters[4] ) : TargetType.Player ) );
            }
            else if( str.StartsWith( "Weather " ) )
            {
                res.Modules.Add( new WeatherModule( parameters[1], int.Parse( parameters[2] ) ) );
            }
            else if( str.StartsWith( "Enemy " ) )
            {
                res.Modules.Add( new EnemySpawnModule( ownerSkill._prefabs.Find( (GameObject obj)=> obj.name == parameters[1] ), parameters[2] ) );
            }
			else if( str.StartsWith( "Wait " ) )
			{
				res.Modules.Add(new WaitModule());
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

