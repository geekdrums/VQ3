using System;
using System.Collections.Generic;

public enum EStrategy
{
	Attack,
	Magic,
	Count
}

public class Strategy
{
	public List<ECommand> UsableCommands { get; private set; }
	public ECommand[] DefaultCommands { get; private set; }

	public Strategy( params ECommand[] UsableCommands )
	{
		this.UsableCommands = new List<ECommand>();
		this.UsableCommands.AddRange( UsableCommands );
		this.DefaultCommands = new ECommand[4];
		for ( int i=0; i<4; i++ )
		{
			DefaultCommands[i] = UsableCommands[0];
		}
	}

	public bool IsUsable( ECommand Command )
	{
		return UsableCommands.Contains( Command );
	}
}

