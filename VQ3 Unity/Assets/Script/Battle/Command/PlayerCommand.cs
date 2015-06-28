using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum EEnhIcon
{
    Power,
    Magic,
    Shield,
    Regene
}
public enum CommandState
{
	Current,
	Linked,
	Acquired,
	NotAcquired,
	DontKnow,
}

[ExecuteInEditMode]
public class PlayerCommand : MonoBehaviour
{
    #region variables

    //
    // static properties
    //
    protected static Vector3 MaxScale = new Vector3( 0.24f, 0.24f, 0.24f );
    protected static float ScaleCoeff = 0.05f;
    protected static float MaskColorCoeff = 0.06f;
    protected static float MaskStartPos = 3.0f;
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

    //
    // editor params
    //
	public List<PlayerCommandData> commandData;
	public int acquireLevel;
	public string nameText;
	public string iconStr;
	public float latitude;
	public float longitude;
	public float radius = 1.0f;
	public List<PlayerCommand> links;

	//
    // graphics editor params
    //
    public GameObject plane1;
    public GameObject plane2;
    public GameObject centerPlane;
    public GameObject centerIcon;
    public GameObject maskPlane;
    public List<Sprite> EnhIcons;
	public CounterSprite levelCounter;


    //
    // graphics properties
	//
	public EThemeColor themeColor { get; protected set; }
	public List<CommandEdge> linkLines = new List<CommandEdge>();
    private GameObject DefPlane { get { return plane1; } }
    private GameObject HealPlane { get { return plane2; } }
    private GameObject EnhPlane { get { return centerPlane; } }
	private GameObject EnhIcon { get { return centerIcon; } }


	//
	// current command properties
	//
	public float GetAtk()
	{
		if( currentData != null ) return currentData.GetAtk();
		else return 0;
	}
	public float GetVP()
	{
		if( currentData != null ) return currentData.GetVP();
		else return 0;
	}
	public float GetVT()
	{
		if( currentData != null ) return currentData.GetVT();
		else return 0;
	}
	public float GetHeal()
	{
		if( currentData != null ) return currentData.GetHeal();
		else return 0;
	}
	public float GetDefend()
	{
		if( currentData != null ) return currentData.GetDefend();
		else return 0;
	}
	public List<EnhanceModule> GetEnhModules()
	{
		if( currentData != null ) return currentData.GetEnhModules();
		else return null;
	}
	public Color GetAtkColor()
	{
		float atk = GetAtk();
		ThemeColor theme = ColorManager.GetThemeColor(themeColor);
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
		ThemeColor theme = ColorManager.GetThemeColor(themeColor);
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
		ThemeColor theme = ColorManager.GetThemeColor(themeColor);
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
		ThemeColor theme = ColorManager.GetThemeColor(themeColor);
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
		ThemeColor theme = ColorManager.GetThemeColor(themeColor);
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
		ThemeColor theme = ColorManager.GetThemeColor(themeColor);
		BaseColor baseColor = ColorManager.Base;
		if( enh == null || enh.Count == 0 )
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

	//
	// star params
	//
	public int numSP = 0;
	public int currentLevel = 0;
	public PlayerCommandData currentData { get { return currentLevel == 0 ? null : commandData[currentLevel-1]; } }

    //
    // battle related properties
    //
	public CommandState state { get; protected set; }
    public bool IsLinkedTo( PlayerCommand node )
    {
        return links.Contains<PlayerCommand>( node );
    }

    #endregion

    #region Unity functions
    //
    // initialize
    //
    void Start()
	{
		ValidateState();
        ValidatePosition();
        ValidateIcons();
		ValidateColor();
    }
    
    //
    // validate
    //
	public virtual void ValidateState()
	{
#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
		state = CommandState.DontKnow;
		if( acquireLevel <= GameContext.PlayerConductor.Level )
		{
			state = CommandState.NotAcquired;
			if( 0 < numSP || commandData[0].RequireSP == 0 )
			{
				state = CommandState.Acquired;
				currentLevel = 0;
				for( int i=0;i<commandData.Count;++i)
				{
					if( commandData[i].RequireSP <= numSP )
					{
						currentLevel = i+1;
					}
				}
			}
		}
		levelCounter.Count = currentLevel;
		levelCounter.CounterColor = currentLevel == 0 ? ColorManager.Base.Shade : Color.black;
	}

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
        maskPlane.GetComponent<Renderer>().material = maskMat;
    }

