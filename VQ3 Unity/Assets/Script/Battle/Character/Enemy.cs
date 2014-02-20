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
    static readonly float DamageTrembleTime = 0.05f;

    public string DisplayName;
    public EnemySpecies Speceis;
    public EnemyCommand[] Commands;
    public StateChangeCondition[] conditions;
    public List<string> StateNames;
    public string currentState = "Default";

    public EnemyCommand currentCommand { get; protected set; }
    protected int commandExecBar;

    protected HPCircle HPCircle;

    // Use this for initialization
    protected virtual void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
        foreach( EnemyCommand c in Commands )
        {
            c.Parse();
        }
        HPCircle = (Instantiate( GameContext.EnemyConductor.HPCirclePrefab, transform.position + Vector3.down * 5.5f, Quaternion.identity ) as GameObject).GetComponent<HPCircle>();
        HPCircle.transform.parent = transform;
        HPCircle.transform.localScale *= Mathf.Sqrt( (float)HitPoint / (float)GameContext.EnemyConductor.baseHP );
        HPCircle.Initialize( this );
        HPCircle.OnTurnStart();
        initialPosition = transform.position;
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
            if( (int)(damageTime / DamageTrembleTime) != (int)((damageTime + Time.deltaTime) / DamageTrembleTime) )
            {
                transform.position = initialPosition + Random.insideUnitSphere * Mathf.Clamp( damageTime, 0.1f, 2.0f ) * 1.3f;
            }
            damageTime -= Time.deltaTime;
            if( damageTime <= 0 )
            {
                transform.position = initialPosition;
            }
            renderer.material.color = (damageTime % (DamageTrembleTime*2) > DamageTrembleTime ? Color.clear : GameContext.EnemyConductor.baseColor);
            damageTime -= Time.deltaTime;
            if( damageTime <= 0 )
            {
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
        if( currentCommand != null && currentCommand.nextState != "" ) currentState = currentCommand.nextState;
        else
        {
            foreach( StateChangeCondition condition in conditions )
            {
                int CompareValue = 0;
                switch( condition.conditionType )
                {
                case ConditionType.MyHP:
                    CompareValue = HitPoint;
                    break;
                case ConditionType.PlayerHP:
                    CompareValue = GameContext.PlayerConductor.PlayerHP;
                    break;
                default:
                    continue;
                }
                int d = (CompareValue - condition.Value);
                if( condition.FromState == "" || condition.FromState == currentState )
                {
                    if( d * condition.Sign > 0 || (d == 0 && condition.Sign == 0) )
                    {
                        currentState = condition.ToState;
                        break;
                    }
                }
                else if( condition.ViceVersa && (condition.ToState == currentState) )
                {
                    if( d * condition.Sign < 0 && condition.FromState != "" )
                    {
                        currentState = condition.FromState;
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
            if( condition.FromState == "" || condition.FromState == currentState )
            {
                if( d * condition.Sign > 0 || (d == 0 && condition.Sign == 0) )
                {
                    currentState = condition.ToState;
                    currentCommand = null;//cancel command
                    break;
                }
            }
        }
    }

    public EnemyCommand CheckCommand()
    {
        TurnInit();
        CheckState();
        HPCircle.OnTurnStart();

        if( currentCommand != null && currentCommand.nextCommand != null )
        {
            currentCommand = currentCommand.nextCommand;
        }
        else
        {
            List<int> probabilityies = new List<int>();
            int probabilitySum = 0;
            foreach( EnemyCommand c in Commands )
            {
                probabilitySum += c.GetProbability( StateNames.Count > 0 ? StateNames.IndexOf( currentState ) : 0 );
                probabilityies.Add( probabilitySum );
            }
            int rand = Random.Range( 0, probabilitySum );
            for( int i = 0; i < probabilityies.Count; i++ )
            {
                if( rand < probabilityies[i] )
                {
                    currentCommand = Commands[i];
                    break;
                }
            }
        }

        if( currentCommand != null && currentCommand.currentState != "" ) currentState = currentCommand.currentState;
        return currentCommand;
    }
    public void SetExecBar( int bar )
    {
        commandExecBar = bar;
    }
    public void CheckSkill()
    {
        Skill skill = (currentCommand != null ? currentCommand.GetCurrentSkill( commandExecBar ) : null);
        if( skill != null )
        {
            Skill objSkill = (Skill)Instantiate( skill, new Vector3(), transform.rotation );
            objSkill.SetOwner( this );
            GameContext.BattleConductor.ExecSkill( objSkill );
            if( objSkill.DescribeText != "" ) TextWindow.AddMessage( DisplayName + "@" + objSkill.DescribeText );
        }
    }

    protected override void BeDamaged( int damage, Character ownerCharacter )
    {
        base.BeDamaged( damage, ownerCharacter );
        CreateDamageText( damage );
        CheckStateOnDamage( damage );
        HPCircle.OnDamage();
    }
    public override void Heal( HealModule heal )
    {
        int oldHitPoint = HitPoint;
        base.Heal( heal );
        CreateDamageText( -(HitPoint - oldHitPoint) );
        HPCircle.OnHeal();
    }

    public void OnBaseColorChanged( Color newColor )
    {
        renderer.material.color = newColor;
    }

    public override string ToString()
    {
        return name + "(" + currentState + ")";
    }


    public enum ConditionType
    {
        MyHP,
        PlayerHP,
        OneDamage,
        TurnDamage,
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
