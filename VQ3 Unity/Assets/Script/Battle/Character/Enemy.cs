using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Character
{
    public string DisplayName;
    public EnemyCommand[] Commands;
    public StateChangeCondition[] conditions;
    public List<string> StateNames;
    public string currentState = "Default";

    public EnemyCommand currentCommand { get; protected set; }
    protected int commandExecBar;

    protected SpriteRenderer HPCircle;
    protected Vector3 baseHPCircleScale;

    protected float targetHPCircleSize;
    protected Color targetHPCircleColor;

    // Use this for initialization
    protected virtual void Start()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        HPCircle = GetComponentsInChildren<SpriteRenderer>()[1];
        if( HPCircle != null )
        {
            baseHPCircleScale = HPCircle.transform.localScale / Mathf.Sqrt( GameContext.EnemyConductor.baseHP );
            targetHPCircleSize = Mathf.Sqrt( MaxHP );
            HPCircle.transform.localScale = baseHPCircleScale * targetHPCircleSize;
            targetHPCircleColor = GameContext.EnemyConductor.baseColor;
            HPCircle.color = targetHPCircleColor;
        }
        foreach( EnemyCommand c in Commands )
        {
            c.Parse();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if( damageTime > 0 )
        {
            renderer.material.color = (damageTime % 0.1f > 0.05f ? Color.clear : targetHPCircleColor);
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
                    renderer.material.color = targetHPCircleColor;
                }
            }
        }
        UpdateAnimation();
    }

    protected virtual void UpdateAnimation()
    {
        HPCircle.transform.localScale = Vector3.Lerp( HPCircle.transform.localScale, baseHPCircleScale * targetHPCircleSize, 0.1f );
        HPCircle.color = Color.Lerp( targetHPCircleColor, HPCircle.color, 0.1f );
    }

    protected void CreateDamageText( int damage )
    {
        GameObject damageText = (Instantiate( GameContext.EnemyConductor.damageTextPrefab, transform.position, Quaternion.identity ) as GameObject);
        damageText.GetComponent<TextMesh>().text = Mathf.Abs( damage ).ToString();
        if( damage < 0 ) damageText.GetComponent<TextMesh>().color = Color.green;
        damageText.transform.position += Vector3.back * 3;
        damageText.transform.parent = transform;
    }
    protected void UpdateHPCircle()
    {
        if( HPCircle != null )
        {
            targetHPCircleSize = Mathf.Sqrt( HitPoint );
            targetHPCircleColor = Color.Lerp( Color.clear, GameContext.EnemyConductor.baseColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) );
            renderer.material.color = targetHPCircleColor;
        }
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

    protected override void BeDamaged( int damage, Skill skill )
    {
        int d = Mathf.Max( 0, damage );
        base.BeDamaged( d, skill );
        UpdateHPCircle();
        CreateDamageText( d );
        CheckStateOnDamage( d );
    }
    public override void Heal( HealModule heal )
    {
        int h = Mathf.Min( MaxHP - HitPoint, (int)(MaxHP * ((float)heal.HealPoint / 100.0f)) );
        base.Heal( heal );
        CreateDamageText( -h );
        UpdateHPCircle();
    }

    public void OnBaseColorChanged( Color newColor )
    {
        renderer.material.color = newColor;
        targetHPCircleColor = Color.Lerp( Color.clear, newColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) );
        HPCircle.color = targetHPCircleColor;
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
