using System.Collections;
using System.Collections.Generic;

public interface IActionModule
{
}

public enum TargetType
{
	First,
	Random,
	All
	//Area, etc...
}

public class AttackModule : IActionModule
{
	public AttackModule( int AttackPower, TargetType TargetType = TargetType.First )
	{
		this.AttackPower = AttackPower;
		this.TargetType = TargetType;
	}

	public int AttackPower { get; private set; }
	public TargetType TargetType { get; private set; }
}

public class MagicModule : IActionModule
{
	public MagicModule( int MagicPower, int VoxonEnergy, TargetType TargetType = TargetType.First )
	{
		this.MagicPower = MagicPower;
		this.VoxonEnergy = VoxonEnergy;
		this.TargetType = TargetType;
	}

	public int MagicPower { get; private set; }
	public int VoxonEnergy { get; private set; }
	public TargetType TargetType { get; private set; }
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