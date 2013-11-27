using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IActionModule
{
}

public enum TargetType
{
	First,
    Second,
    Third,
    Fourth,
    Fifth,

    Anim,
	Random,
    Select,
	All
	//Area, etc...
}

public class TargetModule : IActionModule
{
    public TargetModule( TargetType TargetType = TargetType.First, int AnimIndex = -1 )
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
	public AttackModule( int AttackPower, TargetType TargetType = TargetType.First, int AnimIndex = -1 )
        : base( TargetType, AnimIndex )
	{
		this.AttackPower = AttackPower;
	}

	public int AttackPower { get; private set; }
}

public class MagicModule : TargetModule
{
    public MagicModule( int MagicPower, int VoxonEnergy, TargetType TargetType = TargetType.First, int AnimIndex = -1 )
        : base( TargetType, AnimIndex )
	{
		this.MagicPower = MagicPower;
        this.VoxonEnergy = VoxonEnergy;
	}

	public int MagicPower { get; private set; }
    public int VoxonEnergy { get; private set; }
}

public class DefendModule : IActionModule
{
	public DefendModule( int DefendPower )
	{
		this.DefendPower = DefendPower;
	}

	public int DefendPower { get; private set; }
}

public class HealModule : IActionModule
{
	public HealModule( int HealPoint )
	{
		this.HealPoint = HealPoint;
	}

	public int HealPoint { get; private set; }
}

public class PowerModule : IActionModule
{
	public PowerModule( int AttackPower )
	{
		this.AttackPower = AttackPower;
	}

	public int AttackPower { get; private set; }
}