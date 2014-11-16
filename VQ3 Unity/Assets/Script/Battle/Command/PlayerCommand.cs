using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum EStatusIcon
{
    AAA,
    AA,
    A,
    WWW,
    WW,
    W,
    LLL,
    LL,
    L,
    FFF,
    FF,
    F,
    HH,
    H,
    RR,
    R,
    _RR,
    _R,
    PP,
    P,
    _PP,
    _P,
    MM,
    M,
    _MM,
    _M,
    SS,
    S,
    _SS,
    _S,
    DD,
    D,
    E,
    V,
    rest,
    none,
    Count
}
public enum EEnhIcon
{
    Power,
    Magic,
    Shield,
    Regene
}

[ExecuteInEditMode]
public class PlayerCommand : CommandBase
{
    #region variables

    //
    // static properties
    //
    protected static Vector3 MaxScale = new Vector3( 0.24f, 0.24f, 0.24f );
    protected static float ScaleCoeff = 0.05f;
    protected static float MaskColorCoeff = 0.06f;
    protected static float MaskStartPos = 3.0f;
    protected static Color UnlinkedLineColor = ColorManager.MakeAlpha( Color.white, 0.2f );
    protected static Color PrelinkedLineColor = ColorManager.MakeAlpha( Color.white, 0.2f );
    protected static Vector3 SelectSpot
    {
        get
        {
            if( selectSpot_ == null )
            {
                selectSpot_ = UnityEngine.GameObject.Find( "SelectSpot" );
            }
            return selectSpot_.transform.position;
        }
    }
    protected static GameObject selectSpot_;
    //protected static Quaternion InitialRotation = Quaternion.Euler( new Vector3( 45, 90, -90 ) );

    //
    // editor params
    //
    public string MusicBlockName;
    public float latitude;
    public float longitude;
    public int AcquireLevel = 1;
    public string DescribeText;
	public string DescribeText2;
	public string AcquireText;
    public string NameText;
    public List<PlayerCommand> links;
    public List<EStatusIcon> icons;
    public float radius = 1.0f;

    //
    // graphics editor params
    //
    public GameObject plane1;
    public GameObject plane2;
    public GameObject centerPlane;
    public GameObject centerIcon;
    public GameObject maskPlane;
    public List<Sprite> EnhIcons;


    //
    // graphics properties
    //
    public List<LineRenderer> linkLines = new List<LineRenderer>();
    public void OnEdgeCreated( LineRenderer edge )
    {
        if( linkLines == null ) linkLines = new List<LineRenderer>();
        linkLines.Add( edge );
    }
    private GameObject DefPlane { get { return plane1; } }
    private GameObject HealPlane { get { return plane2; } }
    private GameObject EnhPlane { get { return centerPlane; } }
    private GameObject EnhIcon { get { return centerIcon; } }
    public EThemeColor themeColor { get; protected set; }

    //
    // battle related properties
    //
    public bool IsLinked { get; protected set; }
    public bool IsCurrent { get; protected set; }
    public bool IsSelected { get; protected set; }
    public bool IsAcquired { get; protected set; }
    public bool IsTargetSelectable { get { return _skillList.Find( ( Skill s ) => s.IsTargetSelectable ) != null; } }
    public PlayerCommand ParentCommand { get; protected set; }
    public int NumLoopVariations { get; protected set; }
    public bool IsLinkedTo( PlayerCommand node )
    {
        return links.Contains<PlayerCommand>( node );
    }

    //
    // battle related getters
    //
    public float GetAtk()
    {
        int sum = 0;
        foreach( Skill skill in SkillDictionary.Values )
        {
            if( skill.Actions == null ) skill.Parse();
            foreach( ActionSet a in skill.Actions )
            {
                if( a.GetModule<AttackModule>() != null )
                {
                    sum += a.GetModule<AttackModule>().Power;
                }
            }
        }
        return sum/4;
    }
    public float GetVP()
    {
        float sum = 0;
        foreach( Skill skill in SkillDictionary.Values )
        {
            if( skill.Actions == null ) skill.Parse();
            foreach( ActionSet a in skill.Actions )
            {
                if( a.GetModule<AttackModule>() != null )
                {
                    sum += a.GetModule<AttackModule>().VP;
                }
            }
        }
        return sum;
    }
    public float GetVT()
    {
        int sum = 0;
        foreach( Skill skill in SkillDictionary.Values )
        {
            if( skill.Actions == null ) skill.Parse();
            foreach( ActionSet a in skill.Actions )
            {
                if( a.GetModule<AttackModule>() != null )
                {
                    sum += a.GetModule<AttackModule>().VT;
                }
            }
        }
        return sum/4.0f;
    }
    public float GetHeal()
    {
        float sum = HealPercent;
        foreach( Skill skill in SkillDictionary.Values )
        {
            if( skill.Actions == null ) skill.Parse();
            foreach( ActionSet a in skill.Actions )
            {
                if( a.GetModule<HealModule>() != null )
                {
                    sum += a.GetModule<HealModule>().HealPoint;
                }
            }
        }
        return sum;
    }
    public float GetDefend()
    {
        return (PhysicDefend + MagicDefend) / 2;
    }
    public List<EnhanceModule> GetEnhModules()
    {
        List<EnhanceModule> enhModules = new List<EnhanceModule>();
        foreach( Skill skill in SkillDictionary.Values )
        {
            if( skill.Actions == null ) skill.Parse();
            foreach( ActionSet a in skill.Actions )
            {
                if( a.GetModule<EnhanceModule>() != null )
                {
                    enhModules.Add( a.GetModule<EnhanceModule>() );
                }
            }
        }
        return enhModules;
    }