    public virtual void ValidateIcons()
    {
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

		this.GetComponent<MidairPrimitive>().SetColor(ColorManager.GetThemeColor(themeColor).Bright);

        if( iconStr.Contains( 'D' ) )
        {
            DefPlane.SetActive( true );
            Material defMat = new Material( Shader.Find( "Diffuse" ) );
            defMat.hideFlags = HideFlags.DontSave;
            defMat.color = ColorManager.GetThemeColor( themeColor ).Shade;
            defMat.name = "defMat";
            DefPlane.GetComponent<Renderer>().material = defMat;
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
            HealPlane.GetComponent<Renderer>().material = healMat;
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

    public void OnValidate()
    {
        ValidatePosition();
    }

	public void OnEdgeCreated( CommandEdge edge )
	{
		if( linkLines == null ) linkLines = new List<CommandEdge>();
		linkLines.Add(edge);
		edge.SetParent(this);
	}

    //
    // update
    //
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        UpdateIcon();
    }

    protected virtual void UpdateIcon()
    {
		if( Music.IsJustChanged )
		{
			CommandGraph commandGraph = GetComponentInParent<CommandGraph>();
			float alpha = 0;
			float distance = (this.transform.position - SelectSpot).magnitude;
			if( GameContext.CurrentState == GameState.SetMenu )
			{
				if( state <= CommandState.Acquired )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 1.0f;
				}
				else if( state <= CommandState.NotAcquired )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
					alpha = 0.85f;
				}
				else//DontKnow
				{
					transform.localScale = Vector3.zero;
				}
				levelCounter.transform.localScale = Vector3.one;
			}
			else if( GameContext.CurrentState != GameState.Result )
			{
				if( state <= CommandState.Linked )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 1.2f;
				}
				else if( state <= CommandState.Acquired )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
					alpha = Mathf.Clamp((distance + commandGraph.MaskStartPos) * commandGraph.MaskColorCoeff, 0.7f, 1.0f);
				}
				else//NotAcquired,DontKnow
				{
					transform.localScale = Vector3.zero;
				}
				levelCounter.transform.localScale = Vector3.zero;
			}
			maskPlane.GetComponent<Renderer>().material.color = ColorManager.MakeAlpha(Color.black, alpha);
		}
        transform.rotation = Quaternion.identity;
    }

    #endregion

    #region battle functions

    //
    // battle utilities
    //
    public virtual bool IsUsable()
    {
		return state <= CommandState.Acquired;
    }
    public virtual string GetBlockName()
    {
        return currentData.MusicBlockName;
    }
    public virtual GameObject GetCurrentSkill()
    {
        if( GameContext.VoxSystem.state == VoxState.Eclipse && Music.Just.Bar >= 2 ) return null;
		return currentData.SkillDictionary.ContainsKey(Music.Just.MusicalTime) ? currentData.SkillDictionary[Music.Just.MusicalTime].gameObject : null;
    }
    public IEnumerable<PlayerCommand> LinkedCommands
    {
        get
        {
            foreach( PlayerCommand link in links )
            {
                yield return link;
            }
        }
    }

    //
    // battle actions
    //
    public virtual void SetLink( bool linked )
    {
		if( state <= CommandState.Acquired )
		{
			state = linked ? CommandState.Linked : CommandState.Acquired;
			if( IsUsable() )
			{
				foreach( CommandEdge line in linkLines )
				{
					if( line.State > EdgeState.PreLinked && line.IsUsable )
					{
						line.State = EdgeState.PreLinked;
					}
				}
			}
			else
			{
				foreach( CommandEdge line in linkLines )
				{
					if( line.State > EdgeState.Unlinked && line.IsUsable )
					{
						line.State = EdgeState.Unlinked;
					}
				}
			}
		}
    }
    public void SetCurrent()
    {
		state = CommandState.Current;
		foreach( CommandEdge line in linkLines )
		{
			if( line.State > EdgeState.Linked && line.IsUsable )
			{
				line.State = EdgeState.Linked;
			}
        }
    }
    public void Select()
	{
    }
    public void Deselect()
	{
    }
    public void Acquire()
	{
		state = CommandState.NotAcquired;
		ValidateState();
		transform.localScale = Vector3.one * 0.16f;
		maskPlane.GetComponent<Renderer>().material.color = Color.clear;
		foreach( CommandEdge line in linkLines )
		{
			if( line.GetOtherSide(this).state != CommandState.DontKnow )
			{
				line.State = EdgeState.Unacquired;
			}
		}
    }
    public void Forget()
    {
    }
    public void SetPush( bool isPushing )
    {
    }

    #endregion


	//
	// menu actions
	//

	public void LevelUp()
	{
		++currentLevel;
		levelCounter.Count = currentLevel;
		levelCounter.CounterColor = Color.black;
		int oldSP = numSP;
		numSP = currentData.RequireSP;
		GameContext.PlayerConductor.RemainSP -= (numSP - oldSP);
		state = CommandState.Acquired;
	}

	public void LevelDown()
	{
		--currentLevel;
		levelCounter.Count = currentLevel;
		levelCounter.CounterColor = currentLevel == 0 ? ColorManager.Base.Shade : Color.black;
		int oldSP = numSP;
		numSP = currentData == null ? 0 : currentData.RequireSP;
		state = currentData == null ? CommandState.NotAcquired : CommandState.Acquired;
		GameContext.PlayerConductor.RemainSP += (oldSP - numSP);
	}

	public void SetLinkedFromIntro()
	{
		foreach( CommandEdge commandEdge in linkLines )
		{
			if( commandEdge.State == EdgeState.Unacquired )
			{
				if( commandEdge.GetOtherSide(this).currentLevel > 0 )
				{
					commandEdge.State = EdgeState.Linked;
					commandEdge.GetOtherSide(this).SetLinkedFromIntro();
				}
			}
		}
	}
}