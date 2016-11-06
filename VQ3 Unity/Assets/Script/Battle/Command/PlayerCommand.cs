using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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
    protected static float ScaleCoeff = 0.0f;
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
	public BGAnimBase BGAnim;

	//
    // graphics editor params
    //
    public GameObject plane1;
    public GameObject plane2;
    public GameObject centerPlane;
    public GameObject centerIcon;
    public GameObject maskPlane;


	//
	// graphics properties
	//
	public EThemeColor themeColor;
	public List<CommandEdge> linkLines = new List<CommandEdge>();
    private GameObject DefPlane { get { return plane1; } }
    private GameObject HealPlane { get { return plane2; } }
    private GameObject EnhPlane { get { return centerPlane; } }
	private GameObject EnhIcon { get { return centerIcon; } }


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
	void Awake()
	{
		ValidateState();
	}

    void Start()
	{
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
		if( maskMat != null )
		{
			maskPlane.GetComponent<Renderer>().material = maskMat;
		}
    }

    public virtual void ValidateIcons()
    {
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
                EnhIcon.GetComponent<SpriteRenderer>().sprite = GameContext.PlayerConductor.EnhIcons[(int)EnhanceParamType.Time];
            }
            else if( iconStr.Contains( 'M' ) )
            {
                EnhIcon.GetComponent<SpriteRenderer>().sprite = GameContext.PlayerConductor.EnhIcons[(int)EnhanceParamType.Break];
            }
            else if( iconStr.Contains( 'S' ) )
            {
                EnhIcon.GetComponent<SpriteRenderer>().sprite = GameContext.PlayerConductor.EnhIcons[(int)EnhanceParamType.Defend];
            }
            else if( iconStr.Contains( 'R' ) )
            {
                EnhIcon.GetComponent<SpriteRenderer>().sprite = GameContext.PlayerConductor.EnhIcons[(int)EnhanceParamType.Heal];
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
        UpdateTransform();
    }

    protected virtual void UpdateTransform()
    {
		CommandGraph commandGraph = GetComponentInParent<CommandGraph>();
		float alpha = 0;
		float distance = (this.transform.position - SelectSpot).magnitude;
		if( GameContext.State == GameState.Setting || GameContext.State == GameState.Result )
		{
			if( state <= CommandState.Acquired )
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 1.0f;
			}
			else if( state <= CommandState.NotAcquired )
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
				alpha = 0.6f;
			}
			else//DontKnow
			{
				transform.localScale = Vector3.zero;
			}
		}
		else
		{
			if( state <= CommandState.Linked )
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * ( this is InvertCommand ? 1.0f : 1.2f );
			}
			else if( state <= CommandState.Acquired )
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
				alpha = Mathf.Clamp((distance + commandGraph.MaskStartPos) * commandGraph.MaskColorCoeff, 0.6f, 1.0f);
			}
			else//NotAcquired,DontKnow
			{
				transform.localScale = Vector3.zero;
			}
		}
		maskPlane.GetComponent<Renderer>().material.color = ColorManager.MakeAlpha(Color.black, alpha);
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
		if( GameContext.PlayerConductor.IsEclipse && Music.Just.Bar >= 2 ) return null;
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
	public virtual GameObject InstantiateIconObj(GameObject iconParent)
	{
		GameObject iconObj = Instantiate(gameObject) as GameObject;
		if( iconObj.transform.FindChild("nextRect") != null )
		{
			Destroy(iconObj.transform.FindChild("nextRect").gameObject);
		}
		if( iconObj.transform.FindChild("currentRect") != null )
		{
			Destroy(iconObj.transform.FindChild("currentRect").gameObject);
		}
		iconObj.transform.parent = iconParent.transform;
		iconObj.transform.localPosition = Vector3.zero;
		iconObj.transform.localScale = Vector3.one;
		iconObj.transform.localRotation = Quaternion.identity;
		iconObj.GetComponent<PlayerCommand>().enabled = false;
		return iconObj;
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

    #endregion


	//
	// menu actions
	//

	public void LevelUp()
	{
		++currentLevel;
		int oldSP = numSP;
		numSP = currentData.RequireSP;
		GameContext.PlayerConductor.RemainMemory -= (numSP - oldSP);
		state = CommandState.Acquired;
	}

	public void LevelDown()
	{
		--currentLevel;
		int oldSP = numSP;
		numSP = currentData == null ? 0 : currentData.RequireSP;
		state = currentData == null ? CommandState.NotAcquired : CommandState.Acquired;
		GameContext.PlayerConductor.RemainMemory += (oldSP - numSP);
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