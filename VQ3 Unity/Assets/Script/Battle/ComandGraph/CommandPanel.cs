using UnityEngine;
using System.Collections;

public class CommandPanel : MonoBehaviour
{

	#region editor params

	public MidairPrimitive Frame;
    public MidairPrimitive BaseRect;
    public MidairPrimitive ATRect;
    public MidairPrimitive HLRect;
    public MidairPrimitive DFRect;
    public MidairPrimitive VTRect;
    public MidairPrimitive VPRect;
	public MidairPrimitive ENHRect;
	public MidairPrimitive RevertRect;
	public MidairPrimitive RevertCircleEdge;
	public MidairPrimitive RevertArc;
	public MidairPrimitive InvertRect;
    public CounterSprite ATCount;
    public CounterSprite HLCount;
    public CounterSprite DFCount;
    public CounterSprite VTCount;
	public CounterSprite VPCount;
	public CounterSprite InvertAtCount;
	public CounterSprite LVCount;
	public SpriteRenderer ENHIcon;
    public TextMesh NameText;
	public TextMesh OKText;
	public TextMesh AtText;
	public TextMesh InvertAtText;
	public TextMesh LVText;
    public MidairPrimitive LeftButton;
    public MidairPrimitive RightButton;
    public MidairPrimitive PanelMask;
    public MidairPrimitive GraphMask;
    public GameObject HitPlane;

	public CommandExplanation CommandExp;

	#endregion


	float RingSize { get { return GameContext.PlayerConductor.CommandGraph.AxisRing.Radius; } }
	Color TextColor { get { return (command_ is RevertCommand ? Color.black : Color.white); } }
	Color ButtonColor { get { return (command_ is InvertCommand || command_ is RevertCommand ? Color.black : Color.white); } }

    Vector3 initialPosition_;
    PlayerCommand command_;

    public State state = State.Hide;
    ButtonType buttonType = ButtonType.None;
	bool enableLeft_, enableRight_;
    public enum State
    {
        HideAnim,
        Hide,
        ShowAnim,
        Show,
        DecideAnim,
        Decided,
		ExecuteAnim,
		CancelAnim,
    };
    enum ButtonType
    {
        None,
        OK,
        Left,
        Right,
    };

	// Use this for initialization
	void Start () {
        initialPosition_ = transform.position;
        transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update()
	{
        switch( state )
        {
        case State.HideAnim:
            if( GetComponent<Animation>().isPlaying == false )
            {
                EnterState( State.Hide );
            }
            break;
        case State.Hide:
            break;
        case State.ShowAnim:
            transform.position = Vector3.Lerp( transform.position, initialPosition_, 0.3f );
            if( GetComponent<Animation>().isPlaying == false )
            {
                EnterState( State.Show );
            }
            break;
        case State.Show:
			UpdateShow();
            break;
        case State.DecideAnim:
            if( GetComponent<Animation>().isPlaying == false )
            {
                EnterState( State.Decided );
            }
            break;
		case State.Decided:
			{
				if( Input.GetMouseButtonUp(0) && Music.Just < CommandGraph.AllowInputEnd )
				{
					Ray ray = GameContext.MainCamera.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);
					if( hit.collider == HitPlane.GetComponent<Collider>() )
					{
						GameContext.PlayerConductor.CommandGraph.Deselect();
						OKText.GetComponent<Renderer>().enabled = false;
						EnterState(State.CancelAnim);
						Frame.SetTargetSize(0);
						Frame.SetTargetColor(Color.clear);
						GraphMask.SetTargetColor(Color.clear);
					}
				}
				else if( Input.GetMouseButton(0) )
				{
					OKText.color = Color.Lerp(Color.clear, TextColor, 0.5f);
				}
				else
				{
					OKText.color = Color.white;
				}
			}
			break;
		case State.ExecuteAnim:
		case State.CancelAnim:
            if( Frame.animParam == MidairPrimitive.AnimationParams.None )
            {
                EnterState( State.Hide );
            }
			break;
        }
	}

	void UpdateShow()
	{
		Ray ray = GameContext.MainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);


