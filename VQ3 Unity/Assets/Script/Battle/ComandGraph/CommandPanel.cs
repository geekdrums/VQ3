using UnityEngine;
using System.Collections;

public class CommandPanel : MonoBehaviour {

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
    public SpriteRenderer ENHIcon;
    public TextMesh NameText;
	public TextMesh OKText;
	public TextMesh AtText;
	public TextMesh InvertAtText;
    public MidairPrimitive LeftButton;
    public MidairPrimitive RightButton;
    public MidairPrimitive PanelMask;
    public MidairPrimitive GraphMask;
    public GameObject HitPlane;

    float RingSize { get { return GameContext.PlayerConductor.commandGraph.AxisRing.Radius; } }
	Color TextColor { get { return (command_ is RevertCommand ? Color.black : Color.white); } }

    Vector3 initialPosition_;
    PlayerCommand command_;
    //public bool IsActive { get; private set; }
    //public bool IsDecided { get; private set; }
    public State state = State.Hide;
    ButtonType buttonType = ButtonType.None;
    public enum State
    {
        HideAnim,
        Hide,
        ShowAnim,
        Show,
        DecideAnim,
        Decided,
        ExecuteAnim,
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
		Ray ray = GameContext.MainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);

        switch( state )
        {
        case State.HideAnim:
            //Vector3 rectPos = Vector3.Lerp( transform.localPosition, command_.transform.localPosition, 0.3f );
            //rectPos.x = Mathf.Clamp( rectPos.x, -RingSize + Frame.Radius * 2.0f, RingSize - Frame.Radius * 2.0f );
            //rectPos.y = Mathf.Clamp( rectPos.y, -RingSize + Frame.Radius * 2.0f, RingSize - Frame.Radius * 2.0f );
            //rectPos.z = transform.localPosition.z;
            //transform.localPosition = rectPos;
            if( animation.isPlaying == false )
            {
                EnterState( State.Hide );
            }
            break;
        case State.Hide:
            break;
        case State.ShowAnim:
            transform.position = Vector3.Lerp( transform.position, initialPosition_, 0.3f );
            if( animation.isPlaying == false )
            {
                EnterState( State.Show );
            }
            break;
        case State.Show:
            if( Input.GetMouseButtonDown( 0 ) )
            {
                if( hit.collider == HitPlane.collider )
                {
                    buttonType = ButtonType.OK;
                }
                else if( hit.collider == LeftButton.collider )
                {
                    buttonType = ButtonType.Left;
                }
                else if( hit.collider == RightButton.collider )
                {
                    buttonType = ButtonType.Right;
                }
                else
                {
                    buttonType = ButtonType.None;
                    PanelMask.SetTargetColor( ColorManager.MakeAlpha( Color.black, 0.8f ) );
                    GraphMask.SetTargetColor( Color.clear );
                }
            }
            else if( Input.GetMouseButton( 0 ) )
            {
                if( hit.collider == HitPlane.collider && buttonType == ButtonType.OK )
                {
					Color halfTrans = Color.Lerp(Color.clear, TextColor, 0.5f);
					OKText.color = halfTrans;
					LeftButton.SetTargetColor(halfTrans);
					RightButton.SetTargetColor(halfTrans);
                    Frame.SetGrowSize( 0.5f );
                    Frame.SetTargetSize( 4.0f );
                }
                else
                {
					OKText.color = TextColor;
                    LeftButton.SetTargetColor( Color.white );
                    RightButton.SetTargetColor( Color.white );
                    Frame.SetGrowSize( 0.0f );
                    Frame.SetTargetSize( 4.17f );
                }
            }
            else if( Input.GetMouseButtonUp( 0 ) )
            {
                PanelMask.SetTargetColor( Color.clear );
                GraphMask.SetTargetColor( ColorManager.MakeAlpha( Color.black, 0.8f ) );
				bool isSelectable = (command_ is RevertCommand == false || GameContext.VoxSystem.GetWillEclipse((int)VPCount.Count));
				if( hit.collider == HitPlane.collider && Music.Just < CommandGraph.AllowInputEnd && isSelectable )
                {
                    Frame.SetSize( 4.17f );
                    EnterState( State.DecideAnim );
                    animation["panelDecideAnim"].time = 0;
                    animation["panelDecideAnim"].speed = 1;
                    animation.Play( "panelDecideAnim" );
                    foreach( TextMesh textMesh in GetComponentsInChildren<TextMesh>() )
                    {
                        textMesh.renderer.enabled = false;
                    }
                    foreach( CounterSprite counter in GetComponentsInChildren<CounterSprite>() )
                    {
                        counter.transform.localScale = Vector3.zero;
                    }
                    ENHIcon.renderer.enabled = false;

					OKText.renderer.enabled = true;
					OKText.color = Color.white;
					OKText.text = "X" + System.Environment.NewLine + "CANCEL";

					Music.Resume();
                }
                else
                {
                    OKText.color = TextColor;
					LeftButton.SetTargetColor(TextColor);
					RightButton.SetTargetColor(TextColor);
                    Frame.SetTargetSize( 4.17f );
                }
            }
            else
            {
				OKText.color = Color.Lerp(Color.clear, TextColor, 0.5f + Music.MusicalSin(4) / 2.0f);
				Frame.SetGrowSize(Music.MusicalSin(4) * 0.3f);
            }

			if( command_ is RevertCommand )
			{
				RevertArc.SetTargetArc((float)GameContext.VoxSystem.currentVP/VoxSystem.InvertVP);

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
            break;
        case State.DecideAnim:
            if( animation.isPlaying == false )
            {
                EnterState( State.Decided );
            }
            break;
		case State.Decided:
			{
				if( Input.GetMouseButtonUp(0) && Music.Just < CommandGraph.AllowInputEnd )
				{
					GameContext.PlayerConductor.commandGraph.Deselect();
					EnterState(State.ShowAnim);
					OKText.text = "OK";
					animation["panelDecideAnim"].time = animation["panelDecideAnim"].length;
					animation["panelDecideAnim"].speed = -2.0f;
					animation.Play("panelDecideAnim");
					foreach( TextMesh textMesh in GetComponentsInChildren<TextMesh>() )
					{
						textMesh.renderer.enabled = true;
					}
					foreach( CounterSprite counter in GetComponentsInChildren<CounterSprite>() )
					{
						counter.transform.localScale = Vector3.one;
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
            if( Frame.animParam == MidairPrimitive.AnimationParams.None )
            {
                EnterState( State.Hide );
            }
            break;
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
                HitPlane.collider.enabled = true;
                LeftButton.collider.enabled = true;
                RightButton.collider.enabled = true;
                break;
            case State.Show:
                transform.position = initialPosition_;
                break;
			case State.DecideAnim:
                GameContext.PlayerConductor.commandGraph.Select( ( command_.ParentCommand != null ? command_.ParentCommand : command_ ) );
                break;
            case State.Decided:
				//HitPlane.collider.enabled = false;
				//LeftButton.collider.enabled = false;
				//RightButton.collider.enabled = false;
                //GraphMask.SetTargetColor( Color.clear );
                break;
            case State.ExecuteAnim:
                break;
            }
        }
    }

    public void Show( Vector3 position, PlayerCommand command )
    {
        switch( state )
        {
        case State.Hide:
        case State.Decided:
            transform.position = position;
            animation["panelAnim"].time = 0;
            animation["panelAnim"].speed = 1;
            animation.Play( "panelAnim" );
            break;
        default:
            return;
        }

		EnterState(State.ShowAnim);
		OKText.text = "OK";
        PanelMask.SetTargetColor( Color.clear );
        GraphMask.SetTargetColor( ColorManager.MakeAlpha( Color.black, 0.8f ) );
        transform.localScale = Vector3.one;
        foreach( TextMesh textMesh in GetComponentsInChildren<TextMesh>() )
        {
            textMesh.renderer.enabled = true;
        }
        foreach( CounterSprite counter in GetComponentsInChildren<CounterSprite>() )
        {
            counter.transform.localScale = Vector3.one;
        }
        ENHIcon.renderer.enabled = true;
        command_ = command;
        NameText.text = command_.NameText.ToUpper();

        ThemeColor themeColor = ColorManager.GetThemeColor( command.themeColor );
        BaseColor baseColor = ColorManager.Base;
		if( command is InvertCommand )
		{
			Frame.SetColor(Color.black);
		}
		else
		{
			Frame.SetColor(themeColor.Bright);
		}
        Frame.Num = 4;
        Frame.SetSize( 4.17f );
        Frame.RecalculatePolygon();

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
				RevertRect.SetColor(Color.black);
				RevertCircleEdge.SetColor(Color.white);
				RevertArc.SetWidth(0.9f);
				RevertArc.SetColor(Color.white);
				RevertArc.SetArc((float)GameContext.VoxSystem.currentVP/VoxSystem.InvertVP);
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
			ENHIcon.sprite = command.GetEnhIconSprite();
		}
    }

    public void Hide()
    {
		if( GameContext.VoxSystem.IsInverting && command_ is InvertCommand ) return;
        switch( state )
        {
        case State.Show:
            EnterState( State.HideAnim );
            NameText.text = "";
            animation["panelAnim"].time = animation["panelAnim"].length;
            animation["panelAnim"].speed = -1;
            animation.Play( "panelAnim" );
            break;
        case State.ShowAnim:
            EnterState( State.HideAnim );
            NameText.text = "";
            animation["panelAnim"].speed = -1;
            break;
        case State.DecideAnim:
		case State.Decided:
			OKText.renderer.enabled = false;
			EnterState(State.ExecuteAnim);
			Frame.SetTargetSize(RingSize);
			Frame.SetTargetColor(Color.clear);
			GraphMask.SetTargetColor(Color.clear);
            break;
        }
    }
}
