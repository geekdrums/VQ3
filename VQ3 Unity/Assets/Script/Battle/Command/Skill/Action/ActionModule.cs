using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IActionModule
{
}

public enum TargetType
{
	Left,
    Center,
    Right,

    Anim,
	Random,
    Select,
	All,
    
    Player,

    Self,
    Other,
    Weakest
	//Area, etc...
}

public class TargetModule : IActionModule
{
    public TargetModule( TargetType TargetType = TargetType.Left, int AnimIndex = -1 )
    {
        this.TargetType = TargetType;
        this.AnimIndex = AnimIndex;
    }
    public TargetType TargetType { get; protected set; }
    public int AnimIndex { get; private set; }

    public bool IsLocal { get { return this.TargetType != TargetType.All; } }
}

public class AnimModule : TargetModule
{
    public AnimModule( TargetType TargetType, string AnimName )
        : base( TargetType )
    {
        this.AnimName = AnimName;
    }

    public string AnimName { get; protected set; }
    public Enemy TargetEnemy { get; protected set; }

    public void SetTargetEnemy( Enemy e )
    {
        TargetEnemy = e;
    }
}

public enum AttackType
{
	Attack,
	Magic,
	Dain,
	Lacl,
	Aura,
	Igna,
	Vox,
}

public class AttackModule : TargetModule
{
	public AttackModule( int power, int vp, int vt, AttackType type,
        TargetType target = TargetType.Left, int animIndex = -1 )
		: base(target, animIndex)
	{
		this.Power = power;
		this.VP = vp;
		this.VT = vt;
		this.type = type;
	}

    public int Power { get; private set; }
    public int VP { get; private set; }
    public int VT { get; private set; }
	public AttackType type;
	public int DamageResult { get; private set; }

	public void SetDamageResult( int res )
	{
		DamageResult = res;
	}
}

public class HealModule : TargetModule
{
    public HealModule( int HealPoint, TargetType targetType = TargetType.Self )
        : base( targetType )
    {
		this.HealPoint = HealPoint;
	}

	public int HealPoint { get; private set; }
}

public class EnhanceModule : TargetModule
{
    public EnhanceModule( EnhanceParamType type, int phase, int turn, TargetType targetType = TargetType.Player )
        : base( targetType )
    {
        this.type = type;
        this.phase = phase;
        this.turn = turn;
	}

    public EnhanceParamType type { get; private set; }
    public int phase { get; private set; }
    public int turn { get; private set; }
}

public class DrainModule : TargetModule
{
	public DrainModule( float rate, int attackIndex, TargetType target = TargetType.Player )
		: base(target)
	{
		this.Rate = rate;
		this.AttackIndex = attackIndex;
	}

	public float Rate { get; private set; }
	public int AttackIndex { get; private set; }
}

public class WeatherModule : IActionModule
{
    public WeatherModule( string WeatherName, int Point )
    {
        this.WeatherName = WeatherName;
        this.Point = Point;
    }

    public string WeatherName { get; private set; }
    public int Point { get; private set; }
}

public class EnemySpawnModule : IActionModule
{
    public EnemySpawnModule( GameObject Enemy, string state )
    {
        this.EnemyPrefab = Enemy;
        this.InitialState = state;
    }

    public GameObject EnemyPrefab { get; private set; }
    public string InitialState { get; private set; }
}

public class WaitModule : IActionModule
{
	public WaitModule()
	{
	}
}