using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyConductor : MonoBehaviour {

    static readonly float EnemyInterval = 7.0f;
    static readonly int[] CommandExecBars = new int[3] { 2, 3, 1 };

    public GameObject damageTextPrefab;
    public GameObject HPCirclePrefab;
    public GameObject shortTextWindowPrefab;

    List<Enemy> Enemies = new List<Enemy>();
    WeatherEnemy WeatherEnemy;
    Encounter CurrentEncounter;

    public int EnemyCount { get { return Enemies.Count + (WeatherEnemy != null ? 1 : 0); } }
    public Enemy targetEnemy { get; private set; }
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
        if( GameContext.CurrentState == GameState.Battle )
        {
            if( Music.IsJustChangedBar() && Music.Just.bar >= 1 )
            {
                List<string> messages = new List<string>();
                if( Music.Just.bar == 1 )
                {
                    foreach( Enemy passiveEnemy in from e in Enemies where e.currentCommand != null && e.currentCommand.isPassive select e )
                    {
                        messages.Add( passiveEnemy.DisplayName + passiveEnemy.currentCommand.DescribeText );
                    }
                    if( WeatherEnemy != null && WeatherEnemy.currentCommand != null && WeatherEnemy.currentCommand.isPassive )
                    {
                        messages.Add( WeatherEnemy.currentCommand.DescribeText );
                    }
                }
                if( messages.Count == 0 && ( Music.Just.bar < 2 || GameContext.VoxSystem.state == VoxState.Sun ) )
                {
                    Enemy enemy = Enemies.Find( ( Enemy e ) => e.commandExecBar == Music.Just.bar );
                    if( enemy != null && enemy.currentCommand != null )
                    {
                        messages.Add( enemy.DisplayName + enemy.currentCommand.DescribeText );
                    }
                    else if( WeatherEnemy != null && WeatherEnemy.commandExecBar == Music.Just.bar )
                    {
                        messages.Add( WeatherEnemy.currentCommand.DescribeText );
                    }
                }
                if( messages.Count > 0 )
                {
                    TextWindow.ChangeMessage( messages.ToArray() );
                }
            }
        }
	}

    public void SetEncounter( Encounter encounter )
    {
        CurrentEncounter = encounter;
        foreach( Enemy e in Enemies )
        {
            Destroy( e.gameObject );
        }
        Enemies.Clear();
        GameObject TempObj;
        int l = encounter.Enemies.Length;
        for( int i = 0; i < l; ++i )
        {
            TempObj = (GameObject)Instantiate( encounter.Enemies[i], new Vector3( EnemyInterval * (-(l - 1) / 2.0f + i) * (l==2 ? 1.2f : 1.0f), 4, 0 ), encounter.Enemies[i].transform.rotation );
			TempObj.renderer.material.color = baseColor;
            Enemy enemy = TempObj.GetComponent<Enemy>();
            WeatherEnemy we = TempObj.GetComponent<WeatherEnemy>();
            if( we != null )
            {
                WeatherEnemy = we;
            }
            else
            {
                Enemies.Add( enemy );
            }
            enemy.transform.parent = transform;
            enemy.ChangeState( encounter.StateSets[0][i] );
            enemy.DisplayName += (char)((int)'A' + Enemies.FindAll( ( Enemy e ) => e.DisplayName.StartsWith( enemy.DisplayName ) && e.DisplayName.Length == enemy.DisplayName.Length + 1 ).Count);
        }
        targetEnemy = Enemies[(Enemies.Count - 1) / 2];
        GameContext.VoxSystem.SetTargetEnemy( targetEnemy );

        TextWindow.ClearMessages();
        foreach( Enemy e in Enemies )
        {
            TextWindow.AddMessage( new GUIMessage( e.DisplayName + " Ç™Ç†ÇÁÇÌÇÍÇΩÅI" ) );
        }
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
            //if( nextTarget != null && !Enemies.Contains( nextTarget ) ) nextTarget = Enemies[(Enemies.Count - 1) / 2];
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
        case TargetType.Left:
            Enemy leftTarget = Enemies.Find( ( Enemy e ) => e.transform.position.x <= -EnemyInterval / 2 );
            if( leftTarget != null ) Res.Add( leftTarget );
            break;
        case TargetType.Center:
            Enemy centerTarget = Enemies.Find( ( Enemy e ) => Mathf.Abs( e.transform.position.x ) <= EnemyInterval / 2 );
            if( centerTarget != null ) Res.Add( centerTarget );
            break;
        case TargetType.Right:
            Enemy rightTarget = Enemies.Find( ( Enemy e ) => e.transform.position.x >= EnemyInterval / 2 );
            if( rightTarget != null ) Res.Add( rightTarget );
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
        //if( nextTarget != null )
        //{
        //    targetEnemy = nextTarget;
        //    GameContext.VoxSystem.SetTargetEnemy( targetEnemy );
        //}
        int execIndex = 0;
        foreach( Enemy enemy in Enemies )
        {
            enemy.CheckCommand();
            if( enemy.currentCommand.isPassive )
            {
                enemy.SetExecBar( 0 );
            }
            else
            {
                enemy.SetExecBar( CommandExecBars[execIndex] );
                ++execIndex;
            }
        }

        if( WeatherEnemy != null && !WeatherEnemy.IsSubstance )
        {
            WeatherEnemy.CheckCommand();
            if( WeatherEnemy.currentCommand.isPassive )
            {
                WeatherEnemy.SetExecBar( 0 );
            }
            else
            {
                WeatherEnemy.SetExecBar( CommandExecBars[execIndex] );
            }
        }

        /*
        int num2BarCommands = 0;
        int num1BarCommands = 0;
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
        */
    }

    public void CheckSkill()
    {
        int CurrentIndex = Music.Just.bar;
        if( GameContext.VoxSystem.state == VoxState.Invert ) return;
        if( GameContext.VoxSystem.state == VoxState.Eclipse && CurrentIndex >= 2 ) return;

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

    public void OnNextCommandChanged( Command NextCommand )
    {
        /*
        if( Enemies.Count > 0 )
        {
            if( NextCommand.IsTargetSelectable )
            {
                if( nextTarget == null ) nextTarget = targetEnemy;
                GameContext.VoxSystem.SetNextTargetEnemy( nextTarget );
            }
            else if( nextTarget != null )
            {
                nextTarget = null;
                GameContext.VoxSystem.SetNextTargetEnemy( null );
            }
        }
        */
    }

    public void OnArrowPushed( bool LorR )
    {
        if( Enemies.Count > 0 )
        {
            /*
            int targetIndex = Enemies.IndexOf( nextTarget );
            if( LorR )
            {
                targetIndex = (targetIndex - 1 + Enemies.Count) % Enemies.Count;
                nextTarget = Enemies[targetIndex];
            }
            else
            {
                targetIndex = (targetIndex + 1) % Enemies.Count;
                nextTarget = Enemies[targetIndex];
            }
            GameContext.VoxSystem.SetNextTargetEnemy( nextTarget );
            */
            int targetIndex = Enemies.IndexOf( targetEnemy );
            if( LorR )
            {
                targetIndex = (targetIndex - 1 + Enemies.Count) % Enemies.Count;
                targetEnemy = Enemies[targetIndex];
            }
            else
            {
                targetIndex = (targetIndex + 1) % Enemies.Count;
                targetEnemy = Enemies[targetIndex];
            }
            GameContext.VoxSystem.SetTargetEnemy( targetEnemy );
        }
    }

    public void OnPlayerWin()
    {
    }
    public void OnPlayerLose()
    {
        foreach( Enemy e in Enemies )
        {
            e.OnPlayerLose();
        }
    }
    public void OnContinue()
    {
        foreach( Enemy e in Enemies )
        {
            Destroy( e.gameObject );
            //e.OnContinue();
            //TextWindow.AddMessage( new GUIMessage( e.DisplayName + " Ç™Ç†ÇÁÇÌÇÍÇΩÅI" ) );
        }
        if( WeatherEnemy != null )
        {
            Destroy( WeatherEnemy.gameObject );
        }
        SetEncounter( CurrentEncounter );
    }
}
