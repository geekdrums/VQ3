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
    public CounterSprite ATCount;
    public CounterSprite HLCount;
    public CounterSprite DFCount;
    public CounterSprite VTCount;
    public CounterSprite VPCount;
    public SpriteRenderer ENHIcon;
    public TextMesh NameText;
    public TextMesh OKText;
    public MidairPrimitive LeftButton;
    public MidairPrimitive RightButton;
    public MidairPrimitive PanelMask;
    public MidairPrimitive GraphMask;
    public GameObject HitPlane;

    float RingSize { get { return GameContext.PlayerConductor.commandGraph.AxisRing.Radius; } }

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
	void Update () {
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
            Ray ray = GameContext.MainCamera.ScreenPointToRay( Input.mousePosition );
            RaycastHit hit;
            Physics.Raycast( ray.origin, ray.direction, out hit, Mathf.Infinity );

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
                    Color halfTransWhite = Color.Lerp( Color.clear, Color.white, 0.5f );
                    OKText.color = halfTransWhite;
                    LeftButton.SetTargetColor( halfTransWhite );
                    RightButton.SetTargetColor( halfTransWhite );
                    Frame.SetGrowSize( 0.5f );
                    Frame.SetTargetSize( 4.0f );
                }
                else
                {
                    OKText.color = Color.white;
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
                if( hit.collider == HitPlane.collider )
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
                }
                else
                {
                    OKText.color = Color.white;
                    LeftButton.SetTargetColor( Color.white );
                    RightButton.SetTargetColor( Color.white );
                    Frame.SetTargetSize( 4.17f );
                }
            }
            else
            {
                float sin = (Mathf.Sin( (float)(Mathf.PI * Music.MusicalTime / 4.0f) ) + 1.0f) / 2.0f;
                OKText.color = Color.Lerp( Color.clear, Color.white, 0.5f + sin / 2.0f );
                Frame.SetGrowSize( sin * 0.3f );
            }
            break;
        case State.DecideAnim:
            if( animation.isPlaying == false )
            {
                EnterState( State.Decided );
            }
            break;
        case State.Decided:
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
                GameContext.PlayerConductor.commandGraph.Select( command_ );
                break;
            case State.Decided:
                HitPlane.collider.enabled = false;
                LeftButton.collider.enabled = false;
                RightButton.collider.enabled = false;
                GraphMask.SetTargetColor( Color.clear );
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
        //case State.Decided:
        //    animation["panelDecideAnim"].time = animation["panelDecideAnim"].length;
        //    animation["panelDecideAnim"].speed = -0.1f;
        //    animation.Play( "panelDecideAnim" );
        //    break;
        default:
            return;
        }

        PanelMask.SetTargetColor( Color.clear );
        GraphMask.SetTargetColor( ColorManager.MakeAlpha( Color.black, 0.8f ) );
        EnterState( State.ShowAnim );
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
        Frame.SetColor( themeColor.Light );
        Frame.Num = 4;
        Frame.SetSize( 4.17f );
        Frame.RecalculatePolygon();

        ATCount.Count = command.GetAtk();
        ATRect.SetColor( command.GetAtkColor() );
        HLCount.Count = command.GetHeal();
        HLRect.SetColor( command.GetHealColor() );
        DFCount.Count = command.GetDefend();
        DFRect.SetColor( command.GetDefColor() );
        VTCount.Count = command.GetVT();
        VTRect.SetColor( command.GetVTColor() );
        VPCount.Count = command.GetVP();
        VPRect.SetColor( command.GetVPColor() );
        ENHRect.SetColor( command.GetEnhColor() );
        ENHIcon.sprite = command.GetEnhIconSprite();
    }

    public void Hide()
    {
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
            EnterState( State.ExecuteAnim );
            Frame.SetTargetSize( RingSize );
            Frame.SetTargetColor( Color.clear );
            break;
        }
    }
}
