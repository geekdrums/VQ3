using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyConductor : MonoBehaviour {

    public GameObject damageTextPrefab;
    public GameObject HPCirclePrefab;
    public GameObject nextTargetCursor;

    List<Enemy> Enemies = new List<Enemy>();
    WeatherEnemy WeatherEnemy;

    Enemy targetEnemy;
    Enemy nextTarget;
    Camera mainCamera;

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
    public int baseHP { get { return GameContext.PlayerConductor.PlayerHP; } }

	// Use this for initialization
	void Start () {
		GameContext.EnemyConductor = this;
		baseColor = Color.black;
        mainCamera = GameObject.Find( "Main Camera" ).GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        if( Enemies.Count > 0 )
        {
            if( GameContext.PlayerConductor.commandGraph.NextCommand.IsTargetSelectable )
            {
                if( nextTarget == null ) nextTarget = targetEnemy;
                int targetIndex = Enemies.IndexOf( nextTarget );
                if( Input.GetKeyDown( KeyCode.LeftArrow ) )
                {
                    targetIndex = (targetIndex - 1 + Enemies.Count) % Enemies.Count;
                    nextTarget = Enemies[targetIndex];
                }
                else if( Input.GetKeyDown( KeyCode.RightArrow ) )
                {
                    targetIndex = (targetIndex + 1) % Enemies.Count;
                    nextTarget = Enemies[targetIndex];
                }
                nextTargetCursor.transform.position = Vector3.Lerp( nextTargetCursor.transform.position, nextTarget.transform.position + Vector3.up * 5 + Vector3.back, 0.1f );
            }
            else
            {
                nextTarget = null;
                nextTargetCursor.transform.position = Vector3.Lerp( nextTargetCursor.transform.position, Vector3.up * 10 + Vector3.back, 0.1f );
            }
        }
	}

    public void SetEnemy( params GameObject[] NewEnemies )
    {
        Enemies.Clear();
        GameObject TempObj;
        for( int i=0; i<NewEnemies.Length; ++i )
        {
            TempObj = (GameObject)Instantiate( NewEnemies[i], new Vector3( 7 * (-(NewEnemies.Length - 1)/ 2.0f + i), 6, 0 ), NewEnemies[i].transform.rotation );
			TempObj.renderer.material.color = baseColor;
            Enemy e = TempObj.GetComponent<Enemy>();
            WeatherEnemy we = TempObj.GetComponent<WeatherEnemy>();
            if( we != null )
            {
                WeatherEnemy = we;
            }
            else
            {
                Enemies.Add( e );
            }
            e.transform.parent = transform;
        }
        targetEnemy = Enemies[(Enemies.Count - 1)/2];
        GameContext.VoxSystem.SetTargetEnemy( targetEnemy );
    }

	public bool ReceiveAction( ActionSet Action, Skill skill )
	{
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
			GameContext.VoxSystem.AddVP( magic.VoxPoint );
        }
        DefendModule defend = Action.GetModule<DefendModule>();
        if( defend != null && !skill.isPlayerSkill )
        {
            foreach( Enemy e in GetTargetEnemies( defend, skill ) )
            {
                e.Defend( defend );
                isSucceeded = true;
            }
        }
        MagicDefendModule magicDefend = Action.GetModule<MagicDefendModule>();
        if( magicDefend != null && !skill.isPlayerSkill )
        {
            foreach( Enemy e in GetTargetEnemies( magicDefend, skill ) )
            {
                e.MagicDefend( magicDefend );
                isSucceeded = true;
            }
        }
        HealModule heal = Action.GetModule<HealModule>();
        if( heal != null && !skill.isPlayerSkill )
        {
            foreach( Enemy e in GetTargetEnemies( heal, skill ) )
            {
                e.Heal( heal );
                isSucceeded = true;
            }
        }

        WeatherModule weather = Action.GetModule<WeatherModule>();
        if( WeatherEnemy != null && weather != null )
        {
            bool IsOldSubstance = WeatherEnemy.IsSubstance;
            WeatherEnemy.ReceiveWeatherModule( weather );
            if( IsOldSubstance != WeatherEnemy.IsSubstance )
            {
                if( IsOldSubstance ) Enemies.Remove( WeatherEnemy );
                else Enemies.Add( WeatherEnemy );
            }
        }
        

		Enemies.RemoveAll( ( Enemy e ) => e.HitPoint<=0 );
        if( Enemies.Count == 0 )
        {
            GameContext.BattleConductor.OnPlayerWin();
            if( WeatherEnemy != null && !WeatherEnemy.IsSubstance )
            {
                Destroy( WeatherEnemy.gameObject );
                WeatherEnemy = null;
            }
        }
        else
        {
            if( nextTarget != null && !Enemies.Contains( nextTarget ) ) nextTarget = Enemies[(Enemies.Count - 1) / 2];
            if( !Enemies.Contains( targetEnemy ) )
            {
                targetEnemy = (nextTarget != null && Enemies.Contains( nextTarget ) ? nextTarget : Enemies[(Enemies.Count - 1) / 2]);
                GameContext.VoxSystem.SetTargetEnemy( targetEnemy );
            }
        }

		return isSucceeded;
	}

    List<Enemy> GetTargetEnemies( TargetModule Target, Skill skill )
	{
		List<Enemy> Res = new List<Enemy>();
		if ( Enemies.Count == 0 ) return Res;
        switch( Target.TargetType )
		{
        case TargetType.Select:
            Res.Add( targetEnemy );
            break;
        case TargetType.First:
        case TargetType.Second:
        case TargetType.Third:
            int index = (int)Target.TargetType - (int)TargetType.First;
            if( Enemies.Count > index ) Res.Add( Enemies[index] );
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
            Enemy animTarget = skill.Actions[Target.AnimIndex].GetModule<AnimModule>().TargetEnemy;
            if( Enemies.Contains( animTarget ) ) Res.Add( animTarget );
            break;
        case TargetType.Self:
            Res.Add( skill.OwnerCharacter as Enemy );
            break;
        case TargetType.Other:
            if( Enemies.Count == 1 ) Res.Add( skill.OwnerCharacter as Enemy );
            else
            {
                int rand = Random.Range( 0, Enemies.Count-1 );
                int selfIndex = Enemies.IndexOf( skill.OwnerCharacter as Enemy );
                Res.Add( Enemies[rand < selfIndex ? rand : rand + 1] );
            }
            break;
        case TargetType.Weakest:
            int minHP = 1000000;
            Enemy weakest = skill.OwnerCharacter as Enemy;
            foreach( Enemy enemy in Enemies )
            {
                if( enemy.HitPoint < minHP )
                {
                    weakest = enemy;
                    minHP = enemy.HitPoint;
                }
            }
            Res.Add( weakest );
            break;
		}
		return Res;
	}

    public void CheckCommand()
    {
        if( nextTarget != null )
        {
            targetEnemy = nextTarget;
            GameContext.VoxSystem.SetTargetEnemy( targetEnemy );
        }

        int num2BarCommands = 0;
        int num1BarCommands = 0;
        foreach( Enemy enemy in Enemies )
        {
            enemy.CheckCommand();
        }

        foreach( Enemy enemy in from e in Enemies orderby e.currentCommand.numBar select e )
        {
            EnemyCommand c = enemy.currentCommand;
            if( c.numBar == 1 )
            {
                enemy.SetExecBar( num1BarCommands % 4 );
                ++num1BarCommands;
            }
            else if( c.numBar == 2 )
            {
                enemy.SetExecBar( 2 * (num2BarCommands + (int)((num1BarCommands + 1) / 2)) % 4 );
                ++num2BarCommands;
            }
            else //0 or 4 or 3(not recommended)
            {
                enemy.SetExecBar( 0 );
            }
        }

        if( WeatherEnemy != null && !WeatherEnemy.IsSubstance )
        {
            WeatherEnemy.CheckCommand();
            WeatherEnemy.SetExecBar( 0 );
        }
    }

    public void CheckSkill()
    {
        int CurrentIndex = Music.Just.bar;
        if( GameContext.VoxSystem.state == VoxState.Invert ) return;
        if( GameContext.VoxSystem.state == VoxState.Eclipse && CurrentIndex == 3 ) return;

        foreach( Enemy e in Enemies )
        {
            e.CheckSkill();
        }
        if( WeatherEnemy != null && !WeatherEnemy.IsSubstance ) WeatherEnemy.CheckSkill();
    }

    public void OnInvert()
    {
        foreach( Enemy e in Enemies )
        {
            e.TurnInit();
        }
    }


    public void OnPlayerWin()
    {
    }
    public void OnPlayerLose()
    {
    }
}
