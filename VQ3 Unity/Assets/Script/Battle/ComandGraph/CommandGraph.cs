using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public enum VoxButton
{
    None,
    Ball,
    ArrowRight,
    ArrowLeft,
    Enter,
    Reset,
    Count
}

[ExecuteInEditMode]
public class CommandGraph : MonoBehaviour {

	public static Timing AllowInputEnd = new Timing(3, 3, 3);
	public static Timing WaitInputEnd = new Timing(0, 3, 3);

    public GameObject CommandIconPrefab;
    public GameObject RevertCommandIconPrefab;
    public GameObject InvertCommandIconPrefab;
    public GameObject EdgePrefab;
    public List<GameObject> SkillPrefabs;

    public GameObject CommandSphere;
    public GameObject EdgeSphere;
    public GameObject SelectSpot;
    public GameObject AreaRect;

    public MidairPrimitive CurrentRect;
    public MidairPrimitive TouchRect;
	public MidairPrimitive NextRect;

    public CommandPanel Panel;

    public Vector3 MaxScale = new Vector3( 0.24f, 0.24f, 0.24f );
    public float ScaleCoeff = 0.05f;
    public float MaskColorCoeff = 0.06f;
    public float MaskStartPos = 3.0f;
    public float SphereRadius = 6.5f;
    public float TouchRectCoeff = 1.7f;
    //public GameObject RightArrow;
    //public GameObject LeftArrow;
    //public GameObject TimeBar;
    //public GameObject CurrentBar;
    //public GameObject NextBar;
    public float MAX_LATITUDE;
    public float ROTATE_COEFF;
    public float BUTTON_RADIUS;
    public bool UPDATE_BUTTON;
    //public TextMesh CurrentCommandText;
    //public TextMesh NextCommandText;
    //public List<Sprite> CommandIcons;
    public MidairPrimitive AxisRing;
    //public Strategy PilgrimStrategy;

    public PlayerCommand IntroCommand;
    public List<PlayerCommand> CommandNodes { get; private set; }
    //public List<Strategy> StrategyNodes { get; private set; }
    public PlayerCommand NextCommand { get; private set; }
    public PlayerCommand PushingCommand { get; private set; }
    public PlayerCommand CurrentCommand { get; private set; }
    public PlayerCommand OldCommand { get; private set; }
    public VoxButton CurrentButton { get; private set; }

    //public Color IconAPColor;
    //public Color IconWLFMColor;
    //public Color IconDSColor;
    //public Color IconHRColor;
    //public Color IconVColor;

    bool IsInvert { get { return CurrentCommand is InvertCommand; } }
    bool IsLastInvert { get { return IsInvert && (GameContext.VoxSystem.InvertTime == 1 || (CurrentCommand as InvertCommand).IsLast); } }
    int CommandLoopCount;

    //Timing AllowInputStart = new Timing( 0, 0, 1 );
    Vector3 ballTouchStartPosition;
    Vector3 oldMousePosition;
    Quaternion targetRotation;
    Quaternion offsetRotation;

    //Vector3 initialRightArrowPosition;
    //Vector3 initialLeftArrowPosition;
    //string initialNextText;
    //Vector3 maxTimeBarScale;
    //Vector3 targetTimeBarScale;
    //Vector3 initialCurrentBarPosition;
    //Vector3 initialNextBarPosition;

	// Use this for initialization
	void Start () {
        //StrategyNodes = new List<Strategy>();
        //StrategyNodes.AddRange( GetComponentsInChildren<Strategy>() );
        CommandNodes = new List<PlayerCommand>();
        CommandNodes.AddRange( GetComponentsInChildren<PlayerCommand>() );
        offsetRotation = Quaternion.LookRotation( transform.position - SelectSpot.transform.position );
        //initialPosition = transform.localPosition;
        //initialRightArrowPosition = RightArrow.transform.localPosition;
        //initialLeftArrowPosition = LeftArrow.transform.localPosition;
        CurrentButton = VoxButton.None;
        CurrentCommand = IntroCommand;
        //initialNextText = NextCommandText.text;
        //maxTimeBarScale = new Vector3( CurrentBar.transform.localScale.x, TimeBar.transform.localScale.y, TimeBar.transform.localScale.z );
        //targetTimeBarScale = new Vector3( 0, TimeBar.transform.localScale.y, TimeBar.transform.localScale.z );
        //TimeBar.transform.localScale = targetTimeBarScale;
        //initialCurrentBarPosition = CurrentBar.transform.localPosition;
        //initialNextBarPosition = NextBar.transform.localPosition;

        CurrentRect.transform.parent = IntroCommand.transform;
        CurrentRect.transform.localPosition = Vector3.forward;
        CurrentRect.transform.localScale = Vector3.one;
		NextRect.transform.localScale = Vector3.zero;
        TouchRect.SetColor( Color.clear );
        //TouchRect.GetComponentInChildren<TextMesh>().text = "";
    }

