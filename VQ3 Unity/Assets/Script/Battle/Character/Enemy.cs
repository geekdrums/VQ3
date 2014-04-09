using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemySpecies
{
    Spirit,
    Fairy,
    Human,
    Jewel,
    Beast,
    Dragon,
    Weather,
    Thing
}

public class Enemy : Character
{
    static readonly float DamageTrembleTime = 0.025f;

    public string DisplayName;
    public EnemySpecies Speceis;
    public List<BattleState> States;
    public StateChangeCondition[] conditions;
    public int VPtolerance;

    public EnemyCommand currentCommand { get; protected set; }
    public int commandExecBar { get; protected set; }
    public BattleState currentState { get; protected set; }

    protected HPCircle HPCircle;
    protected int turnCount;
    protected ActionResult lastDamageResult;

    // Use this for initialization
    protected virtual void Start()
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
        HPCircle = (Instantiate( GameContext.EnemyConductor.HPCirclePrefab, transform.position + Vector3.down * 5.0f, Quaternion.identity ) as GameObject).GetComponent<HPCircle>();
        HPCircle.transform.parent = transform;
        HPCircle.transform.localScale *= Mathf.Min( 1.5f, Mathf.Sqrt( (float)HitPoint / (float)GameContext.EnemyConductor.baseHP ) );
        HPCircle.Initialize( this );
        HPCircle.OnTurnStart();
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
    }
    protected virtual void UpdateAnimation()
    {
        if( damageTime > 0 )
        {
            if( lastDamageResult == ActionResult.PhysicGoodDamage || lastDamageResult == ActionResult.MagicGoodDamage )
            {
                if( (int)(damageTime / DamageTrembleTime) != (int)((damageTime + Time.deltaTime) / DamageTrembleTime) )
                {
                    transform.localPosition = initialPosition + Random.insideUnitSphere * Mathf.Clamp( damageTime, 0.1f, 1.5f ) * 1.3f;
                }
            }
            renderer.material.color = (damageTime % (DamageTrembleTime * 2) > DamageTrembleTime ? Color.clear : GameContext.EnemyConductor.baseColor);
            damageTime -= Time.deltaTime;
            if( damageTime <= 0 )
            {
                transform.localPosition = initialPosition;
                if( HitPoint <= 0 )
                {
                    renderer.material.color = Color.clear;
                    Destroy( this.gameObject );
                }
                else
                {
                    renderer.material.color = GameContext.EnemyConductor.baseColor;
                }
            }
        }
    }

    protected void CreateDamageText( int damage )
    {
        if( damage == 0 ) return;
        GameObject damageText = (Instantiate( GameContext.EnemyConductor.damageTextPrefab ) as GameObject);
        damageText.GetComponent<DamageText>().Initialize( damage, transform.position + Vector3.back * 3 + Random.onUnitSphere );
    }
    protected virtual void CheckState()
    {
        if( currentCommand != null && currentCommand.nextState != "" ) ChangeState( currentCommand.nextState );
        else
        {
            foreach( StateChangeCondition condition in conditions )
            {
                int CompareValue = 0;
                switch( condition.conditionType )
                {
                case ConditionType.PlayerHP:
                    CompareValue = GameContext.PlayerConductor.PlayerHP;
                    break;
                case ConditionType.TurnCout:
                    CompareValue = turnCount;
                    break;
                case ConditionType.EnemyCount:
                    CompareValue = GameContext.EnemyConductor.EnemyCount;
                    break;
                default:
                    continue;
                }
                int d = (CompareValue - condition.Value);
                if( condition.FromState == "" || condition.FromState == currentState.name )
                {
                    if( d * condition.Sign > 0 || (d == 0 && condition.Sign == 0) )
                    {
                        ChangeState( condition.ToState );
                        break;
                    }
                }
                else if( condition.ViceVersa && (condition.ToState == currentState.name) )
                {
                    if( d * condition.Sign < 0 && condition.FromState != "" )
                    {
                        ChangeState( condition.FromState );
                        break;
                    }
                }
            }
        }
    }
    protected virtual void CheckStateOnDamage( int damage )
    {
        foreach( StateChangeCondition condition in conditions )
        {
            int CompareValue = 0;
            switch( condition.conditionType )
            {
            case ConditionType.MyHP:
                CompareValue = HitPoint;
                break;
            case ConditionType.OneDamage:
                CompareValue = damage;
                break;
            case ConditionType.TurnDamage:
                CompareValue = TurnDamage;
                break;
            default:
                continue;
            }
            int d = (CompareValue - condition.Value);
            if( ( condition.FromState == "" || condition.FromState == currentState.name ) && condition.ToState != currentState.name )
            {
                if( d * condition.Sign > 0 || (d == 0 && condition.Sign == 0) )
                {
                    ChangeState( condition.ToState );
                    currentCommand = null;//cancel command
                    break;
                }
            }
            else if( condition.ViceVersa && (condition.ToState == currentState.name) && condition.FromState != currentState.name )
            {
                if( d * condition.Sign < 0 && condition.FromState != "" )
                {
                    ChangeState( condition.FromState );
                    currentCommand = null;//cancel command
                    break;
                }
            }
        }
    }

    public override void TurnInit( CommandBase command )
    {
        base.TurnInit( command );
        HPCircle.OnTurnStart();
    }
    public void InvertInit()
    {
        PhysicDefend = 0;
        MagicDefend = 0;
        HealPercent = 0;
        TurnDamage = 0;
        HPCircle.OnTurnStart();
    }
    public EnemyCommand CheckCommand()
    {
        CheckState();

        currentCommand = currentState.pattern[turnCount % currentState.pattern.Length];
        ++turnCount;

        TurnInit( currentCommand );

        return currentCommand;
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
            turnCount = 0;
            if( currentState.DescribeText != "" )
            {
                ShortTextWindow shortText = (Instantiate( GameContext.EnemyConductor.shortTextWindowPrefab ) as GameObject).GetComponent<ShortTextWindow>();
                shortText.Initialize( currentState.DescribeText );
                shortText.transform.position = new Vector3( transform.position.x*0.7f, shortText.transform.position.y, shortText.transform.position.z );
                //shortText.transform.parent = transform;
            }
        }
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
        int oldHP = HitPoint;
 	    base.BeAttacked(attack, skill);
        int damage = oldHP - HitPoint;

        if( attack.isPhysic )
        {
            switch( Speceis )
            {
            case EnemySpecies.Human:
            case EnemySpecies.Thing:
            case EnemySpecies.Fairy:
            case EnemySpecies.Jewel:
                lastDamageResult = ActionResult.PhysicDamage;
                break;
            case EnemySpecies.Spirit:
            case EnemySpecies.Dragon:
                lastDamageResult = ActionResult.PhysicGoodDamage;
                break;
            case EnemySpecies.Beast:
                lastDamageResult = ActionResult.PhysicBadDamage;
                break;
            case EnemySpecies.Weather:
                break;
            }
            SEPlayer.Play( lastDamageResult, skill.OwnerCharacter, damage );
        }
        else
        {
            switch( Speceis )
            {
            case EnemySpecies.Human:
            case EnemySpecies.Thing:
            case EnemySpecies.Spirit:
            case EnemySpecies.Weather:
                lastDamageResult = ActionResult.MagicDamage;
                break;
            case EnemySpecies.Fairy:
            case EnemySpecies.Beast:
                lastDamageResult = ActionResult.MagicGoodDamage;
                break;
            case EnemySpecies.Dragon:
                lastDamageResult = ActionResult.MagicBadDamage;
                break;
            case EnemySpecies.Jewel:
                break;
            }
            SEPlayer.Play( lastDamageResult, skill.OwnerCharacter, damage );
        }
    }
    protected override void BeDamaged( int damage, Character ownerCharacter )
    {
        base.BeDamaged( damage, ownerCharacter );
        CreateDamageText( damage );
        HPCircle.OnDamage();
        if( HitPoint > 0 )
        {
            CheckStateOnDamage( damage );
        }
    }
    public override void Heal( HealModule heal )
    {
        int oldHitPoint = HitPoint;
        base.Heal( heal );
        CreateDamageText( -(HitPoint - oldHitPoint) );
        HPCircle.OnHeal();
        SEPlayer.Play( ActionResult.EnemyHeal, this, HitPoint - oldHitPoint );
    }
    public override void UpdateHealHP()
    {
        base.UpdateHealHP();
        HPCircle.OnUpdateHP();
    }

    public void OnBaseColorChanged( Color newColor )
    {
        renderer.material.color = newColor;
        if( HPCircle != null )
        {
            //HPCircle.CurrentCircle.GetComponent<SpriteRenderer>().color = newColor;
            HPCircle.OnBaseColorChanged();
        }
    }
    public void OnPlayerLose()
    {
        HPCircle.SetActive( false );
    }
    /*
    public void OnContinue()
    {
        HitPoint = MaxHP;
        currentState = States[initialStateIndex];
        turnCount = 0;
        HPCircle.SetActive( true );
        TurnInit();
    }
    */

    public override string ToString()
    {
        return DisplayName + "(" + currentState.name + ")";
    }

    [System.Serializable]
    public class BattleState
    {
        public string name;
        public EnemyCommand[] pattern;
        public string DescribeText;
    }

    public enum ConditionType
    {
        MyHP,
        PlayerHP,
        OneDamage,
        TurnDamage,
        TurnCout,
        EnemyCount,
        Count
    }
    [System.Serializable]
    public class StateChangeCondition
    {
        public ConditionType conditionType;
        public int Value;
        public int Sign;// -1:lower, 0:equal, 1:higher
        public string FromState;
        public string ToState;
        public bool ViceVersa;
    }
}
