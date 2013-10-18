using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyConductor : MonoBehaviour {
    
    List<Enemy> Enemies = new List<Enemy>();

	// Use this for initialization
	void Start () {
		GameContext.EnemyConductor = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetEnemy( params GameObject[] NewEnemies )
    {
        Enemies.Clear();
        GameObject TempObj;
        for( int i=0; i<NewEnemies.Length; ++i )
        {
            TempObj = (GameObject)Instantiate( NewEnemies[i], new Vector3( 10 * (-(NewEnemies.Length - 1)/ 2.0f + i), 5, 20 ), NewEnemies[i].transform.rotation );
            Enemies.Add( TempObj.GetComponent<Enemy>() );
        }
    }

	public bool ReceiveAction( ActionSet Action, Command command )
	{
		bool isSucceeded = false;
		AttackModule attack = Action.GetModule<AttackModule>();
        if( attack != null && command.isPlayerAction )
		{
			foreach ( Enemy e in GetTargetEnemies( attack.TargetType ) )
            {
                if( command.isLocal ) command.transform.position = e.transform.position;
				e.BeAttacked( attack, command );
				isSucceeded = true;
			}
		}
		MagicModule magic = Action.GetModule<MagicModule>();
        if( magic != null && command.isPlayerAction )
		{
			foreach ( Enemy e in GetTargetEnemies( magic.TargetType ) )
            {
                if( command.isLocal ) command.transform.position = e.transform.position;
				e.BeMagicAttacked( magic, command );
				isSucceeded = true;
			}
		}

		Enemies.RemoveAll( ( Enemy e ) => e.HitPoint<=0 );
		if ( Enemies.Count == 0 )
		{
			GameContext.BattleConductor.OnPlayerWin();
		}

		return isSucceeded;
	}

	List<Enemy> GetTargetEnemies( TargetType TargetType )
	{
		List<Enemy> Res = new List<Enemy>();
		if ( Enemies.Count == 0 ) return Res;
		switch ( TargetType )
		{
		case TargetType.First:
			Res.Add( Enemies[0] );
			break;
		case TargetType.Random:
			Res.Add( Enemies[Random.Range( 0, Enemies.Count )] );
			break;
		case TargetType.All:
			foreach ( Enemy e in Enemies )
			{
				Res.Add( e );
			}
			break;
		}
		return Res;
	}

    public void OnBarStarted(int CurrentIndex)
    {
        if (CurrentIndex < Enemies.Count)
        {
            Command commandPrefab = Enemies[CurrentIndex].GetExecCommand();
            Command NewCommand = (Command)Instantiate(commandPrefab, new Vector3(), commandPrefab.transform.rotation);
            NewCommand.SetOwner(Enemies[CurrentIndex]);
            GameContext.BattleConductor.ExecCommand(NewCommand);
        }
    }
}