    void InitializeLinks()
    {
        //StrategyNodes = new List<Strategy>();
        //StrategyNodes.AddRange( GetComponentsInChildren<Strategy>() );
        //foreach( Strategy strategy in StrategyNodes )
        //{
        //    foreach( IVoxNode link in strategy.links )
        //    {
        //        InstantiateLine( strategy, link );
        //    }
        //    foreach( PlayerCommand command in strategy.Commands )
        //    {
        //        foreach( IVoxNode link in command.links )
        //        {
        //            InstantiateLine( command, link );
        //        }
        //        command.SetLink( false );
        //    }
        //}
        CommandNodes = new List<PlayerCommand>();
        CommandNodes.AddRange( GetComponentsInChildren<PlayerCommand>() );
        List<Pair<PlayerCommand, PlayerCommand>> commandPairs = new List<Pair<PlayerCommand, PlayerCommand>>();
        foreach( PlayerCommand command in CommandNodes )
        {
            command.SetLink( false );
            if( command.ParentCommand != null ) continue;
            foreach( PlayerCommand link in command.links )
            {
                if( null == commandPairs.Find( ( Pair<PlayerCommand, PlayerCommand> pair ) =>
                        (pair.first == command && pair.second == link) || (pair.first == command && pair.second == link) ) )
                {
                    commandPairs.Add( new Pair<PlayerCommand, PlayerCommand>( link, command ) );
                }
            }
        }
        foreach( PlayerCommand link in IntroCommand.links )
        {
            commandPairs.Add( new Pair<PlayerCommand, PlayerCommand>( link, IntroCommand ) );
        }
        foreach( Pair<PlayerCommand, PlayerCommand> pair in commandPairs )
        {
            InstantiateLine( pair.first, pair.second );
        }
    }

