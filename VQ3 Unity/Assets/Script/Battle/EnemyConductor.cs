using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyConductor : MonoBehaviour {

    static readonly float EnemyInterval = 7.0f;
    static readonly int[] CommandExecBars = new int[3] { 2, 3, 1 };

    public GameObject damageTextPrefab;
    public GameObject commandIconPrefab;
    public List<Sprite> EnemyCommandIcons;
    public GameObject HPCirclePrefab;
    public GameObject shortTextWindowPrefab;
    public EnemyCommand PhysicDefaultCommand;
    public EnemyCommand MagicDefaultCommand;

    List<Enemy> Enemies = new List<Enemy>();
    WeatherEnemy WeatherEnemy;
    Encounter CurrentEncounter;

    public int EnemyCount { get { return Enemies.Count + (WeatherEnemy != null ? 1 : 0); } }
    public int VPtolerance
    {
        get
        {
            int res = 0;
            foreach( Enemy e in Enemies )
            {
                res += e.VPtolerance;
            }
            return Mathf.Min( 100, res );
        }
    }
    public Enemy targetEnemy { get; private set; }

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
    public int baseHP { get { return GameContext.PlayerConductor.PlayerMaxHP; } }

	// Use this for initialization
	void Start () {
		GameContext.EnemyConductor = this;
		baseColor = Color.black;
        PhysicDefaultCommand.Parse();
        MagicDefaultCommand.Parse();
	}
	
	// Update is called once per frame
	void Update () {
        if( GameContext.CurrentState == GameState.Battle )
        {
            if( Music.IsJustChangedBar() && Music.Just.bar >= 1 )
            {
                /*
                List<string> messages = new List<string>();
                //if( Music.Just.bar == 1 )
                //{
                //    foreach( Enemy passiveEnemy in from e in Enemies where e.currentCommand != null && e.currentCommand.isPassive select e )
                //    {
                //        messages.Add( passiveEnemy.DisplayName + passiveEnemy.currentCommand.DescribeText );
                //    }
                //    if( WeatherEnemy != null && WeatherEnemy.currentCommand != null && WeatherEnemy.currentCommand.isPassive )
                //    {
                //        messages.Add( WeatherEnemy.currentCommand.DescribeText );
                //    }
                //}
                if( messages.Count == 0 && ( Music.Just.bar < 2 || GameContext.VoxSystem.state == VoxState.Sun ) )
                {
                    Enemy enemy = Enemies.Find( ( Enemy e ) => e.commandExecBar == Music.Just.bar );
                    if( WeatherEnemy != null && WeatherEnemy.commandExecBar == Music.Just.bar )
                    {
                        messages.Add( WeatherEnemy.currentCommand.DescribeText );
                    }
                    else if( enemy != null && enemy.currentCommand != null )
                    {
                        messages.Add( enemy.DisplayName + enemy.currentCommand.DescribeText );
                    }
                }
                if( messages.Count > 0 )
                {
                    TextWindow.ChangeMessage( messages.ToArray() );
                }
                */
            }
        }
	}

    public void SetEncounter( Encounter encounter )
    {
        CurrentEncounter = encounter;
        int l = encounter.Enemies.Length;
        for( int i = 0; i < l; ++i )
        {
            SpawnEnemy( encounter.Enemies[i], encounter.StateSets[0][i], GetSpawnPosition( i, l ) );
        }
        targetEnemy = Enemies[(Enemies.Count - 1) / 2];
        GameContext.VoxSystem.SetTargetEnemy( targetEnemy );

        if( encounter.tutorialMessage.Type != TutorialMessageType.None )
        {
            TextWindow.SetTutorialMessage( encounter.tutorialMessage );
        }
    }

    Vector3 GetSpawnPosition( int index, int l ) { return new Vector3( EnemyInterval * (-(l - 1) / 2.0f + index) * (l == 2 ? 1.2f : 1.0f), 3.0f, 3 ); }

    void SpawnEnemy( GameObject enemyPrefab, string initialState, Vector3 spawnPosition )
    {
        GameObject TempObj;
        TempObj = (GameObject)Instantiate( enemyPrefab, spawnPosition, enemyPrefab.transform.rotation );
        //TempObj.renderer.material.color = baseColor;
        Enemy enemy = TempObj.GetComponent<Enemy>();
        WeatherEnemy we = TempObj.GetComponent<WeatherEnemy>();
        if( we != null )
        {
            WeatherEnemy = we;
            WeatherEnemy.InitState( WeatherEnemy.WeatherName );
        }
        else
        {
            Enemies.Add( enemy );
            TextWindow.ChangeMessage( BattleMessageType.EnemyEmerge, enemy.DisplayName + " があらわれた！" );
            enemy.InitState( initialState );
        }
        enemy.transform.localPosition += transform.position;
        enemy.transform.localScale *= transform.lossyScale.x;
        enemy.transform.parent = transform;
        enemy.SetTargetPosition( enemy.transform.localPosition );
        //enemy.outlineSprite.color = enemy.currentState.color;
        enemy.DisplayName += (char)((int)'A' + Enemies.FindAll( ( Enemy e ) => e.DisplayName.StartsWith( enemy.DisplayName ) && e.DisplayName.Length == enemy.DisplayName.Length + 1 ).Count);
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
                    skill.transform.localScale *= transform.lossyScale.x;
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
			GameContext.VoxSystem.AddVPVT((int)(attack.VP*(skill.OwnerCharacter as Player).VPCoeff), (int)(attack.VT*(skill.OwnerCharacter as Player).VTCoeff));
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
        if( weather != null )
        {
            if( WeatherEnemy != null )
            {
                bool IsOldSubstance = WeatherEnemy.IsSubstance;
                WeatherEnemy.ReceiveWeatherModule( weather );
                if( IsOldSubstance != WeatherEnemy.IsSubstance )
                {
                    if( IsOldSubstance ) Enemies.Remove( WeatherEnemy );
                    else Enemies.Add( WeatherEnemy );
                }
                isSucceeded = true;
            }
        }
        EnemySpawnModule spawner = Action.GetModule<EnemySpawnModule>();
        if( spawner != null && Enemies.Count < 3 )
        {
            for( int i = 0; i < Enemies.Count; i++ ) Enemies[i].SetTargetPosition( GetSpawnPosition( i, Enemies.Count + 1 ) );
            SpawnEnemy( spawner.EnemyPrefab, spawner.InitialState, GetSpawnPosition( Enemies.Count, Enemies.Count + 1 ) );
            isSucceeded = true;
            GameContext.VoxSystem.SetTargetEnemy( targetEnemy );
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
            if( !Enemies.Contains( targetEnemy ) )
            {
                targetEnemy = Enemies[(Enemies.Count - 1) / 2];
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
                if( enemy.HitPoint < minHP && enemy.HitPoint < enemy.MaxHP )
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

    public void UpdateHealHP()
    {
        foreach( Enemy enemy in Enemies )
        {
            enemy.UpdateHealHP();
        }
    }

    public void CheckCommand()
    {
        //if( GameContext.VoxSystem.state == VoxState.Invert ) return;

        int execIndex = 0;
        foreach( Enemy enemy in Enemies )
        {
            enemy.CheckCommand();
            enemy.SetExecBar( CommandExecBars[execIndex] );
            ++execIndex;
        }

        if( WeatherEnemy != null && !WeatherEnemy.IsSubstance )
        {
            WeatherEnemy.CheckCommand();
            WeatherEnemy.SetExecBar( CommandExecBars[execIndex] );
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
    public void CheckWaitCommand()
    {
        //int index = GameContext.PlayerConductor.WaitCount % Enemies.Count;
        for( int i=0;i<Enemies.Count; i++ )
        {
            //if( i == index )
            //{
            //    Enemies[i].SetWaitCommand( Enemies[i].PhysicAttack >= Enemies[i].MagicAttack ? PhysicDefaultCommand : MagicDefaultCommand );
            //}
            //else
            //{
            //    Enemies[i].SetWaitCommand( null );
            //}
            Enemies[i].SetWaitCommand( null );
            Enemies[i].SetExecBar( 0 );
        }
        if( WeatherEnemy != null && !WeatherEnemy.IsSubstance )
        {
            WeatherEnemy.SetWaitCommand( null );
            WeatherEnemy.SetExecBar( 0 );
        }
    }

    public void CheckSkill()
    {
        //int CurrentIndex = Music.Just.bar;
        //if( GameContext.VoxSystem.state == VoxState.Invert ) return;
        if( GameContext.VoxSystem.IsInverting ) return;//state == VoxState.Eclipse && GameContext.VoxSystem.IsReadyEclipse && CurrentIndex >= 2 ) return;

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
            e.InvertInit();
        }
    }
    public void OnRevert()
    {
        foreach( Enemy e in Enemies )
        {
            e.OnRevert();
        }
    }

    public void OnArrowPushed( bool LorR )
    {
        if( Enemies.Count > 0 )
        {
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
        Cleanup();
    }
    public void OnPlayerLose()
    {
        Cleanup();
        foreach( Enemy e in Enemies )
        {
            e.OnPlayerLose();
        }
    }
    public void OnContinue()
    {
        SetEncounter( CurrentEncounter );
    }

    void Cleanup()
    {
        foreach( Enemy e in Enemies )
        {
            Destroy( e.gameObject );
            //e.OnContinue();
            //TextWindow.AddMessage( new GUIMessage( e.DisplayName + " があらわれた！" ) );
        }
        if( WeatherEnemy != null )
        {
            Destroy( WeatherEnemy.gameObject );
            WeatherEnemy = null;
        }
        Enemies.Clear();
    }


    [System.Serializable]
    public class StateSet
    {
        public StateSet( string states )
        {
            nameList = states;
        }

        public string nameList;
        public string this[int i]
        {
            get
            {
                return nameList.Split( ' ' )[i];
            }
        }
    }
    /*
    public enum ConditionType
    {
        ItsHP,
        PlayerHP,
        TotalTurnCout,
        OnDead,
        OnChanged,
        Count
    }
    public struct ConditionParts
    {
        public ConditionType conditionType;
        public int MaxValue;
        public int MinValue;
    }
    [System.Serializable]
    public class StateSetCondition : IEnumerable<ConditionParts>
    {
        public List<string> _conditions;
        public string StateNameList;

        List<ConditionParts> conditionParts = new List<ConditionParts>();

        public void Parse()
        {
            foreach( string str in _conditions )
            {
                string[] conditionParams = str.Split( ' ' );
                if( conditionParams.Length != 3 ) Debug.LogError( "condition param must be TYPE MIN MAX format. ->" + str );
                else
                {
                    conditionParts.Add( new ConditionParts()
                    {
                        conditionType = (ConditionType)System.Enum.Parse( typeof( ConditionType ), conditionParams[0] ),
                        MinValue = conditionParams[1] == "-" ? -9999999 : int.Parse( conditionParams[1] ),
                        MaxValue = conditionParams[2] == "-" ? +9999999 : int.Parse( conditionParams[2] ),
                    } );
                }
            }
        }

        public IEnumerator<ConditionParts> GetEnumerator()
        {
            foreach( ConditionParts parts in conditionParts ) yield return parts;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    */
}
