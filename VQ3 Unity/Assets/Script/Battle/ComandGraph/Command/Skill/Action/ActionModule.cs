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

public class AttackModule : TargetModule
{
	public AttackModule( int Power, int VoxPoint, int VoxTime, bool isPhysic,
        TargetType TargetType = TargetType.Left, int AnimIndex = -1 )
        : base( TargetType, AnimIndex )
	{
        this.Power = Power;
        this.VP = VoxPoint;
        this.VT = VoxTime;
        this.isPhysic = isPhysic;
	}

    public int Power { get; private set; }
    public int VP { get; private set; }
    public int VT { get; private set; }
    public bool isPhysic;
}

/*
public class MagicModule : TargetModule
{
    public MagicModule( int MagicPower, int VoxPoint, TargetType TargetType = TargetType.Left, int AnimIndex = -1 )
        : base( TargetType, AnimIndex )
	{
		this.MagicPower = MagicPower;
        this.VoxPoint = VoxPoint;
	}

	public int MagicPower { get; private set; }
    public int VoxPoint { get; private set; }
}

public class DefendModule : TargetModule
{
	public DefendModule( int DefendPower, TargetType targetType = TargetType.Self )
        : base( targetType )
	{
		this.DefendPower = DefendPower;
	}

	public int DefendPower { get; private set; }
}

public class MagicDefendModule : TargetModule
{
    public MagicDefendModule( int MagicDefendPower, TargetType targetType = TargetType.Self )
        : base( targetType )
    {
        this.MagicDefendPower = MagicDefendPower;
    }

    public int MagicDefendPower { get; private set; }
}
*/

public class HealModule : TargetModule
{
    public HealModule( int HealPoint, TargetType targetType = TargetType.Self )
        : base( targetType )
    {
		this.HealPoint = HealPoint;
	}

	public int HealPoint { get; private set; }
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