    public Color GetAtkColor()
    {
        float atk = GetAtk();
        ThemeColor theme = ColorManager.GetThemeColor( themeColor );
        BaseColor baseColor = ColorManager.Base;
        if( atk <= 0 )
        {
            return baseColor.MiddleBack;
        }
        else if( atk <= 15 )
        {
            return theme.Shade;
        }
        else if( atk <= 30 )
        {
            return theme.Light;
        }
        else
        {
            return theme.Bright;
        }
    }
    public Color GetHealColor()
    {
        float heal = GetHeal();
        ThemeColor theme = ColorManager.GetThemeColor( themeColor );
        BaseColor baseColor = ColorManager.Base;
        if( heal <= 0 )
        {
            return baseColor.MiddleBack;
        }
        else if( heal <= 5 )
        {
            return theme.Shade;
        }
        else if( heal <= 30 )
        {
            return theme.Light;
        }
        else
        {
            return theme.Bright;
        }
    }
    public Color GetDefColor()
    {
        float def = GetDefend();
        ThemeColor theme = ColorManager.GetThemeColor( themeColor );
        BaseColor baseColor = ColorManager.Base;
        if( def <= 0 )
        {
            return baseColor.MiddleBack;
        }
        else if( def <= 20 )
        {
            return theme.Shade;
        }
        else if( def <= 50 )
        {
            return theme.Light;
        }
        else
        {
            return theme.Bright;
        }
    }
    public Color GetVPColor()
    {
        float vp = GetVP();
        ThemeColor theme = ColorManager.GetThemeColor( themeColor );
        BaseColor baseColor = ColorManager.Base;
        if( vp <= 0 )
        {
            return baseColor.MiddleBack;
        }
        else if( vp <= 15 )
        {
            return theme.Shade;
        }
        else if( vp <= 30 )
        {
            return theme.Light;
        }
        else
        {
            return theme.Bright;
        }
    }
    public Color GetVTColor()
    {
        float vt = GetVT();
        ThemeColor theme = ColorManager.GetThemeColor( themeColor );
        BaseColor baseColor = ColorManager.Base;
        if( vt <= 0 )
        {
            return baseColor.MiddleBack;
        }
        else if( vt <= 15 )
        {
            return theme.Shade;
        }
        else if( vt <= 30 )
        {
            return theme.Light;
        }
        else
        {
            return theme.Bright;
        }
    }
    public Color GetEnhColor()
    {
        List<EnhanceModule> enh = GetEnhModules();
        ThemeColor theme = ColorManager.GetThemeColor( themeColor );
        BaseColor baseColor = ColorManager.Base;
        if( enh.Count == 0 )
        {
            return baseColor.Back;
        }
        else if( enh.Count == 1 && enh[0].phase == 1 )
        {
            return theme.Light;
        }
        else
        {
            return theme.Bright;
        }
    }
    public Sprite GetEnhIconSprite()
    {
        if( EnhIcon.GetComponent<SpriteRenderer>() != null )
        {
            return EnhIcon.GetComponent<SpriteRenderer>().sprite;
        }
        return null;
    }

    #endregion

    #region Unity functions
    //
    // initialize
    //
    void Start()
    {
        Parse();
        IsLinked = false;
        IsCurrent = false;
        IsSelected = false;
        ValidatePosition();
        ValidateIcons();
        ValidateColor();
    }

    public override void Parse()
    {
        base.Parse();
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        //textMesh = GetComponent<TextMesh>();
        IsLinked = true;
        IsAcquired = true;//AcquireLevel <= GameContext.PlayerConductor.Level;
        if( !IsAcquired )
        {
            SetColor( Color.clear );
        }
        NumLoopVariations = 1;
        foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
        {
            if( command != this )
            {
                command.ParentCommand = this;
                int index = -1;
                if( int.TryParse( command.name.Replace( this.name, "" ), out index ) )
                {
                    NumLoopVariations++;
                }
            }
        }
    }
    
