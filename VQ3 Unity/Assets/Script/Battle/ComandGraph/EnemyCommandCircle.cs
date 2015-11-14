using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyCommandCircle : MonoBehaviour {

	public GameObject PatternCirclePrefab;
	public MidairPrimitive Shade;

	public Encounter.BattleSet BattleSet { get; set; }

	public EnemyCommandState CurrentState { get; private set; }
	public EnemyCommandState OldState { get; private set; }
	public EnemyCommandSet CurrentCommandSet { get; private set; }
	public int TurnCount { get; private set; }

	static readonly Timing StateChangeTiming = new Timing(3, 0, 0);
	static readonly Timing SetNextTiming = new Timing(3, 3, 0);

	List<EnemyCommandPattern> patternCircles_ = new List<EnemyCommandPattern>();
	EnemyCommandPattern currentPatternCircle_ = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if( GameContext.State == GameState.Battle )
		{
			if( CurrentState.Name == "Invert" && GameContext.LuxSystem.BreakTime > 1 )
			{
			}
			else
			{
				if( GameContext.BattleState == BattleState.Intro || GameContext.BattleState == BattleState.Wait || GameContext.BattleState == BattleState.ShieldBreak )
				{
				}
				else if( Music.IsJustChangedAt(StateChangeTiming) )
				{
					if( CurrentState.Name != "Invert" ) OldState = CurrentState;
					CheckState();
					//todo: パターンサークルどうしよう？
				}
				else if( Music.IsJustChangedAt(SetNextTiming) )
				{
					currentPatternCircle_.SetNext();
					UpdateShade();
				}
			}

		}
	}

	private void UpdateShade()
	{
		Shade.SetTargetColor(ColorManager.MakeAlpha(Color.black, Mathf.Pow(currentPatternCircle_.GetCurrentThreat()/100.0f, 2)));
	}


	private void CheckState()
	{
		if( TurnCount >= CurrentState.Pattern.Length && CurrentState.NextState != "" ) ChangeState(CurrentState.NextState);
		else
		{
			foreach( StateChangeCondition condition in BattleSet.Conditions )
			{
				bool flag = true;
				foreach( ConditionParts parts in condition )
				{
					int CompareValue = 0;
					switch( parts.conditionType )
					{
					case ConditionType.PlayerHP:
						CompareValue = GameContext.PlayerConductor.PlayerHP;
						break;
					case ConditionType.TurnCount:
						CompareValue = TurnCount;
						break;
					case ConditionType.EnemyCount:
						CompareValue = GameContext.EnemyConductor.EnemyCount;
						break;
					default:
						continue;
					}
					flag &= (parts.MinValue <= CompareValue && CompareValue <= parts.MaxValue);
					if( !flag ) break;
				}
				if( flag && (condition.FromState == "" || condition.FromState == CurrentState.Name) && condition.ToState != CurrentState.Name )
				{
					ChangeState(condition.ToState);
					break;
				}
				else if( !flag && condition.ViceVersa && condition.ToState == CurrentState.Name && condition.FromState != CurrentState.Name )
				{
					ChangeState(condition.FromState);
					break;
				}
			}
		}
	}


	public EnemyCommandSet CheckCommand()
	{
		if( CurrentState.Pattern != null && CurrentState.Pattern.Length > 0 )
		{
			CurrentCommandSet = CurrentState.Pattern[TurnCount % CurrentState.Pattern.Length];
		}
		else
		{
			CurrentCommandSet = null;
		}
		++TurnCount;

		currentPatternCircle_.SetCurrent();

		return CurrentCommandSet;
	}

	public void ChangeState(string name)
	{
		if( CurrentState == null || CurrentState.Name != name )
		{
			CurrentState = BattleSet.States.Find((EnemyCommandState state) => state.Name == name);
			if( CurrentState == null )
			{
				CurrentState = OldState;
				print("ChangeState Failed: " + name);
			}
			TurnCount = 0;
		}
	}

	public void BattleInit(Encounter.BattleSet battleSet)
	{
		BattleSet = battleSet;
		CurrentState = OldState = null;
		CurrentCommandSet = null;
		foreach( StateChangeCondition condition in battleSet.Conditions )
		{
			condition.Parse();
		}
		ChangeState(battleSet.States[0].Name);

		foreach( EnemyCommandPattern pattern in patternCircles_ )
		{
			Destroy(pattern.gameObject);
		}
		patternCircles_.Clear();
		for( int i=0; i<battleSet.States.Count; ++i )
		{
			EnemyCommandPattern pattern = Instantiate(PatternCirclePrefab).GetComponent<EnemyCommandPattern>();
			pattern.transform.parent = this.transform;
			pattern.transform.localPosition = Vector3.zero;
			pattern.transform.localScale = Vector3.one;
			pattern.Initialize(battleSet.States[i]);
			patternCircles_.Add(pattern);
		}
		currentPatternCircle_ = patternCircles_[0];
		UpdateShade();
	}

	public void InvertInit()
	{
		OldState = CurrentState;
		EnemyCommandState invertState = BattleSet.States.Find((EnemyCommandState state) => state.Name == "Invert");
		if( invertState == null )
		{
			invertState = new EnemyCommandState();
			invertState.Name = "Invert";
			invertState.Pattern = new EnemyCommandSet[0];
			invertState.NextState = "";
		}
		CurrentState = invertState;
		Shade.SetColor(Color.clear);
	}

	public virtual void OnRevert()
	{
		if( CurrentState.Name == "Invert" )
		{
			EnemyCommandState nextState = OldState;
			OldState = CurrentState;
			CurrentState = nextState;
		}
	}
}
