using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemySpecies
{
    Spirit,
    Fairy,
    Human,
    Thing,
    Beast,
    Dragon,
    Jewel,
    Weather,
	Boss,
}

public class Enemy : Character
{
    protected static readonly float DamageTrembleTime = 0.025f;
    protected static readonly Timing StateChangeTiming = new Timing(3,0,0);

    public string DisplayName;
    public EnemySpecies Speceis;
    public List<BattleState> States;
    public StateChangeCondition[] conditions;
	public string ExplanationText;
    //public SpriteRenderer outlineSprite;

    public EnemyCommand currentCommand { get; protected set; }
    public int commandExecBar { get; protected set; }
    public BattleState currentState { get; protected set; }
    public BattleState oldState { get; protected set; }
    public Vector3 targetLocalPosition { get; protected set; }

    protected int turnCount;
	protected ActionResult lastDamageResult;
	protected DamageGauge lastDamageGauge;
    protected SpriteRenderer spriteRenderer;
    protected List<EnemyCommandIcon> commandIcons;
	protected ShortTextWindow shortText;


    // Use this for initialization
	public virtual void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
        foreach( EnemyCommand c in GetComponentsInChildren<EnemyCommand>() )
        {
            c.Parse();
        }
        initialPosition = transform.localPosition;
        targetLocalPosition = initialPosition;

