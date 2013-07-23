using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyProperty
{
	public int HP;
	public string Name;
	public Texture Texture;
	public TransformProperty Transform;

	[System.Serializable]
	public class TransformProperty
	{
		public Vector3 Position;
		public float Scale = 1;
	}
}

public class EnemyConductor : MonoBehaviour {

	public GameObject EnemyOriginal;

	public EnemyProperty[] EnemyProperties;

	List<Enemy> Enemies;

	// Use this for initialization
	void Start () {
		GameContext.EnemyConductor = this;
		Enemies = new List<Enemy>();
		Enemy tempEnemy;
		foreach ( EnemyProperty ep in EnemyProperties )
		{
			tempEnemy = ( (GameObject)Instantiate( EnemyOriginal, new Vector3(), EnemyOriginal.transform.rotation ) ).GetComponent<Enemy>();
			tempEnemy.Initialize( ep );
			Enemies.Add( tempEnemy );
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ReceiveAction( ActionSet Action, bool isPlayerAction )
	{
		AttackModule attack = Action.GetModule<AttackModule>();
		if ( attack != null && isPlayerAction )
		{
			foreach ( Enemy e in GetTargetEnemies( attack.TargetType ) )
			{
				e.BeAttacked( attack );
			}
		}
		MagicModule magic = Action.GetModule<MagicModule>();
		if ( magic != null && isPlayerAction )
		{
			foreach ( Enemy e in GetTargetEnemies( magic.TargetType ) )
			{
				e.BeMagicAttacked( magic );
			}
		}


		Enemies.RemoveAll( ( Enemy e ) => e.HitPoint<=0 );
		if ( Enemies.Count == 0 )
		{
			GameContext.BattleConductor.OnPlayerWin();
		}
	}


	List<Enemy> GetTargetEnemies( TargetType TargetType )
	{
		List<Enemy> Res;
		Res = new List<Enemy>();
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
}
