using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyConductor : MonoBehaviour {
    
    List<Enemy> Enemies = new List<Enemy>();

	Color _baseColor;
	public Color baseColor
	{
		get { return _baseColor; }
		set
		{
			_baseColor = value;
			foreach ( Enemy e in Enemies )
			{
                e.OnBaseColorChanged( value );
			}
		}
	}
    public int baseHP { get; protected set; }

	// Use this for initialization
	void Start () {
		GameContext.EnemyConductor = this;
		baseColor = Color.black;
        baseHP = 100;//temp
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
            TempObj = (GameObject)Instantiate( NewEnemies[i], new Vector3( 7 * (-(NewEnemies.Length - 1)/ 2.0f + i), 0, 0 ), NewEnemies[i].transform.rotation );
			TempObj.renderer.material.color = baseColor;
            Enemies.Add( TempObj.GetComponent<Enemy>() );
        }
    }

	public bool ReceiveAction( ActionSet Action, Skill skill )
	{
        if( !skill.isPlayerSkill ) skill.OwnerCharacter.SkillInit();

		bool isSucceeded = false;
        AnimModule anim = Action.GetModule<AnimModule>();
        if( anim != null && skill.isPlayerSkill )
        {
            foreach( Enemy e in GetTargetEnemies( anim, skill ) )
            {
                isSucceeded = true;
                if( anim.IsLocal )
                {
                    anim.SetTargetEnemy( e );
                    skill.transform.position = e.transform.position + Vector3.back * 0.1f;
                    break;
                }
            }
        }
		AttackModule attack = Action.GetModule<AttackModule>();
        if( attack != null && skill.isPlayerSkill )
		{
            foreach( Enemy e in GetTargetEnemies( attack, skill ) )
            {
				e.BeAttacked( attack, skill );
				isSucceeded = true;
			}
		}
		MagicModule magic = Action.GetModule<MagicModule>();
        if( magic != null && skill.isPlayerSkill )
		{
            foreach( Enemy e in GetTargetEnemies( magic, skill ) )
            {
				e.BeMagicAttacked( magic, skill );
				isSucceeded = true;
			}
			GameContext.VoxonSystem.AddVoxon( magic.VoxonPoint );
		}

		Enemies.RemoveAll( ( Enemy e ) => e.HitPoint<=0 );
		if ( Enemies.Count == 0 )
		{
			GameContext.BattleConductor.OnPlayerWin();
		}

		return isSucceeded;
	}

    List<Enemy> GetTargetEnemies( TargetModule Target, Skill skill )
	{
		List<Enemy> Res = new List<Enemy>();
		if ( Enemies.Count == 0 ) return Res;
        switch( Target.TargetType )
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
        case TargetType.Anim:
            Res.Add( skill.Actions[Target.AnimIndex].GetModule<AnimModule>().TargetEnemy );
            break;
		}
		return Res;
	}

    public void CheckSkill()
    {
        int CurrentIndex = Music.Just.bar;
        if( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.Break ) return;
        if( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.ShowBreak && CurrentIndex == 3 ) return;
        if( Music.IsJustChangedWhen( ( Timing t ) => t.barUnit == 8 ) && CurrentIndex < Enemies.Count )
        {
            Skill enemySkill = Enemies[CurrentIndex].GetCurrentSkill();
            Skill objSkill = (Skill)Instantiate( enemySkill, new Vector3(), enemySkill.transform.rotation );
            objSkill.SetOwner( Enemies[CurrentIndex] );
            GameContext.BattleConductor.ExecSkill( objSkill );
        }
    }
    /*
        GameObject playerSkill = CurrentCommand.GetCurrentSkill();
        if( playerSkill != null )
        {
            Skill objSkill = ( Instantiate( playerSkill ) as GameObject ).GetComponent<Skill>();
            objSkill.SetOwner( Player );
            GameContext.BattleConductor.ExecSkill( objSkill );
        }
    }
    */
}