    void InstantiateLine( PlayerCommand from, PlayerCommand to )
    {
        LineRenderer edge = (Instantiate( EdgePrefab ) as GameObject).GetComponent<LineRenderer>();
        edge.transform.position = EdgeSphere.transform.position;
        edge.transform.parent = EdgeSphere.transform;
        edge.SetVertexCount( 2 );
        edge.SetPosition( 0, from.transform.localPosition );
        edge.SetPosition( 1, to.transform.localPosition );
        from.OnEdgeCreated( edge );
        to.OnEdgeCreated( edge );
        //edge.SetVertexCount( 8 );
        //for( int i = 0; i < 8; i++ )
        //{
        //    edge.SetPosition( i, transform.rotation * Vector3.Slerp( from.Transform().localPosition, to.Transform().localPosition, (3 + i) / 12.0f ) );
        //}
    }
    
#if UNITY_EDITOR
    enum CommandListProperty
    {
        Level,
        Category,
        Name,
        Longitude,
        Latitude,
        EnglishName,
        Icon,
        Music,
        Link1,
        Link2,
        Link3,
        Link4,
        Link5,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
        Optima,
        //AcquireText,
    }
    void UpdateCommandList()
    {
		CurrentRect.transform.parent = transform;
		NextRect.transform.parent = transform;
        foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
        {
            DestroyImmediate( command.gameObject );
        }
        foreach( LineRenderer commandEdge in EdgeSphere.GetComponentsInChildren<LineRenderer>() )
        {
            DestroyImmediate( commandEdge.gameObject );
        }

        string path = Application.streamingAssetsPath + "/VQ3List - Command.csv";
        StreamReader reader = File.OpenText( path );
        if( reader != null )
        {
            CommandNodes = new List<PlayerCommand>();
            //CommandNodes.AddRange( GetComponentsInChildren<PlayerCommand>() );
            Dictionary<PlayerCommand, string[]> LinkDictionary = new Dictionary<PlayerCommand, string[]>(); 
            string line = reader.ReadLine();
            char[] separator = new char[]{','};
            while( (line = reader.ReadLine()) != null )
            {
                string[] propertyTexts = line.Split(separator, System.StringSplitOptions.None);
                string commandName = propertyTexts[(int)CommandListProperty.Name];
                string categoryName = propertyTexts[(int)CommandListProperty.Category];
                if( commandName == "" || categoryName  == "" ) continue;
                PlayerCommand playerCommand = CommandNodes.Find( ( PlayerCommand command ) => command.name == commandName );
                if( playerCommand == null )
                {
                    GameObject commandObj = null;

                    if( categoryName == "V" )
                    {
                        commandObj = Instantiate( InvertCommandIconPrefab ) as GameObject;
                    }
                    else if( categoryName == "R" )
                    {
                        commandObj = Instantiate( RevertCommandIconPrefab ) as GameObject;
                    }
                    else
                    {
                        commandObj = Instantiate( CommandIconPrefab ) as GameObject;
                    }
                    commandObj.name = commandName;
                    commandObj.transform.parent = this.transform;
                    playerCommand = commandObj.GetComponent<PlayerCommand>();
                    CommandNodes.Add( playerCommand );
                }
                LinkDictionary.Add( playerCommand, new string[] { 
                    propertyTexts[(int)CommandListProperty.Link1],propertyTexts[(int)CommandListProperty.Link2],propertyTexts[(int)CommandListProperty.Link3],
                    propertyTexts[(int)CommandListProperty.Link4],propertyTexts[(int)CommandListProperty.Link5] } );
                playerCommand.AcquireLevel = int.Parse( propertyTexts[(int)CommandListProperty.Level] );
                playerCommand.longitude = int.Parse( propertyTexts[(int)CommandListProperty.Longitude] );
                playerCommand.latitude = int.Parse( propertyTexts[(int)CommandListProperty.Latitude] );
                playerCommand.MusicBlockName = propertyTexts[(int)CommandListProperty.Music];
                playerCommand.NameText = propertyTexts[(int)CommandListProperty.EnglishName];

                string[] skillTexts = new string[4];
                skillTexts[0] = propertyTexts[(int)CommandListProperty.Skill1];
                skillTexts[1] = propertyTexts[(int)CommandListProperty.Skill2];
                skillTexts[2] = propertyTexts[(int)CommandListProperty.Skill3];
                skillTexts[3] = propertyTexts[(int)CommandListProperty.Skill4];
                playerCommand._timingStr = "";
                foreach( string skillText in skillTexts )
                {
                    if( skillText == "" ) break;
                    string skillName = skillText.Substring(0,skillText.IndexOf(" ") );
                    Skill skill = SkillPrefabs.Find( ( GameObject obj ) => obj.name == skillName ).GetComponent<Skill>();
                    if( skill != null )
                    {
                        playerCommand._skillList.Add( skill );
                        playerCommand._timingStr += skillText.Substring( skillText.IndexOf( " " ) ) + ",";
                    }
                    else
                    {
                        Debug.Log( "Can't find " + skillText );
                    }
                }

                //playerCommand.GetComponent<TextMesh>().text = commandName.Insert( 2, "\n" );//propertyTexts[(int)CommandListProperty.Icon];
                //playerCommand.GetComponent<TextMesh>().fontSize = 8;
                //playerCommand.AcquireText = propertyTexts[(int)CommandListProperty.AcquireText];
                string iconStr = propertyTexts[(int)CommandListProperty.Icon];
                playerCommand.icons = new List<EStatusIcon>();
                for( int i = 0; i < (int)EStatusIcon.Count; i++ )
                {
                    if( iconStr.Contains( ((EStatusIcon)i).ToString() ) )
                    {
                        playerCommand.icons.Add( (EStatusIcon)i );
                        iconStr = iconStr.Replace( ((EStatusIcon)i).ToString(), "" );
                        if( iconStr == "" ) break;
                    }
                }
                //if( categoryName == "G" ) playerCommand.SetParent( PilgrimStrategy );
                
                if( playerCommand.icons.Contains( EStatusIcon.DD ) )
                {
                    playerCommand.PhysicDefend = 60;
                    playerCommand.MagicDefend = 60;
                }
                else if( playerCommand.icons.Contains( EStatusIcon.D ) )
                {
                    playerCommand.PhysicDefend = 40;
                    playerCommand.MagicDefend = 40;
                }
                if( playerCommand.icons.Contains( EStatusIcon.HH ) )
                {
                    playerCommand.HealPercent = 40;
                }
                else if( playerCommand.icons.Contains( EStatusIcon.H ) )
                {
                    playerCommand.HealPercent = 22;
                }
                if( propertyTexts[(int)CommandListProperty.Music] == "intro" )
                {
                    IntroCommand = playerCommand;
                }
                playerCommand.ValidatePosition();
            }
            foreach( PlayerCommand playerCommand in CommandNodes )
            {
                if( !LinkDictionary.ContainsKey( playerCommand ) )
                {
                    continue;
                }
                playerCommand.links = new List<PlayerCommand>();
                foreach( string linkStr in LinkDictionary[playerCommand] )
                {
                    if( linkStr != "" )
                    {
                        PlayerCommand linkNode = CommandNodes.Find( ( PlayerCommand command ) => command.name == linkStr );
                        if( linkNode != null )
                        {
                            playerCommand.links.Add( linkNode );
                        }
                    }
                }
            }

            InitializeLinks();
        }
        else
        {
            Debug.LogError( path + " not found!" );
        }
    }
#endif

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying )
        {
            if( UPDATE_BUTTON )
            {
                UPDATE_BUTTON = false;
                UpdateCommandList();
            }
            return;
        }
