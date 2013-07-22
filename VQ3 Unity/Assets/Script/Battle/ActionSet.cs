using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionSet : IEnumerable<IActionModule>
{
	List<IActionModule> Modules;
	public T GetModule<T>()
		where T : class, IActionModule
	{
		T ResModule;
		foreach ( IActionModule module in Modules )
		{
			ResModule = ( module as T );
			if ( ResModule != null ) return ResModule;
		}
		return null;
	}

	public ActionSet( params IActionModule[] Modules )
	{
		this.Modules = new List<IActionModule>();
		this.Modules.AddRange( Modules );
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