    //
    // validate
    //
    public virtual void ValidatePosition()
    {
        if( GetComponentInParent<CommandGraph>() == null ) return;
        transform.localPosition = Quaternion.AngleAxis( longitude, Vector3.down ) * Quaternion.AngleAxis( latitude, Vector3.right ) * Vector3.back * GetComponentInParent<CommandGraph>().SphereRadius;
        transform.localScale = MaxScale * (1.0f - (this.transform.position - SelectSpot).magnitude * ScaleCoeff);
        transform.rotation = Quaternion.identity;
    }

    public virtual void ValidateColor()
    {
        float alpha = (transform.localPosition.z + MaskStartPos) * MaskColorCoeff;
        Material maskMat = new Material( Shader.Find( "Transparent/Diffuse" ) );
        maskMat.hideFlags = HideFlags.DontSave;
        maskMat.color = ColorManager.MakeAlpha( Color.black, alpha );
        maskMat.name = "maskMat";
        maskPlane.renderer.material = maskMat;

        foreach( LineRenderer line in linkLines )
        {
            Color lineColor = ColorManager.MakeAlpha( Color.white, 1.0f - alpha );
            line.SetColors( lineColor, lineColor );
        }
    }

    public virtual void ValidateIcons()
    {
        if( GetComponentInParent<CommandGraph>() == null ) return;
        string iconStr = "";
        foreach( EStatusIcon ic in icons ) iconStr += ic.ToString();

        themeColor = EThemeColor.White;
        if( iconStr.Contains( 'A' ) )
        {
            themeColor = EThemeColor.Blue;
        }
        else if( iconStr.Contains( 'W' ) )
        {
            themeColor = EThemeColor.Green;
        }
        else if( iconStr.Contains( 'F' ) )
        {
            themeColor = EThemeColor.Red;
        }
        else if( iconStr.Contains( 'L' ) )
        {
            themeColor = EThemeColor.Yellow;
        }

		//Material baseMat = new Material( Shader.Find( "Diffuse" ) );
		//baseMat.hideFlags = HideFlags.DontSave;
		//baseMat.color = ColorManager.GetThemeColor( themeColor ).Bright;
		//baseMat.name = "baseMat";
		//this.renderer.material = baseMat;
		this.GetComponent<MidairPrimitive>().SetColor(ColorManager.GetThemeColor(themeColor).Bright);

        if( iconStr.Contains( 'D' ) )
        {
            DefPlane.SetActive( true );
            Material defMat = new Material( Shader.Find( "Diffuse" ) );
            defMat.hideFlags = HideFlags.DontSave;
            defMat.color = ColorManager.GetThemeColor( themeColor ).Shade;
            defMat.name = "defMat";
            DefPlane.renderer.material = defMat;
        }
        else
        {
            DefPlane.SetActive( false );
        }
        if( iconStr.Contains( 'H' ) )
        {
            HealPlane.SetActive( true );
            Material healMat = new Material( Shader.Find( "Diffuse" ) );
            healMat.hideFlags = HideFlags.DontSave;
            healMat.color = ColorManager.GetThemeColor( themeColor ).Light;
            healMat.name = "healMat";
            HealPlane.renderer.material = healMat;
        }
        else
        {
            HealPlane.SetActive( false );
        }
        if( iconStr.Contains( 'P' ) || iconStr.Contains( 'M' ) || iconStr.Contains( 'S' ) || iconStr.Contains( 'R' ) )
        {
            EnhPlane.SetActive( true );
            if( iconStr.Contains( 'P' ) )
            {
                EnhIcon.GetComponent<SpriteRenderer>().sprite = EnhIcons[(int)EEnhIcon.Power];
            }
            else if( iconStr.Contains( 'M' ) )
            {
                EnhIcon.GetComponent<SpriteRenderer>().sprite = EnhIcons[(int)EEnhIcon.Magic];
            }
            else if( iconStr.Contains( 'S' ) )
            {
                EnhIcon.GetComponent<SpriteRenderer>().sprite = EnhIcons[(int)EEnhIcon.Shield];
            }
            else if( iconStr.Contains( 'R' ) )
            {
                EnhIcon.GetComponent<SpriteRenderer>().sprite = EnhIcons[(int)EEnhIcon.Regene];
            }
            if( themeColor != EThemeColor.White )
            {
                EnhIcon.GetComponent<SpriteRenderer>().color = ColorManager.GetThemeColor( themeColor ).Bright;
            }
            else
            {
                EnhIcon.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }
        else
        {
            EnhPlane.SetActive( false );
        }
    }

    void OnValidate()
    {
        ValidatePosition();
    }

    //
    // update
    //
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
		if( ParentCommand != null ) return;
        UpdateIcon();
    }