		if( Input.GetMouseButtonDown(0) )
		{
			if( hit.collider == HitPlane.GetComponent<Collider>() && GameContext.State != GameState.Setting )
			{
				buttonType = ButtonType.OK;
			}
			else if( hit.collider == LeftButton.GetComponent<Collider>() && enableLeft_ )
			{
				buttonType = ButtonType.Left;
			}
			else if( hit.collider == RightButton.GetComponent<Collider>() && enableRight_ )
			{
				buttonType = ButtonType.Right;
			}
			else
			{
				buttonType = ButtonType.None;
				PanelMask.SetTargetColor(ColorManager.MakeAlpha(Color.black, 0.8f));
				GraphMask.SetTargetColor(Color.clear);
			}
		}
		else if( Input.GetMouseButton(0) )
		{
			if( hit.collider == HitPlane.GetComponent<Collider>() && buttonType == ButtonType.OK )
			{
				Color halfTrans = Color.Lerp(Color.clear, TextColor, 0.5f);
				OKText.color = halfTrans;
				LeftButton.SetTargetColor(halfTrans);
				RightButton.SetTargetColor(halfTrans);
				Frame.SetGrowSize(0.5f);
				Frame.SetTargetSize(4.0f);
			}
			else if( hit.collider == LeftButton.GetComponent<Collider>() && buttonType == ButtonType.Left )
			{
				LeftButton.SetTargetSize(1.0f);
				LeftButton.SetTargetColor(ButtonColor);
				Frame.SetGrowSize(0.5f);
				Frame.SetTargetSize(4.0f);
			}
			else if( hit.collider == RightButton.GetComponent<Collider>() && buttonType == ButtonType.Right )
			{
				RightButton.SetTargetSize(1.0f);
				RightButton.SetTargetColor(ButtonColor);
				Frame.SetGrowSize(0.5f);
				Frame.SetTargetSize(4.0f);
			}
			else
			{
				OKText.color = TextColor;
				LeftButton.SetTargetColor(ButtonColor);
				RightButton.SetTargetColor(ButtonColor);
				LeftButton.SetTargetSize(0.5f);
				RightButton.SetTargetSize(0.5f);
				Frame.SetGrowSize(0.0f);
				Frame.SetTargetSize(4.17f);
			}
		}
		else if( Input.GetMouseButtonUp(0) )
		{
			PanelMask.SetTargetColor(Color.clear);
			GraphMask.SetTargetColor(ColorManager.MakeAlpha(Color.black, 0.8f));
			bool isSelectable = (command_ is RevertCommand == false || GameContext.VoxSystem.GetWillEclipse((int)VPCount.Count));
			//Push OK
			if( hit.collider == HitPlane.GetComponent<Collider>() && buttonType == ButtonType.OK && Music.Just < CommandGraph.AllowInputEnd && isSelectable )
			{
				Frame.SetSize(4.17f);
				Frame.SetGrowSize(0);
				EnterState(State.DecideAnim);
				GetComponent<Animation>()["panelDecideAnim"].time = 0;
				GetComponent<Animation>()["panelDecideAnim"].speed = 1;
				GetComponent<Animation>().Play("panelDecideAnim");
				foreach( TextMesh textMesh in GetComponentsInChildren<TextMesh>() )
				{
					textMesh.GetComponent<Renderer>().enabled = false;
				}
				foreach( CounterSprite counter in GetComponentsInChildren<CounterSprite>() )
				{
					counter.transform.localScale = Vector3.zero;
				}
				ENHIcon.GetComponent<Renderer>().enabled = false;

				OKText.GetComponent<Renderer>().enabled = true;
				OKText.color = Color.white;
				OKText.text = command_.nameText.ToUpper() + System.Environment.NewLine + "X" + System.Environment.NewLine + "CANCEL";

				Music.Resume();
			}
			else if( hit.collider != HitPlane.GetComponent<Collider>() && hit.collider != GameContext.PlayerConductor.CommandGraph.CommandSphere.GetComponent<Collider>() )
			{
				if( hit.collider != null )
				{
					print(hit.collider.name);
				}
				Hide();
			}
			else
			{
				OKText.color = TextColor;
				LeftButton.SetTargetColor(ButtonColor);
				RightButton.SetTargetColor(ButtonColor);
				Frame.SetTargetSize(4.17f);
			}
		}
		else
		{
			OKText.color = Color.Lerp(Color.clear, TextColor, 0.5f + Music.MusicalCos(4) / 2.0f);
			LeftButton.SetColor(enableLeft_ ? Color.Lerp(Color.clear, ButtonColor, 0.5f + Music.MusicalCos(4) / 2.0f) : ColorManager.Base.MiddleBack);
			RightButton.SetColor(enableRight_ ? Color.Lerp(Color.clear, ButtonColor, 0.5f + Music.MusicalCos(4) / 2.0f) : ColorManager.Base.MiddleBack);
			Frame.SetGrowSize(Music.MusicalCos(4) * 0.3f);
			LeftButton.SetTargetSize(0.5f);
			RightButton.SetTargetSize(0.5f);
		}

