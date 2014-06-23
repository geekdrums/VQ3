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
}

public class Enemy : Character
{
    static readonly float DamageTrembleTime = 0.025f;
    static readonly Timing StateChangeTiming = new Timing(3,0,0);

    public string DisplayName;
    public EnemySpecies Speceis;
    public List<BattleState> States;
    public StateChangeCondition[] conditions;
    public int VPtolerance;
    //public SpriteRenderer outlineSprite;

    public EnemyCommand currentCommand { get; protected set; }
    public int commandExecBar { get; protected set; }
    public BattleState currentState { get; protected set; }
    public BattleState oldState { get; protected set; }

    protected HPCircle HPCircle;
    protected int turnCount;
    protected ActionResult lastDamageResult;
    protected SpriteRenderer spriteRenderer;
    protected List<EnemyCommandIcon> commandIcons;
    protected ShortTextWindow shortText;

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

        foreach( StateChangeCondition condition in conditions )
        {
            condition.Parse();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();

        if( GameContext.CurrentState == GameState.Battle )
        {
            if( Music.IsJustChangedAt( commandExecBar ) && currentCommand != null && currentCommand.ShortText != "" )
            {
                if( shortText != null )
                {
                    Destroy( shortText.gameObject );
                    shortText = null;
                }
                if( !GameContext.VoxSystem.IsInverting )
                {
                    shortText = (Instantiate( GameContext.EnemyConductor.shortTextWindowPrefab ) as GameObject).GetComponent<ShortTextWindow>();
                    shortText.Initialize( currentCommand.ShortText );
                    shortText.transform.position = new Vector3( transform.position.x, shortText.transform.position.y, shortText.transform.position.z );
                    //shortText.transform.parent = transform;
                }
            }
            if( Music.IsJustChangedAt( StateChangeTiming )
                && (currentState.name != "Invert" || GameContext.VoxSystem.InvertTime == 1) )
            {
                oldState = currentState;
                CheckState();
                
                if( oldState == currentState && turnCount >= currentState.pattern.Length )
                {
                    for( int i = 0; i < commandIcons.Count; i++ )
                    {
                        if( i < currentState.pattern.Length )
                        {
                            commandIcons[i].SetIcon( GameContext.EnemyConductor.EnemyCommandIcons.Find( ( Sprite sprite ) => sprite.name == currentState.pattern[currentState.pattern.Length - i - 1].Icon.ToString().Replace( '_', '-' ) ) );
                            commandIcons[i].SetIndex( i, currentState.pattern.Length );
                        }
                        else
                        {
                            commandIcons[i].SetIcon( null );
                        }
                    }
                }
            }
            //else if( Music.Just > StateChangeTiming && ( oldState != null && oldState != currentState ) )
            //{
            //    float mt = (float)Music.MusicalTime - StateChangeTiming.totalUnit;
            //    float t = (mt >= 12 ? 1.0f : (2.0f - Mathf.Cos( Mathf.PI * mt / 4.0f )) / 2.0f);
            //    outlineSprite.color = Color.Lerp( oldState.color, currentState.color, t );
            //}
        }
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
            spriteRenderer.color = (damageTime % (DamageTrembleTime * 2) > DamageTrembleTime ? Color.clear : GameContext.EnemyConductor.baseColor);
            //outlineSprite.color = (damageTime % (DamageTrembleTime * 2) > DamageTrembleTime ? Color.clear : currentState.color);

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

    protected void CreateDamageText( int damage )
    {
        if( damage == 0 ) return;
        GameObject damageText = (Instantiate( GameContext.EnemyConductor.damageTextPrefab ) as GameObject);
        damageText.GetComponent<DamageText>().Initialize( damage, transform.position + Vector3.back * 3 + Random.onUnitSphere );
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
        HPCircle.OnTurnStart();
    }
    public void InvertInit()
    {
        PhysicDefend = 0;
        MagicDefend = 0;
        HealPercent = 0;
        TurnDamage = 0;
        HPCircle.OnTurnStart();
        currentCommand = null;

        oldState = currentState;
        BattleState invertState = States.Find( ( BattleState state ) => state.name == "Invert" );
        if( invertState == null )
        {
            invertState = new BattleState();
            invertState.name = "Invert";
            //invertState.color = Color.clear;
        }
        currentState = invertState;
    }
    public void OnRevert()
    {
        BattleState nextState = oldState;
        oldState = currentState;
        currentState = nextState;
    }
    public EnemyCommand CheckCommand()
    {
        if( currentState.pattern != null && currentState.pattern.Length > 0 )
        {
            currentCommand = currentState.pattern[turnCount % currentState.pattern.Length];
            TurnInit( currentCommand );
            for( int i = 0; i < commandIcons.Count; i++ )
            {
                commandIcons[i].SetIndex( i, currentState.pattern.Length - (turnCount % currentState.pattern.Length) - 1 );
            }
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
            turnCount = 0;
            for( int i = 0; i < commandIcons.Count; i++ )
            {
                if( i < currentState.pattern.Length )
                {
                    commandIcons[i].SetIcon( GameContext.EnemyConductor.EnemyCommandIcons.Find( ( Sprite sprite ) => sprite.name == currentState.pattern[currentState.pattern.Length - i - 1].Icon.ToString().Replace( '_', '-' )  ) );
                    commandIcons[i].SetIndex( i, currentState.pattern.Length );
                }
                else
                {
                    commandIcons[i].SetIcon( null );
                }
            }
            //if( currentState.DescribeText != "" )
            //{
            //    ShortTextWindow shortText = (Instantiate( GameContext.EnemyConductor.shortTextWindowPrefab ) as GameObject).GetComponent<ShortTextWindow>();
            //    shortText.Initialize( currentState.DescribeText );
            //    shortText.transform.position = new Vector3( transform.position.x*0.7f, shortText.transform.position.y, shortText.transform.position.z );
            //    //shortText.transform.parent = transform;
            //}
        }
    }
    public void InitState( string name )
    {
        commandIcons = new List<EnemyCommandIcon>();
        int maxCommandNNnum = 0;
        foreach( BattleState state in States )
        {
            maxCommandNNnum = Mathf.Max( maxCommandNNnum, state.pattern.Length );
        }
        for( int i = 0; i < maxCommandNNnum; i++ )
        {
            commandIcons.Add( (Instantiate( GameContext.EnemyConductor.commandIconPrefab ) as GameObject).GetComponent<EnemyCommandIcon>() );
            commandIcons[i].transform.parent = transform;
            commandIcons[i].transform.localPosition = Vector3.zero;
            commandIcons[i].SetIcon( null );
        }

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
        //if( HitPoint > 0 )
        //{
        //    CheckStateOnDamage( damage );
        //}
        if( HitPoint <= 0 )
        {
            if( shortText != null )
            {
                Destroy( shortText.gameObject );
                shortText = null;
            }
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
        if( spriteRenderer == null ) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = newColor;
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
        public string nextState;
        //public Color color = Color.clear;
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