        foreach( StateChangeCondition condition in conditions )
        {
            condition.Parse();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
	public virtual void Update()
    {
        UpdateAnimation();

        if( GameContext.State == GameState.Battle )
        {
            if( Music.IsJustChangedAt( commandExecBar ) && currentCommand != null && currentCommand.ShortText != "" )
            {
                if( shortText != null )
                {
                    Destroy( shortText.gameObject );
                    shortText = null;
                }
                if( !GameContext.VoxSystem.IsOverloading )
                {
                    shortText = (Instantiate( GameContext.EnemyConductor.shortTextWindowPrefab ) as GameObject).GetComponent<ShortTextWindow>();
                    shortText.Initialize( currentCommand.ShortText );
                    shortText.transform.position = new Vector3( transform.position.x, shortText.transform.position.y, shortText.transform.position.z );
                }
            }
            if( Music.IsJustChangedAt( StateChangeTiming )
                && (currentState.name != "Invert" || GameContext.VoxSystem.OverloadTime == 1) )
            {
                if( currentState.name != "Invert" ) oldState = currentState;
                CheckState();
            }
        }
    }
    protected virtual void UpdateAnimation()
    {
        if( damageTime > 0 )
        {
            if( (int)(damageTime / DamageTrembleTime) != (int)((damageTime + Time.deltaTime) / DamageTrembleTime) )
            {
				transform.localPosition = initialPosition + Random.insideUnitSphere * Mathf.Clamp(damageTime -  GameContext.EnemyConductor.EnemyDamageTimeMin, 0.2f, 2.0f) * GameContext.EnemyConductor.EnemyDamageShake;
            }
            spriteRenderer.color = (damageTime % (DamageTrembleTime * 2) > DamageTrembleTime ? ( GameContext.VoxSystem.IsOverFlow ? ColorManager.Theme.Bright : Color.clear ) : GameContext.EnemyConductor.baseColor);

            damageTime -= Time.deltaTime;
            if( damageTime <= 0 )
            {
                transform.localPosition = initialPosition;
                if( HitPoint <= 0 )
                {
                    spriteRenderer.color = Color.clear;
                    Destroy( this.gameObject );
                }
                else
                {
                    spriteRenderer.color = GameContext.EnemyConductor.baseColor;
                }
            }
        }
    }

	protected void CreateDamageText(int damage, ActionResult actResult, GameObject parent = null)
	{
		if( damage == 0 ) return;
		if( lastDamageGauge != null && damage > 0 )
		{
			lastDamageGauge.AddDamage(damage);
		}
		else
		{
			GameObject damageGauge = (Instantiate(GameContext.EnemyConductor.damageGaugePrefab) as GameObject);
			if( damage > 0 )
			{
				lastDamageGauge = damageGauge.GetComponent<DamageGauge>();
			}
			damageGauge.GetComponent<DamageGauge>().Initialize(this, damage, actResult, parent);
		}
	}

    protected virtual void CheckState()
    {
        if( currentCommand != null && currentCommand.nextState != "" ) ChangeState( currentCommand.nextState );
        else if( turnCount >= currentState.pattern.Length && currentState.nextState != "" ) ChangeState( currentState.nextState );
        else
        {
            foreach( StateChangeCondition condition in conditions )
            {
                bool flag = true;
                foreach( ConditionParts parts in condition )
                {
                    int CompareValue = 0;
                    switch( parts.conditionType )
                    {
                    case ConditionType.MyHP:
                        CompareValue = HitPoint;
                        break;
                    case ConditionType.PlayerHP:
                        CompareValue = GameContext.PlayerConductor.PlayerHP;
                        break;
                    case ConditionType.TurnCount:
                        CompareValue = turnCount;
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
                if( flag && ( condition.FromState == "" || condition.FromState == currentState.name ) && condition.ToState != currentState.name )
                {
                    ChangeState( condition.ToState );
                    break;
                }
                else if( !flag && condition.ViceVersa && condition.ToState == currentState.name && condition.FromState != currentState.name )
                {
                    ChangeState( condition.FromState );
                    break;
                }
            }
        }
    }

    public override void TurnInit( CommandBase command )
    {
        base.TurnInit( command );
		if( GameContext.VoxSystem.State != VoxState.Overload )
		{
			lastDamageGauge = null;
		}
    }
    public virtual void InvertInit()
    {
		DefendPercent = 0;
        HealPercent = 0;
        TurnDamage = 0;
        currentCommand = null;

        oldState = currentState;
        BattleState invertState = States.Find( ( BattleState state ) => state.name == "Invert" );
        if( invertState == null )
        {
            invertState = new BattleState();
            invertState.name = "Invert";
            invertState.pattern = new EnemyCommand[0];
            invertState.nextState = "";
        }
        currentState = invertState;
    }
    public virtual void OnRevert()
	{
		if( currentState.name == "Invert" )
		{
			BattleState nextState = oldState;
			oldState = currentState;
			currentState = nextState;
		}
	}
    public EnemyCommand CheckCommand()
    {
        if( currentState.pattern != null && currentState.pattern.Length > 0 )
        {
            currentCommand = currentState.pattern[turnCount % currentState.pattern.Length];
            TurnInit( currentCommand );
        }
        else
        {
            currentCommand = null;
        }
        ++turnCount;

        return currentCommand;
    }
    public void SetWaitCommand( EnemyCommand WaitCommand )
    {
        DefaultInit();
        currentCommand = WaitCommand;
    }
    public void SetExecBar( int bar )
    {
        commandExecBar = bar;
    }
    public void ChangeState( string name )
    {
        if( currentState == null || currentState.name != name )
        {
            currentState = States.Find( ( BattleState state ) => state.name == name );
            if( currentState == null )
            {
                currentState = oldState;
                print( "ChangeState Failed: " + name );
            }
            turnCount = 0;
        }
    }
    public void InitState( string name )
    {
        ChangeState( name );
    }
    public void CheckSkill()
    {
        Skill skill = (currentCommand != null ? currentCommand.GetCurrentSkill( commandExecBar ) : null);
        if( skill != null )
        {
            Skill objSkill = (Skill)Instantiate( skill, new Vector3(), transform.rotation );
            objSkill.SetOwner( this );
            GameContext.BattleConductor.ExecSkill( objSkill );
        }
    }

    public override void BeAttacked(AttackModule attack, Skill skill)
	{
		float typeCoeff = 1.0f;

		if( attack.type == AttackType.Vox )
		{
			lastDamageResult = ActionResult.PhysicGoodDamage;
			typeCoeff = 2.0f;
		}
		else
		{
			/*
			switch( Speceis )
			{
			case EnemySpecies.Human:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
				else lastDamageResult = ActionResult.MagicDamage;
				break;
			case EnemySpecies.Thing:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicBadDamage;
				else lastDamageResult = ActionResult.MagicBadDamage;
				break;
			case EnemySpecies.Fairy:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
				else lastDamageResult = ActionResult.MagicGoodDamage;
				break;
			case EnemySpecies.Jewel:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
				else lastDamageResult = ActionResult.NoDamage;
				break;
			case EnemySpecies.Spirit:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicGoodDamage;
				else lastDamageResult = ActionResult.MagicDamage;
				break;
			case EnemySpecies.Dragon:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
				else lastDamageResult = ActionResult.MagicBadDamage;
				break;
			case EnemySpecies.Beast:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicBadDamage;
				else lastDamageResult = ActionResult.MagicDamage;
				break;
			case EnemySpecies.Weather:
				if( attack.type != AttackType.Dain ) lastDamageResult = ActionResult.MagicDamage;
				else lastDamageResult = ActionResult.NoDamage;
				break;
			case EnemySpecies.Boss:
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicBadDamage;
				else lastDamageResult = ActionResult.MagicBadDamage;
				break;
			}
			*/

			if( GameContext.VoxSystem.IsOverFlow )
			{
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicGoodDamage;
				else lastDamageResult = ActionResult.MagicGoodDamage;
			}
			else
			{
				switch( Speceis )
				{
				case EnemySpecies.Fairy:
					if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
					else lastDamageResult = ActionResult.MagicGoodDamage;
					break;
				case EnemySpecies.Spirit:
					if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicGoodDamage;
					else lastDamageResult = ActionResult.MagicDamage;
					break;
				default:
					if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
					else lastDamageResult = ActionResult.MagicDamage;
					break;
				}
				if( lastDamageResult.ToString().EndsWith("GoodDamage") ) typeCoeff = 2.0f;
			}
		}
        

		float overFlowPower = 0.0f;
		if( GameContext.VoxSystem.IsOverFlow && GameContext.VoxSystem.State != VoxState.Overload )
		{
			overFlowPower = attack.VP * 8.0f;
		}

		float damage = skill.OwnerCharacter.PhysicAttack * ((attack.Power + overFlowPower) / 100.0f) * typeCoeff * DefendCoeff;
        BeDamaged( Mathf.Max( 0, (int)damage ), skill );
        Debug.Log( this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint );
		
		if( lastDamageResult != ActionResult.NoDamage )
		{
			SEPlayer.Play(lastDamageResult, skill.OwnerCharacter, (int)damage);
		}
    }
	protected override void BeDamaged(int damage, Skill skill)
	{
		base.BeDamaged(damage, skill);
		CreateDamageText(damage, lastDamageResult, skill.damageParent);
		if( HitPoint <= 0 )
		{
			OnDead();
			if( shortText != null )
			{
				Destroy(shortText.gameObject);
				shortText = null;
			}
		}
	}
	public override void Heal(HealModule heal)
	{
		int oldHitPoint = HitPoint;
		base.Heal(heal);
		CreateDamageText(-(HitPoint - oldHitPoint), ActionResult.EnemyHeal);
		SEPlayer.Play(ActionResult.EnemyHeal, this, HitPoint - oldHitPoint);
	}
	public override void Drain(DrainModule drain, int drainDamage)
	{
		int oldHitPoint = HitPoint;
 		base.Drain(drain, drainDamage);
		CreateDamageText(-(HitPoint - oldHitPoint), ActionResult.EnemyHeal);
		SEPlayer.Play(ActionResult.EnemyHeal, this, HitPoint - oldHitPoint);
	}
    public override void UpdateHealHP()
    {
        base.UpdateHealHP();
    }

    public void SetTargetPosition( Vector3 target )
    {
        targetLocalPosition = target;
        initialPosition = target;
    }

    public void OnBaseColorChanged( Color newColor )
    {
        if( spriteRenderer == null ) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = newColor;
    }
    public void OnPlayerLose()
    {
    }

	public override void OnExecuted( Skill skill, ActionSet act )
	{
		base.OnExecuted(skill, act);

		if( skill.characterAnimName != "" && GetComponent<Animation>() != null && GetComponent<Animation>().GetClip(skill.characterAnimName) != null )
		{
			GetComponent<Animation>()[skill.characterAnimName].speed = 1 / (float)(Music.CurrentUnitPerBeat * Music.MusicalTimeUnit);
			GetComponent<Animation>().Play(skill.characterAnimName);
		}
	}
	public virtual void OnDead()
	{
	}

    public override string ToString()
    {
        return DisplayName + "(" + currentState.name + ")";
    }

    [System.Serializable]
    public class BattleState
    {
        public string name;
        public EnemyCommand[] pattern;
        public string nextState;
    }

    public enum ConditionType
    {
        MyHP,
        PlayerHP,
        TurnCount,
        EnemyCount,
        Count
    }
    public struct ConditionParts
    {
        public ConditionType conditionType;
        public int MaxValue;
        public int MinValue;
    }
    [System.Serializable]
    public class StateChangeCondition : IEnumerable<ConditionParts>
    {
        public List<string> _conditions;
        public string FromState;
        public string ToState;
        public bool ViceVersa;

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
}