		if( command_ is RevertCommand && GameContext.State != GameState.Setting )
		{
			RevertArc.SetTargetArc((float)GameContext.VoxSystem.currentVP/GameContext.EnemyConductor.InvertVP);

			if( GameContext.VoxSystem.GetWillEclipse((int)VPCount.Count) )
			{
				RevertCircleEdge.SetColor(Color.clear);
				RevertArc.SetColor(Color.clear);
				RevertRect.SetTargetColor(Color.clear);
			}
			else
			{
				RevertCircleEdge.SetColor(Color.white);
				RevertArc.SetColor(Color.white);
				RevertRect.SetTargetColor(Color.black);
			}
		}
	}

    void EnterState( State inState )
    {
        if( state != inState )
        {
            state = inState;
            switch( state )
            {
            case State.HideAnim:
                break;
            case State.Hide:
                transform.position = initialPosition_;
                transform.localScale = Vector3.zero;
                GraphMask.SetTargetColor( Color.clear );
                break;
            case State.ShowAnim:
                HitPlane.GetComponent<Collider>().enabled = true;
                LeftButton.GetComponent<Collider>().enabled = true;
                RightButton.GetComponent<Collider>().enabled = true;
                break;
            case State.Show:
                transform.position = initialPosition_;
                break;
			case State.DecideAnim:
                GameContext.PlayerConductor.CommandGraph.Select( command_ );
                break;
            case State.Decided:
                break;
            case State.ExecuteAnim:
                break;
			case State.CancelAnim:
				break;
            }
        }
    }

    public void Show( Vector3 position, PlayerCommand command )
    {
        if( state == State.Hide || state == State.Decided || GameContext.State == GameState.Setting )
        {
            transform.position = position;
            GetComponent<Animation>()["panelAnim"].time = 0;
            GetComponent<Animation>()["panelAnim"].speed = 1;
            GetComponent<Animation>().Play( "panelAnim" );
			if( GameContext.State == GameState.Setting )
			{
				TextWindow.SetMessage(MessageCategory.Help, "SPを使って " + command.name + "をレベルアップ、または" + System.Environment.NewLine
					+ "レベルダウンさせて　SPを他に割り振ることが　できます。");
			}
        }
		else return;

		EnterState(State.ShowAnim);
		OKText.text = "OK";
        PanelMask.SetTargetColor( Color.clear );
        GraphMask.SetTargetColor( ColorManager.MakeAlpha( Color.black, 0.8f ) );
        transform.localScale = Vector3.one;
        foreach( TextMesh textMesh in GetComponentsInChildren<TextMesh>() )
        {
            textMesh.GetComponent<Renderer>().enabled = true;
        }
        foreach( CounterSprite counter in GetComponentsInChildren<CounterSprite>() )
        {
            counter.transform.localScale = Vector3.one;
        }
        ENHIcon.GetComponent<Renderer>().enabled = true;
        command_ = command;
        NameText.text = command_.nameText.ToUpper();
		NameText.color = command.currentLevel == 0 ? ColorManager.Base.Shade : Color.white;

        ThemeColor themeColor = ColorManager.GetThemeColor( command.themeColor );
		if( command is InvertCommand )
		{
			Frame.SetColor(Color.black);
		}
		else
		{
			Frame.SetColor( command.currentLevel == 0 ? ColorManager.Base.Shade : themeColor.Bright);
		}
        Frame.Num = 4;
        Frame.SetSize( 4.17f );
        //Frame.RecalculatePolygon();

		HLCount.Count = command.GetHeal();
        HLRect.SetColor( command.GetHealColor() );
		DFCount.Count = command.GetDefend();
        DFRect.SetColor( command.GetDefColor() );

		ATRect.SetColor(Color.clear);
		InvertRect.SetColor(Color.clear);
		InvertAtCount.CounterColor = Color.clear;
		InvertAtText.color = Color.clear;
		RevertRect.SetColor(Color.clear);
		RevertArc.SetColor(Color.clear);
		RevertCircleEdge.SetColor(Color.clear);
		OKText.transform.localScale = Vector3.one * 0.2f;
		LVText.GetComponent<Renderer>().enabled = false;
		LVCount.CounterScale = 0;
		LVCount.CounterColor = Color.clear;
		enableLeft_ = false;
		enableRight_= false;
		LeftButton.GetComponent<Renderer>().enabled = enableLeft_;
		LeftButton.GetComponent<Collider>().enabled = enableLeft_;
		RightButton.GetComponent<Renderer>().enabled = enableRight_;
		RightButton.GetComponent<Collider>().enabled = enableRight_;
		if( command is InvertCommand )
		{
			OKText.color = Color.black;
			BaseRect.SetColor(Color.white);
			InvertRect.SetColor(Color.black);
			InvertAtCount.CounterColor = ColorManager.Accent.Critical;
			InvertAtText.color = Color.white;
			AtText.color = Color.clear;
			InvertAtCount.Count = command.GetAtk();
			VTRect.SetColor(Color.clear);
			VPRect.SetColor(Color.clear);
			ENHRect.SetColor(Color.clear);
			ATCount.CounterColor = Color.clear;
			VTCount.CounterColor = Color.clear;
			VPCount.CounterColor = Color.clear;
		}
		else
		{
			if( command is RevertCommand )
			{
				OKText.color = Color.black;
				BaseRect.SetColor(Color.white);
				if( GameContext.State != GameState.Setting )
				{
					RevertRect.SetColor(Color.black);
					RevertCircleEdge.SetColor(Color.white);
					RevertArc.SetWidth(0.9f);
					RevertArc.SetColor(Color.white);
					RevertArc.SetArc((float)GameContext.VoxSystem.currentVP/GameContext.EnemyConductor.InvertVP);
				}
			}
			else
			{
				OKText.color = Color.white;
				BaseRect.SetColor(Color.black);
			}
			AtText.color = Color.black;
			ATCount.CounterColor = Color.black;
			VTCount.CounterColor = Color.black;
			VPCount.CounterColor = Color.black;
			ATCount.Count = command.GetAtk();
			ATRect.SetColor(command.GetAtkColor());
			VTCount.Count = command.GetVT();
			VTRect.SetColor(command.GetVTColor());
			VPCount.Count = command.GetVP();
			VPRect.SetColor(command.GetVPColor());
			ENHRect.SetColor(command.GetEnhColor());
			ENHIcon.sprite = null;
			if( command.centerIcon.GetComponent<SpriteRenderer>() != null )
			{
				ENHIcon.sprite = command.centerIcon.GetComponent<SpriteRenderer>().sprite;
			}
		}
		if( command_.currentLevel == 0 ) BaseRect.SetColor(ColorManager.Base.Light);
    }

    public void Hide()
	{
		if( GameContext.VoxSystem.IsOverloading && command_ is InvertCommand ) return;
        switch( state )
        {
        case State.Show:
            EnterState( State.HideAnim );
            NameText.text = "";
            GetComponent<Animation>()["panelAnim"].time = GetComponent<Animation>()["panelAnim"].length;
            GetComponent<Animation>()["panelAnim"].speed = -1;
            GetComponent<Animation>().Play( "panelAnim" );
            break;
        case State.ShowAnim:
            EnterState( State.HideAnim );
            NameText.text = "";
            GetComponent<Animation>()["panelAnim"].speed = -1;
            break;
        case State.DecideAnim:
		case State.Decided:
			OKText.GetComponent<Renderer>().enabled = false;
			EnterState(State.ExecuteAnim);
			Frame.SetTargetSize(RingSize);
			GraphMask.SetTargetColor(Color.clear);
            break;
        }
    }
}
