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
			switch( parameters[0] )
			{
			case "Anim":
				res.Modules.Add(new AnimModule((TargetType)Enum.Parse(typeof(TargetType), parameters[1]), (parameters.Length > 2 ? parameters[2] : "")));
				break;
			case "Attack":
			case "Magic":
			case "Dain":
			case "Lacl":
			case "Aura":
			case "Igna":
			case "Vox":
				res.Modules.Add(new AttackModule(
					power: int.Parse(parameters[1]), vp: int.Parse(parameters[2]), vt: int.Parse(parameters[3]),
					type: (AttackType)Enum.Parse(typeof(AttackType), parameters[0]),
					target: (TargetType)Enum.Parse(typeof(TargetType), parameters[4]),
					animIndex: (parameters.Length > 5 ? int.Parse(parameters[5]) : -1)));
				break;
			case "Heal":
				res.Modules.Add( new HealModule( int.Parse( parameters[1] ),
                    parameters.Length > 2 ? (TargetType)Enum.Parse( typeof( TargetType ), parameters[2] ) : TargetType.Self ) );
				break;
			case "Enhance":
                res.Modules.Add( new EnhanceModule( (EnhanceParamType)Enum.Parse( typeof( EnhanceParamType ), parameters[1] ), int.Parse( parameters[2] ), int.Parse( parameters[3] ),
                    parameters.Length > 4 ? (TargetType)Enum.Parse( typeof( TargetType ), parameters[4] ) : TargetType.Player ) );
				break;
			case "Drain":
				res.Modules.Add(new DrainModule(float.Parse(parameters[1]), int.Parse(parameters[2]),
					parameters.Length > 3 ? (TargetType)Enum.Parse(typeof(TargetType), parameters[3]) : TargetType.Self));
				break;
			case "Weather":
                res.Modules.Add( new WeatherModule( parameters[1], int.Parse( parameters[2] ) ) );
				break;
			case "Enemy":
                res.Modules.Add( new EnemySpawnModule( ownerSkill._prefabs.Find( (GameObject obj)=> obj.name == parameters[1] ), parameters[2] ) );
				break;
			case "Wait":
				res.Modules.Add(new WaitModule());
				break;
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