    protected void UpdateIcon()
    {
        if( Music.isJustChanged )
		{
			CommandGraph commandGraph = GetComponentInParent<CommandGraph>();
			float alpha = 0;
			float distance = (this.transform.position - SelectSpot).magnitude;
			if( IsLinked )
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 1.2f;
			}
			else
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
				alpha = Mathf.Clamp( (distance + commandGraph.MaskStartPos) * commandGraph.MaskColorCoeff, 0.7f, 1.0f );
			}
            maskPlane.renderer.material.color = ColorManager.MakeAlpha( Color.black, alpha );
        }
        transform.rotation = Quaternion.identity;
    }

    #endregion

    #region battle functions

    //
    // battle utilities
    //
    public bool IsUsable()
    {
        return IsAcquired && IsLinked;
    }
    public virtual string GetBlockName()
    {
        return MusicBlockName;
    }
    public virtual GameObject GetCurrentSkill()
    {
        if( GameContext.VoxSystem.state == VoxState.Eclipse && Music.Just.bar >= 2 ) return null;
        return SkillDictionary.ContainsKey( Music.Just.totalUnit ) ? SkillDictionary[Music.Just.totalUnit].gameObject : null;
    }
    public IEnumerable<PlayerCommand> LinkedCommands
    {
        get
        {
            //if( ParentStrategy != null )
            //{
            //    foreach( PlayerCommand c in ParentStrategy.LinkedCommands )
            //    {
            //        yield return c;
            //    }
            //}
            foreach( PlayerCommand link in links )
            {
                yield return link;
                //Strategy linkedStrategy = link as Strategy;
                //PlayerCommand linkedCommand = link as PlayerCommand;
                /*if( linkedStrategy != null )
                {
                    foreach( PlayerCommand c in linkedStrategy.Commands )
                    {
                        yield return c;
                    }
                }
                else */
                //if( linkedCommand != null )
                //{
                //    yield return linkedCommand;
                //}
            }
        }
    }
    public PlayerCommand FindVariation( string suffix )
    {
        return GetComponentsInChildren<PlayerCommand>().First<PlayerCommand>( ( PlayerCommand command ) => command.name == this.name + suffix );
    }
    public PlayerCommand FindLoopVariation( int numLoop )
    {
        int index = numLoop % NumLoopVariations;
        return (index == 0 ? this : FindVariation( index.ToString() ));
    }

    //
    // battle actions
    //
    public virtual void SetLink( bool linked )
    {
        IsCurrent = false;
        IsSelected = false;
        IsLinked = linked;
        if( IsUsable() )
        {
            SetColor( Color.black );
            foreach( LineRenderer line in linkLines )
            {
                line.SetColors( PrelinkedLineColor, PrelinkedLineColor );
                line.SetWidth( 0.1f, 0.1f );
            }
        }
        else
        {
            if( IsAcquired )
            {
                SetColor( Color.gray );
            }
            foreach( LineRenderer line in linkLines )
            {
                line.SetColors( UnlinkedLineColor, UnlinkedLineColor );
                line.SetWidth( 0.05f, 0.05f );
            }
        }
    }
    public void SetCurrent()
    {
        if( IsAcquired )
        {
            IsCurrent = true;
            SetColor( Color.red );
            foreach( LineRenderer line in linkLines )
            {
                line.SetColors( Color.white, Color.white );
                line.SetWidth( 0.3f, 0.3f );
            }
        }
    }
    public void Select()
    {
        if( IsAcquired )
        {
            IsSelected = true;
            SetColor( Color.magenta );
        }
    }
    public void Deselect()
    {
        if( IsAcquired )
        {
            IsSelected = false;
            SetColor( IsCurrent ? Color.red : Color.black );
        }
    }
    public void Acquire()
    {
        IsAcquired = true;
        SetColor( Color.gray );
    }
    public void Forget()
    {
        IsSelected = false;
        IsAcquired = false;
        SetColor( Color.clear );
    }
    public void SetPush( bool isPushing )
    {
        if( isPushing ) SetColor( Color.white );
        else SetColor( IsCurrent ? Color.red : ( IsSelected ? Color.magenta : Color.black ) );
    }

    //
    // other utilities
    //
    protected virtual void SetColor( Color color )
    {
        //if( textMesh != null )
        //{
        //    textMesh.color = color;
        //}
    }

    #endregion
}