#endif
        UpdateInput();


        switch( GameContext.CurrentState )
        {
        case GameState.Continue:
            if( GameContext.CurrentState == GameState.Continue && (!Music.IsPlaying() || Music.Just.totalUnit > 4)
                && Input.GetMouseButtonUp( 0 ) )
            {
                GameContext.ChangeState( GameState.Intro );
            }
            break;
        case GameState.Intro:
            if( Music.NextBlockName == "intro" && Music.Just.totalUnit > 4 )
            {
                //if( Music.isJustChanged ) SelectNearestNode();
                if( Music.isJustChanged && NextCommand != null && NextCommand != IntroCommand && !Input.GetMouseButton( 0 ) )
                {
                    SetNextBlock();
                }
            }
            if( Music.IsJustChangedAt( AllowInputEnd ) )
            {
                SetNextBlock();
            }
            //UpdateCommandLine();
            break;
        case GameState.Battle:
            if( Music.IsJustChangedAt( AllowInputEnd ) ||
                (Music.CurrentBlockName == "wait" && Music.IsJustChangedAt( WaitInputEnd )) )
            {
                SetNextBlock();
            }
            //UpdateCommandLine();
            break;
        }

        if( Music.CurrentBlockName == "wait" || GameContext.CurrentState != GameState.Battle )
        {
            AxisRing.ArcRate = 0.0f;
        }
        else
        {
            AxisRing.ArcRate = (float)(1.0f - Music.MusicalTime / 64.0);
        }
		if( NextCommand != null )
		{
			NextRect.SetSize(6 + Music.MusicalSin(4));
			NextRect.SetColor(Color.Lerp(Color.white, Color.clear, Music.MusicalSin(4) * 0.5f));
		}
        //AxisRing.SetTargetColor( ( GameContext.CurrentState == GameState.Battle && NextCommand != null ? Color.magenta : Color.white ) );
    }

    void UpdateInput()
    {
        Ray ray = GameContext.MainCamera.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
        Physics.Raycast( ray.origin, ray.direction, out hit, Mathf.Infinity );

        if( Input.GetMouseButtonDown( 0 ) )
        {
            oldMousePosition = Input.mousePosition;
            CurrentButton = VoxButton.None;
            if( hit.collider == CommandSphere.collider )
            {
                CurrentButton = VoxButton.Ball;
                if( Panel.state != CommandPanel.State.Show )
                {
                    PushCommandButton( hit.point );
                }
                ballTouchStartPosition = hit.point;
            }
            //else if( hit.collider == RightArrow.collider )
            //{
            //    CurrentButton = VoxButton.ArrowRight;
            //    GameContext.EnemyConductor.OnArrowPushed( false );
            //}
            //else if( hit.collider == LeftArrow.collider )
            //{
            //    CurrentButton = VoxButton.ArrowLeft;
            //    GameContext.EnemyConductor.OnArrowPushed( true );
            //}
        }
        else if( Input.GetMouseButtonUp( 0 ) )
        {
            if( CurrentButton == VoxButton.Ball && Music.Just < AllowInputEnd )
            {
                if( Panel.state == CommandPanel.State.Show )
                {
                    PushingCommand = null;
                    Panel.Hide();
                }
                else if( PushingCommand != null )
                {
                    TouchRect.GrowSize = 0;
                    TouchRect.SetSize( 0 );
                    TouchRect.SetWidth( 0 );
					PlayerCommand pushCommand = PushingCommand;
					if( PushingCommand == (CurrentCommand != null && CurrentCommand.ParentCommand != null ? CurrentCommand.ParentCommand : CurrentCommand) )
					{
						pushCommand = PushingCommand.FindLoopVariation(CommandLoopCount+1);
					}
                    if( NextCommand != null )
                    {
                        if( GameContext.CurrentState != GameState.Intro )
                        {
                            NextCommand.Deselect();
                            NextCommand = null;
							Panel.Show(TouchRect.transform.position, pushCommand);
							NextRect.transform.localScale = Vector3.zero;
                        }
                    }
                    else
                    {
						Panel.Show(TouchRect.transform.position, pushCommand);
                    }
                }
            }
            else if( hit.collider == AreaRect.collider )
            {
                if( Panel.state == CommandPanel.State.Show )
                {
                    Panel.Hide();
                }
            }
        }
        else if( Input.GetMouseButton( 0 ) )
        {
            if( CurrentButton == VoxButton.Ball )
            {
                Quaternion oldRotation = transform.rotation;

                Vector3 deltaV = Input.mousePosition - oldMousePosition;
                transform.rotation *= (Quaternion.Inverse( transform.rotation )
                    * Quaternion.AngleAxis( deltaV.y * ROTATE_COEFF, Vector3.right )
                    * Quaternion.AngleAxis( deltaV.x * ROTATE_COEFF, Vector3.down ) * transform.rotation);
                oldMousePosition = Input.mousePosition;

                SelectSpot.transform.parent = transform;
                transform.rotation = Quaternion.Inverse( Quaternion.LookRotation( -SelectSpot.transform.localPosition ) ) * offsetRotation;
                SelectSpot.transform.parent = transform.parent;

                Quaternion up = Quaternion.LookRotation( Vector3.up, Vector3.up );
                Quaternion down = Quaternion.LookRotation( Vector3.down, Vector3.up );
                Quaternion rotUp = Quaternion.LookRotation( transform.up, Vector3.up );
                Quaternion rotDown = Quaternion.LookRotation( -transform.up, Vector3.up );
                float angle = Mathf.Min( Quaternion.Angle( rotUp, up ), Quaternion.Angle( rotDown, down ), Quaternion.Angle( rotUp, down ), Quaternion.Angle( rotDown, up ) );
                if( angle > MAX_LATITUDE )
                {
                    transform.rotation = oldRotation;
                    //transform.rotation *= Quaternion.FromToRotation( transform.up, Quaternion.RotateTowards( up, rotUp, MAX_LATITUDE ) * Vector3.up );
                }

                //touch rect
                Vector3 rectPos = new Vector3( hit.point.x, hit.point.y, TouchRect.transform.position.z ) - TouchRect.transform.parent.position;
                rectPos.x = Mathf.Clamp( rectPos.x, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
                rectPos.y = Mathf.Clamp( rectPos.y, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
                TouchRect.transform.localPosition = rectPos;

                if( (ballTouchStartPosition - hit.point).magnitude > BUTTON_RADIUS / 2 )
                {
                    if( PushingCommand != null )
                    {
                        PushingCommand.SetPush( false );
                        PushingCommand = null;
                        TouchRect.GrowSize = 0;
                        TouchRect.SetTargetSize( 2.0f );
                        TouchRect.SetTargetWidth( 0 );
                        //TouchRect.GetComponentInChildren<TextMesh>().text = "";
                    }
                    else if( Panel.state == CommandPanel.State.Show )
                    {
                        Panel.Hide();
                    }
                }
            }

            //if( CurrentButton == VoxButton.ArrowRight )
            //{
            //    RightArrow.transform.localPosition = Vector3.MoveTowards( RightArrow.transform.localPosition,
            //        initialRightArrowPosition + (Input.GetMouseButton( 0 ) ? Vector3.forward * 0.3f : Vector3.zero), 0.1f );
            //}
            //else if( CurrentButton == VoxButton.ArrowLeft )
            //{
            //    LeftArrow.transform.localPosition = Vector3.MoveTowards( LeftArrow.transform.localPosition,
            //        initialLeftArrowPosition + (Input.GetMouseButton( 0 ) ? Vector3.forward * 0.3f : Vector3.zero), 0.1f );
            //}
        }
        else
        {
            CurrentButton = VoxButton.None;
            if( NextCommand != null )
            {
                transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 0.1f );
            }
            //RightArrow.transform.localPosition = Vector3.MoveTowards( RightArrow.transform.localPosition, initialRightArrowPosition, 0.1f );
            //LeftArrow.transform.localPosition = Vector3.MoveTowards( LeftArrow.transform.localPosition, initialLeftArrowPosition, 0.1f );
        }
            //Quaternion oldRotation = transform.rotation;
            //Vector3 up = transform.up;
            //transform.rotation = Quaternion.LookRotation( transform.forward, new Vector3( 0, up.y, up.z ) );
            /*
            Quaternion up = Quaternion.LookRotation( Vector3.up, Vector3.up );
            Quaternion down = Quaternion.LookRotation( Vector3.down, Vector3.up );
            Quaternion rotUp = Quaternion.LookRotation( transform.up, Vector3.up );
            Quaternion rotDown = Quaternion.LookRotation( -transform.up, Vector3.up );
            float angle = Mathf.Min( Quaternion.Angle( rotUp, up ), Quaternion.Angle( rotDown, down ), Quaternion.Angle( rotUp, down ), Quaternion.Angle( rotDown, up ) );
            print( angle );
            if( angle > MAX_LATITUDE )
            {
                //transform.rotation = oldRotation;
                //transform.rotation *= Quaternion.FromToRotation( transform.up, Quaternion.RotateTowards( up, rotUp, MAX_LATITUDE ) * Vector3.up );
            }
            */
            /*
            transform.localPosition = Vector3.MoveTowards( transform.localPosition, initialPosition + Vector3.forward * 0.2f, 0.1f );
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards( transform.localPosition, initialPosition, 0.1f );
            //transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 0.1f );
        }*/

        EdgeSphere.transform.rotation = transform.rotation;
    }


    //void UpdateCommandLine()
    //{
    //    bool isWait = Music.CurrentBlockName == "wait";
    //    float mt = (float)Music.MusicalTime;
    //    targetTimeBarScale.x = maxTimeBarScale.x * (mt / (isWait ? 16.0f : 64.0f));
    //    TimeBar.transform.localScale = Vector3.Lerp( TimeBar.transform.localScale, targetTimeBarScale, 0.2f );
    //    CurrentBar.transform.localPosition = Vector3.Lerp( CurrentBar.transform.localPosition, initialCurrentBarPosition, 0.2f );
    //    if( NextCommand != null )
    //    {
    //        NextCommandText.color = Color.white * 0.8f;
    //    }
    //    else
    //    {
    //        NextCommandText.color = new Color( 1, 1, 1, (mt < 4 ? 0 : Mathf.Abs( Mathf.Sin( Mathf.PI * mt / (2 * (isWait ? 1 : Mathf.Pow( 2, 3 - Music.Just.bar ) )) ) )) );
    //    }
    //}

    void PushCommandButton( Vector3 pushingPosition )
    {
        PushingCommand = null;
        PlayerCommand selectedCommand = null;
        float minDistance = 99999;
		foreach( PlayerCommand command in GetLinkedCommands() )//GetComponentsInChildren<PlayerCommand>() ) 
        {
            if( command == null || command == IntroCommand /*|| !command.IsUsable()*/ ) continue;
            float d = (pushingPosition - command.transform.position).magnitude;
            if( d < minDistance )
            {
                minDistance = d;
                selectedCommand = command;
            }
        }
        if( selectedCommand != null && minDistance <= BUTTON_RADIUS )
        {
            selectedCommand.SetPush( true );
            PushingCommand = selectedCommand;
            SEPlayer.Play( "tickback" );
            TouchRect.SetColor( ColorManager.GetThemeColor( PushingCommand.themeColor ).Light );
            TouchRect.GrowSize = 0.2f;
            TouchRect.SetAnimationWidth( 2.0f, 0.1f );
            TouchRect.SetAnimationSize( 2.0f, 4.17f );
            Vector3 rectPos = new Vector3( pushingPosition.x, pushingPosition.y, TouchRect.transform.position.z ) - TouchRect.transform.parent.position;
            rectPos.x = Mathf.Clamp( rectPos.x, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
            rectPos.y = Mathf.Clamp( rectPos.y, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
            TouchRect.transform.localPosition = rectPos;
            //TouchRect.GetComponentInChildren<TextMesh>().text = PushingCommand.NameText.ToUpper();
            //TouchRect.GetComponentInChildren<TextMesh>().transform.localPosition
        }
    }

    /*
    void SelectNearestNode()
    {
        PlayerCommand selectedCommand = CurrentCommand;
        float minDistance = (SelectSpot.transform.position - CurrentCommand.transform.position).magnitude;
        foreach( PlayerCommand command in GetLinkedCommands() )
        {
            if( command == null || !command.IsUsable() ) continue;
            float d = (SelectSpot.transform.position - command.transform.position).magnitude;
            if( d < minDistance )
            {
                minDistance = d;
                selectedCommand = command;
            }
        }
        if( NextCommand != selectedCommand )//&& selectedCommand != IntroCommand )
        {
            Select( selectedCommand );
            SEPlayer.Play( selectedCommand == CurrentCommand ? "tickback" : "tick" );
        }
    }
    */

    IEnumerable<PlayerCommand> GetLinkedCommands()
    {
        if( GameContext.VoxSystem.state == VoxState.Invert && GameContext.VoxSystem.InvertTime == 1 )
        {
            foreach( PlayerCommand c in IntroCommand.LinkedCommands )
            {
                yield return c;
            }
        }
        else
        {
            yield return CurrentCommand;
            foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
            {
                yield return c;
            }
        }
    }

    void SetNextBlock()
    {
        if( Music.NextBlockName == "endro" )
        {
            return;
        }

        if( NextCommand == null || NextCommand == IntroCommand )
        {
            if( GameContext.VoxSystem.state == VoxState.Eclipse && GameContext.VoxSystem.IsReadyEclipse )
            {
                foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
                {
                    if( c is InvertCommand )
                    {
                        Select( c );
                        break;
                    }
                }
            }
            else if( IsInvert && !IsLastInvert )
            {
				NextCommand = (CurrentCommand != null && CurrentCommand.ParentCommand != null ? CurrentCommand.ParentCommand : CurrentCommand); ;
            }
            else
            {
                if( IsInvert ) CurrentCommand = IntroCommand;
                NextCommand = null;
                OldCommand = null;
                Music.SetNextBlock( "wait" );
            }
        }

        if( NextCommand != null )
        {
            OldCommand = (CurrentCommand != null && CurrentCommand.ParentCommand != null ? CurrentCommand.ParentCommand : CurrentCommand);
            if( OldCommand == NextCommand )
            {
                CommandLoopCount++;
                NextCommand = NextCommand.FindLoopVariation( CommandLoopCount );
                if( NextCommand == null ) Debug.LogError( OldCommand.name + CommandLoopCount.ToString() + "variation not found!" );
            }
            else
            {
                CommandLoopCount = 0;
            }
            //TODO: Add variation logic here( former block, parameter, etc... )

            Music.SetNextBlock( NextCommand.GetBlockName() );
        }
        
    }

    //void SetCommandIcons( GameObject CommandText, PlayerCommand command )
    //{
    //    EStatusIcon[] icons = new EStatusIcon[3];
    //    if( command == null )
    //    {
    //        icons[0] = icons[1] = icons[2] = EStatusIcon.none;
    //    }
    //    else
    //    {
    //        for( int i = 0; i < 3; i++ )
    //        {
    //            if( i < command.icons.Count )
    //            {
    //                icons[i] = command.icons[i];
    //            }
    //            else
    //            {
    //                icons[i] = EStatusIcon.none;
    //            }
    //        }
    //    }
    //    int index = 0;
    //    foreach( StatusIcon statusIcon in CommandText.GetComponentsInChildren<StatusIcon>() )
    //    {
    //        string iconName = icons[index].ToString();
    //        Sprite iconSpr = CommandIcons.Find( ( Sprite sprite ) => sprite.name == iconName );
    //        Color targetColor = Color.white;
    //        IconReactType reactType = IconReactType.None;
    //        if( iconSpr == null ) iconSpr = CommandIcons[CommandIcons.Count - 1];
    //        else
    //        {
    //            if( iconName.Contains( "W" ) || iconName.Contains( "L" ) || iconName.Contains( "F" ) )
    //            {
    //                targetColor = IconWLFMColor;
    //                reactType = IconReactType.OnMagic;
    //            }
    //            else if( iconName.Contains( "M" ) )
    //            {
    //                targetColor = IconWLFMColor;
    //                reactType = IconReactType.OnFaith;
    //            }
    //            else if( iconName.Contains( "A" ) )
    //            {
    //                targetColor = IconAPColor;
    //                reactType = IconReactType.OnAttack;
    //            }
    //            else if( iconName.Contains( "P" ) )
    //            {
    //                targetColor = IconAPColor;
    //                reactType = IconReactType.OnBrave;
    //            }
    //            else if( iconName.Contains( "D" ) )
    //            {
    //                targetColor = IconDSColor;
    //                reactType = IconReactType.OnDamage;
    //            }
    //            else if( iconName.Contains( "S" ) )
    //            {
    //                targetColor = IconDSColor;
    //                reactType = IconReactType.OnShield;
    //            }
    //            else if( iconName.Contains( "H" ) )
    //            {
    //                targetColor = IconHRColor;
    //                reactType = IconReactType.OnHeal;
    //            }
    //            else if( iconName.Contains( "R" ) )
    //            {
    //                targetColor = IconHRColor;
    //                reactType = IconReactType.OnRegene;
    //            }
    //            else if( iconName.Contains( "E" ) )
    //            {
    //                targetColor = IconHRColor;
    //                reactType = IconReactType.OnEsna;
    //            }
    //            else if( iconName.Contains( "V" ) )
    //            {
    //                targetColor = IconVColor;
    //                reactType = IconReactType.OnInvert;
    //            }
    //        }
    //        statusIcon.SetSprite( iconSpr, icons[index], targetColor, reactType );
    //        ++index;
    //    }
    //}

    public void CheckCommand()
    {
        EThemeColor themeColor = CurrentCommand.themeColor;
        if( OldCommand != null )
        {
            OldCommand.SetLink( false );
            foreach( PlayerCommand c in OldCommand.LinkedCommands )
            {
                c.SetLink( false );
            }
        }
        if( NextCommand != null )
        {
            CurrentCommand = NextCommand;
            themeColor = CurrentCommand.themeColor;
            CurrentRect.transform.parent = CurrentCommand.transform;
            CurrentRect.transform.localPosition = Vector3.forward;
            CurrentRect.transform.localScale = Vector3.one;
			NextRect.transform.localScale = Vector3.zero;
            foreach( PlayerCommand c in GetLinkedCommands() )
            {
                if( c != IntroCommand ) c.SetLink( true );
            }
            CurrentCommand.SetCurrent();
            NextCommand = null;
            //CurrentCommandText.text = CurrentCommand.name;
            //NextCommandText.text = initialNextText;
            //CurrentBar.transform.localPosition = initialNextBarPosition;
            //SetCommandIcons( CurrentCommandText.gameObject, CurrentCommand );
            //SetCommandIcons( NextCommandText.gameObject, null );

            if( IsLastInvert )
            {
                Select( IntroCommand );
                IntroCommand.Deselect();
            }
            AxisRing.SetAnimationSize( 6.7f, 7.5f );
            Panel.Hide();
        }
        else
        {
            //CurrentCommandText.text = "";
            //SetCommandIcons( CurrentCommandText.gameObject, null );
        }

        VoxState desiredState = GetDesiredVoxState();
        if( GameContext.VoxSystem.state != desiredState )
        {
            GameContext.VoxSystem.SetState( desiredState );
        }
		ColorManager.SetThemeColor(themeColor);

		if( CurrentCommand is InvertCommand )
		{
			AxisRing.SetColor(Color.black);
		}
		else
		{
			AxisRing.SetColor(ColorManager.Theme.Light);
		}
    }

    public PlayerCommand CheckAcquireCommand( int Level )
    {
        foreach( PlayerCommand command in CommandNodes )
        {
            if( command.AcquireLevel <= Level && !command.IsAcquired )
            {
                return command;
            }
        }
        return null;
    }
    public PlayerCommand CheckForgetCommand( int Level )
    {
        foreach( PlayerCommand command in CommandNodes )
        {
            if( command.AcquireLevel > Level && command.IsAcquired )
            {
                return command;
            }
        }
        return null;
    }

    public void OnBattleStart()
    {
        foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
        {
            command.SetLink( false );
        }
        OldCommand = null;
        NextCommand = null;
        CommandLoopCount = 0;
        Select( IntroCommand );
        CheckCommand();
        transform.rotation = Quaternion.Inverse( Quaternion.LookRotation( -IntroCommand.transform.localPosition ) ) * offsetRotation;
        // Quaternion.Inverse( CurrentCommand.transform.localRotation ) * offsetRotation;
        //targetTimeBarScale.x = 0;
        //CurrentBar.transform.localPosition = initialCurrentBarPosition;
        //TimeBar.transform.localScale = targetTimeBarScale;
    }

    public void Select( PlayerCommand command )
    {
		if( CurrentCommand == IntroCommand && command != IntroCommand )
		{
			IntroCommand.renderer.enabled = false;
			foreach( LineRenderer line in IntroCommand.linkLines )
			{
				line.renderer.enabled = false;
			}
		}
		else if( command == IntroCommand )
		{
			IntroCommand.renderer.enabled = true;
			foreach( LineRenderer line in IntroCommand.linkLines )
			{
				line.renderer.enabled = true;
			}
		}
        if( NextCommand != null ) NextCommand.Deselect();
        NextCommand = command;
		NextCommand.Select();
		NextRect.transform.parent = NextCommand.transform;
		NextRect.transform.localPosition = Vector3.forward;
		NextRect.transform.localScale = Vector3.one;

        //NextCommandText.text = NextCommand.name;
        //SetCommandIcons( NextCommandText.gameObject, NextCommand );
        targetRotation = Quaternion.Inverse( Quaternion.LookRotation( -command.transform.localPosition ) ) * offsetRotation;
		//Quaternion.Inverse( command.transform.localRotation ) * offsetRotation;
		if( command != IntroCommand ) SEPlayer.Play("select");
    }

	public void SelectInitialInvertCommand()
	{
		foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
		{
			if( c is InvertCommand )
			{
				if( NextCommand != c ) Panel.Show(SelectSpot.transform.position, c);
				break;
			}
		}
	}

    public void OnReactEvent( IconReactType type )
    {
        //foreach( StatusIcon statusIcon in CurrentCommandText.GetComponentsInChildren<StatusIcon>() )
        //{
        //    statusIcon.ReactEvent( type );
        //}
        if( type == IconReactType.OnInvert )
        {
            if( NextCommand != null && !(NextCommand is InvertCommand) ) NextCommand.Deselect();
            CurrentCommand.SetLink( false );
            foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
            {
                if( c != NextCommand )
                {
                    c.SetLink( c is InvertCommand );
                }
            }
        }
    }

    VoxState GetDesiredVoxState()
    {
        if( !GameContext.PlayerConductor.CanUseInvert || Music.CurrentBlockName == "wait" )
        {
            return VoxState.Sun;
        }
        else if( CurrentCommand.icons.Contains( EStatusIcon.V ) )
        {
            return VoxState.Eclipse;
        }
        else if( IsInvert )
        {
            return VoxState.Invert;
        }
        else
        {
            return VoxState.Sun;
        }
    }
}
