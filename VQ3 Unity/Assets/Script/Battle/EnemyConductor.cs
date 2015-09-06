using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyConductor : MonoBehaviour {

    static readonly float EnemyInterval = 7.0f;
    static readonly int[] CommandExecBars = new int[3] { 2, 3, 1 };

    public GameObject damageGaugePrefab;
    public GameObject commandIconPrefab;
    public List<Sprite> EnemyCommandIcons;
    public GameObject shortTextWindowPrefab;
    public EnemyCommand PhysicDefaultCommand;
    public EnemyCommand MagicDefaultCommand;

    List<Enemy> Enemies = new List<Enemy>();
	Encounter CurrentEncounter;

    public int EnemyCount { get { return Enemies.Count; } }
	public int InvertVP { get { return ( CurrentEncounter != null ? CurrentEncounter.InvertVP : 0 ); } }
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
	void Awake()
	{
		GameContext.EnemyConductor = this;
	}

	// Use this for initialization
	void Start () {
		baseColor = Color.black;
        PhysicDefaultCommand.Parse();
        MagicDefaultCommand.Parse();
	}
	
	// Update is called once per frame
	void Update () {
        if( GameContext.State == GameState.Battle )
        {
			if( Input.GetMouseButtonUp(0) )
			{
				Ray ray = GameContext.MainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);

				foreach( Enemy e in Enemies )
				{
					if( e.GetComponent<Collider>() == hit.collider )
					{
						targetEnemy = e;
						GameContext.VoxSystem.SetTargetEnemy(targetEnemy);
						break;
					}
				}
			}
        }
	}

    public void SetEncounter( Encounter encounter )
    {
        CurrentEncounter = encounter;
		Encounter.BattleSet battleSet = encounter.BattleSets[0];
		int l = battleSet.Enemies.Length;
        for( int i = 0; i < l; ++i )
        {
			SpawnEnemy(battleSet.Enemies[i], battleSet.StateSets[0][i], GetInitSpawnPosition(i, l));
        }
        targetEnemy = Enemies[0];
        GameContext.VoxSystem.SetTargetEnemy( targetEnemy );
    }

	Vector3 GetInitSpawnPosition( int index, int l ) { return new Vector3( EnemyInterval * (-(l - 1) / 2.0f + index) * (l == 2 ? 1.2f : 1.0f), 3.0f, 5 ); }
	Vector3 GetSpawnPosition( int index, int l ) { return new Vector3((index == 0 ? 0 : (index == 1 ? EnemyInterval : -EnemyInterval)), 3.0f, 5); }


    void SpawnEnemy( GameObject enemyPrefab, string initialState, Vector3 spawnPosition )
    {
        GameObject TempObj;
        TempObj = (GameObject)Instantiate( enemyPrefab );
        Enemy enemy = TempObj.GetComponent<Enemy>();
        Enemies.Add( enemy );
        TextWindow.SetMessage( MessageCategory.EnemyEmerge, enemy.DisplayName + " があらわれた！" );
        enemy.InitState( initialState );
		enemy.transform.localPosition = spawnPosition;
        enemy.transform.localScale *= transform.lossyScale.x;
        enemy.transform.parent = transform;
        enemy.SetTargetPosition( enemy.transform.localPosition );
        enemy.DisplayName += (char)((int)'A' + Enemies.FindAll( ( Enemy e ) => e.DisplayName.StartsWith( enemy.DisplayName ) && e.DisplayName.Length == enemy.DisplayName.Length + 1 ).Count);
    }

	public bool ReceiveAction( ActionSet Action, Skill skill )
	{
		bool isSucceeded = false;
        AnimModule anim = Action.GetModule<AnimModule>();
        if( anim != null && skill.isPlayerSkill )
		{
			if( anim.IsLocal )
			{
				foreach( Enemy e in GetTargetEnemies(anim, skill) )
				{
					isSucceeded = true;
					anim.SetTargetEnemy(e);
					skill.transform.localPosition = e.transform.localPosition + Vector3.back * 3.0f;
					skill.transform.localScale *= transform.lossyScale.x;
					break;
				}
			}
			else
			{
				skill.transform.localScale *= transform.lossyScale.x;
				isSucceeded = true;
			}
        }
		AttackModule attack = Action.GetModule<AttackModule>();
        if( attack != null && skill.isPlayerSkill )
		{
			GameContext.VoxSystem.AddVPVT((int)(attack.VP*(skill.OwnerCharacter as Player).VPCoeff), (int)(attack.VT*(skill.OwnerCharacter as Player).VTCoeff));
            foreach( Enemy e in GetTargetEnemies( attack, skill ) )
            {
				e.BeAttacked( attack, skill );
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
		DrainModule drain = Action.GetModule<DrainModule>();
		if( drain != null && !skill.isPlayerSkill )
		{
			skill.OwnerCharacter.Drain(drain, skill.Actions[drain.AttackIndex].GetModule<AttackModule>().DamageResult);
			isSucceeded = true;
		}
        EnemySpawnModule spawner = Action.GetModule<EnemySpawnModule>();
        if( spawner != null && Enemies.Count < 3 )
        {
            for( int i = 0; i < Enemies.Count; i++ ) Enemies[i].SetTargetPosition( GetSpawnPosition( i, Enemies.Count + 1 ) );
            SpawnEnemy( spawner.EnemyPrefab, spawner.InitialState, GetSpawnPosition( Enemies.Count, Enemies.Count + 1 ) );
            isSucceeded = true;
			GameContext.VoxSystem.SetTargetEnemy(targetEnemy);
			SEPlayer.Play("spawn");
        }
        

		Enemies.RemoveAll( ( Enemy e ) => e.HitPoint<=0 );
		if( Enemies.Count == 0 )
		{
			GameContext.BattleConductor.SetState(BattleState.Win);
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
        int execIndex = 0;
        foreach( Enemy enemy in Enemies )
        {
            enemy.CheckCommand();
            enemy.SetExecBar( CommandExecBars[execIndex] );
            ++execIndex;
        }
    }
    public void CheckWaitCommand()
    {
        for( int i=0;i<Enemies.Count; i++ )
        {
            Enemies[i].SetWaitCommand( null );
            Enemies[i].SetExecBar( 0 );
        }
    }

    public void CheckSkill()
    {
        if( GameContext.VoxSystem.IsOverloading ) return;

        foreach( Enemy e in Enemies )
        {
            e.CheckSkill();
        }
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
		Cleanup();
    }

    void Cleanup()
    {
        foreach( Enemy e in Enemies )
        {
            Destroy( e.gameObject );
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
}
