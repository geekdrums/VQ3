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

    public GameObject EdgePrefab;
    public PlayerCommand IntroCommand;
    public GameObject VoxBall;
    public GameObject RightArrow;
    public GameObject TimeBar;
    public GameObject CurrentBar;
    public GameObject NextBar;
    public float MAX_LATITUDE;
    public float ROTATE_COEFF;
    public float BUTTON_RADIUS;
    public bool UPDATE_BUTTON;
    public TextMesh CurrentCommandText;
    public TextMesh NextCommandText;
    public List<Sprite> CommandIcons;

    public List<PlayerCommand> CommandNodes { get; private set; }
    public List<Strategy> StrategyNodes { get; private set; }
    public PlayerCommand NextCommand { get; private set; }
    public PlayerCommand PushingCommand { get; private set; }
    public PlayerCommand CurrentCommand { get; private set; }
    public PlayerCommand OldCommand { get; private set; }
    public VoxButton CurrentButton { get; private set; }

    bool IsInvert { get { return CurrentCommand is InvertCommand; } }
    int RemainInvertTime;
    int CommandLoopCount;

    Camera MainCamera;
    Timing AllowInputEnd = new Timing( 3, 3, 3 );
    Timing WaitInputEnd = new Timing( 0, 3, 3 );
    Timing AllowInputStart = new Timing( 0, 0, 1 );
    Vector3 oldMousePosition;
    Quaternion targetRotation;
    Vector3 initialRightArrowPosition;
    Vector3 ballTouchStartPosition;
    string initialNextText;
    Vector3 maxTimeBarScale;
    Vector3 targetTimeBarScale;
    Vector3 initialCurrentBarPosition;
    Vector3 initialNextBarPosition;

	// Use this for initialization
	void Start () {
        MainCamera = GameObject.Find( "Main Camera" ).GetComponent<Camera>();
        StrategyNodes = new List<Strategy>();
        StrategyNodes.AddRange( GetComponentsInChildren<Strategy>() );
        CommandNodes = new List<PlayerCommand>();
        CommandNodes.AddRange( GetComponentsInChildren<PlayerCommand>() );
        //offsetRotation = Quaternion.LookRotation( transform.position - SelectSpot.transform.position );
        //initialPosition = transform.localPosition;
        initialRightArrowPosition = RightArrow.transform.localPosition;
        CurrentButton = VoxButton.None;
        CurrentCommand = IntroCommand;
        initialNextText = NextCommandText.text;
        maxTimeBarScale = new Vector3( CurrentBar.transform.localScale.x, TimeBar.transform.localScale.y, TimeBar.transform.localScale.z );
        targetTimeBarScale = new Vector3( 0, TimeBar.transform.localScale.y, TimeBar.transform.localScale.z );
        TimeBar.transform.localScale = targetTimeBarScale;
        initialCurrentBarPosition = CurrentBar.transform.localPosition;
        initialNextBarPosition = NextBar.transform.localPosition;
    }

    void InitializeLinks()
    {
        StrategyNodes = new List<Strategy>();
        StrategyNodes.AddRange( GetComponentsInChildren<Strategy>() );
        foreach( Strategy strategy in StrategyNodes )
        {
            foreach( IVoxNode link in strategy.links )
            {
                InstantiateLine( strategy, link );
            }
            foreach( PlayerCommand command in strategy.Commands )
            {
                foreach( IVoxNode link in command.links )
                {
                    InstantiateLine( command, link );
                }
                command.SetLink( false );
            }
        }
        CommandNodes = new List<PlayerCommand>();
        CommandNodes.AddRange( GetComponentsInChildren<PlayerCommand>() );
        foreach( PlayerCommand command in CommandNodes )
        {
            command.SetLink( false );
            if( command.ParentCommand != null ) continue;
            foreach( IVoxNode link in command.links )
            {
                InstantiateLine( command, link );
            }
        }
        foreach( IVoxNode link in IntroCommand.links )
        {
            InstantiateLine( IntroCommand, link );
        }
    }

    void InstantiateLine( IVoxNode from, IVoxNode to )
    {
        LineRenderer edge = (Instantiate( EdgePrefab ) as GameObject).GetComponent<LineRenderer>();
        edge.transform.position = transform.position;//from.Transform().position;
        edge.transform.parent = from.Transform();
        Vector3 direction = to.Transform().position - from.Transform().position;
        //edge.SetPosition( 0, direction.normalized * from.Radius() );
        //edge.SetPosition( 1, direction.normalized * ( direction.magnitude - to.Radius() ) );
        edge.SetVertexCount( 8 );
        for( int i = 0; i < 8; i++ )
        {
            edge.SetPosition( i, transform.rotation * Vector3.Slerp( from.Transform().localPosition, to.Transform().localPosition, (3 + i) / 12.0f ) );
        }
    }
    
#if UNITY_EDITOR
    enum CommandListProperty
    {
        Level,
        Category,
        Name,
        Icon,
        Music,
        Link1,
        Link2,
        Link3,
        Link4,
        Link5,
        Longitude,
        Latitude,
    }
    void UpdateCommandList()
    {
        string path = Application.streamingAssetsPath + "/VQ3List.csv";
        StreamReader reader = File.OpenText( path );
        if( reader != null )
        {
            CommandNodes = new List<PlayerCommand>();
            CommandNodes.AddRange( GetComponentsInChildren<PlayerCommand>() );
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
                    GameObject commandObj = new GameObject( commandName, ( categoryName == "V" ? typeof(InvertCommand) : typeof(PlayerCommand) ), typeof(MeshRenderer), typeof(TextMesh) );
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
                playerCommand.GetComponent<TextMesh>().text = propertyTexts[(int)CommandListProperty.Icon];
                playerCommand.GetComponent<TextMesh>().fontSize = 10;
                string iconStr = propertyTexts[(int)CommandListProperty.Icon];
                playerCommand.icons = new List<StatusIcon>();
                for( int i = 0; i < (int)StatusIcon.Count; i++ )
                {
                    if( iconStr.Contains( ((StatusIcon)i).ToString() ) )
                    {
                        playerCommand.icons.Add( (StatusIcon)i );
                        iconStr = iconStr.Replace( ((StatusIcon)i).ToString(), "" );
                        if( iconStr == "" ) break;
                    }
                }
                
                if( playerCommand.icons.Contains( StatusIcon.DD ) )
                {
                    playerCommand.PhysicDefend = 70;
                    playerCommand.MagicDefend = 70;
                }
                else if( playerCommand.icons.Contains( StatusIcon.D ) )
                {
                    playerCommand.PhysicDefend = 45;
                    playerCommand.MagicDefend = 45;
                }
                if( playerCommand.icons.Contains( StatusIcon.HH ) )
                {
                    playerCommand.HealPercent = 40;
                }
                else if( playerCommand.icons.Contains( StatusIcon.H ) )
                {
                    playerCommand.HealPercent = 20;
                }
                playerCommand.OnValidatePosition();
                if( propertyTexts[(int)CommandListProperty.Music] == "intro" )
                {
                    IntroCommand = playerCommand;
                }
            }
            foreach( PlayerCommand playerCommand in CommandNodes )
            {
                if( !LinkDictionary.ContainsKey( playerCommand ) )
                {
                    continue;
                }
                playerCommand.links = new List<MonoBehaviour>();
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
                foreach( LineRenderer commandEdge in playerCommand.GetComponentsInChildren<LineRenderer>() )
                {
                    DestroyImmediate( commandEdge.gameObject );
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
                && Input.GetMouseButtonUp( 0 ) && CurrentButton == VoxButton.Reset )
            {
                GameContext.ChangeState( GameState.Intro );
            }
            break;
        case GameState.Intro:
            if( Music.GetNextBlockName() == "intro" && Music.Just.totalUnit > 4 )
            {
                //if( Music.isJustChanged ) SelectNearestNode();
                if( Music.isJustChanged && NextCommand != null && NextCommand != IntroCommand && !Input.GetMouseButton( 0 ) )
                {
                    SetNextBlock();
                    SEPlayer.Play( "select" );
                }
            }
            if( Music.IsJustChangedAt( AllowInputEnd ) )
            {
                SetNextBlock();
            }
            UpdateCommandLine();
            break;
        case GameState.Battle:
            if( AllowInputStart <= Music.Just && Music.Just < AllowInputEnd )
            {
                //if( Music.isJustChanged ) SelectNearestNode();
                if( GameContext.VoxSystem.state == VoxState.Invert )
                {
                    if( Music.IsJustChangedAt( 3, 2 ) && RemainInvertTime == 1 )
                    {
                        GameContext.VoxSystem.SetState( VoxState.Revert );
                    }
                }
            }
            if( Music.IsJustChangedAt( AllowInputEnd ) || 
                ( Music.GetCurrentBlockName() == "wait" && Music.IsJustChangedAt( WaitInputEnd ) ) )
            {
                SetNextBlock();
            }
            UpdateCommandLine();
            break;
        }
    }

    void UpdateInput()
    {
        if( Input.GetMouseButtonDown( 0 ) ) oldMousePosition = Input.mousePosition;
        Ray ray = MainCamera.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
        Physics.Raycast( ray.origin, ray.direction, out hit, Mathf.Infinity );

        CurrentButton = VoxButton.None;
        if( hit.collider == VoxBall.collider )
        {
            CurrentButton = VoxButton.Ball;
        }
        else if( hit.collider == RightArrow.collider )
        {
            CurrentButton = VoxButton.ArrowRight;
        }

        if( CurrentButton == VoxButton.Ball )
        {
            if( Input.GetMouseButtonDown( 0 ) )
            {
                PushCommandButton( hit.collider.ClosestPointOnBounds( hit.point ) );
                ballTouchStartPosition = hit.point;
            }
            if( Input.GetMouseButton( 0 ) )
            {
                Vector3 deltaV = Input.mousePosition - oldMousePosition;
                transform.rotation *= (Quaternion.Inverse( transform.rotation )
                    * Quaternion.AngleAxis( deltaV.y * ROTATE_COEFF, Vector3.right )
                    * Quaternion.AngleAxis( deltaV.x * ROTATE_COEFF, -transform.up ) * transform.rotation);
                oldMousePosition = Input.mousePosition;

                if( PushingCommand != null && (ballTouchStartPosition - hit.point).magnitude > BUTTON_RADIUS/2 )
                {
                    PushingCommand.SetPush( false );
                    PushingCommand = null;
                }
            }
            if( Input.GetMouseButtonUp( 0 ) )
            {
                if( PushingCommand != null )
                {
                    if( PushingCommand.IsSelected )
                    {
                        PushingCommand.Deselect();
                        NextCommand = null;
                        NextCommandText.text = initialNextText;
                        SEPlayer.Play( "tickback" );
                        SetCommandIcons( NextCommandText.gameObject, null );
                    }
                    else
                    {
                        Select( PushingCommand );
                        SEPlayer.Play( "tick" );
                    }
                }
            }
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

        if( CurrentButton == VoxButton.ArrowRight )
        {
            RightArrow.transform.localPosition = Vector3.MoveTowards( RightArrow.transform.localPosition,
                initialRightArrowPosition + ( Input.GetMouseButton( 0 ) ? Vector3.forward * 0.3f : Vector3.zero ), 0.1f );
            if( Input.GetMouseButtonDown( 0 ) )
            {
                GameContext.EnemyConductor.OnArrowPushed( false );
            }
        }
    }


    void UpdateCommandLine()
    {
        bool isWait = Music.GetCurrentBlockName() == "wait";
        float mt = (float)Music.MusicalTime;
        targetTimeBarScale.x = maxTimeBarScale.x * (mt / (isWait ? 16.0f : 64.0f));
        TimeBar.transform.localScale = Vector3.Lerp( TimeBar.transform.localScale, targetTimeBarScale, 0.2f );
        CurrentBar.transform.localPosition = Vector3.Lerp( CurrentBar.transform.localPosition, initialCurrentBarPosition, 0.2f );
        if( NextCommand != null )
        {
            NextCommandText.color = Color.white;
        }
        else
        {
            NextCommandText.color = new Color( 1, 1, 1, (mt < 4 ? 0 : Mathf.Abs( Mathf.Sin( Mathf.PI * mt / (2 * (isWait ? 1 : Mathf.Pow( 2, 3 - Music.Just.bar ) )) ) )) );
        }
    }

    void PushCommandButton( Vector3 pushingPosition )
    {
        PushingCommand = null;
        PlayerCommand selectedCommand = null;
        float minDistance = 99999;
        foreach( PlayerCommand command in GetLinkedCommands() )
        {
            if( command == null || !command.IsUsable() ) continue;
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
        if( GameContext.VoxSystem.state == VoxState.Invert && RemainInvertTime == 1 )
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
            //yield return IntroCommand;
        }
    }

    void SetNextBlock()
    {
        if( Music.GetNextBlockName() == "endro" )
        {
            return;
        }


        if( NextCommand == IntroCommand )
        {
            Music.SetNextBlock( "intro" );//runaway
            return;
        }

        if( NextCommand == null )
        {
            OldCommand = null;
            Music.SetNextBlock( "wait" );
        }
        else
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
                CommandLoopCount = 1;
            }
            //TODO: Add variation logic here( former block, parameter, etc... )

            Music.SetNextBlock( NextCommand.GetBlockName() );
        }
        /*
        if( IsInvert )
        {
            --RemainInvertTime;
            if( RemainInvertTime == 0 )
            {
                if( NextCommand.ParentStrategy == InvertStrategy )
                {
                    foreach( PlayerCommand c in InvertStrategy.LinkedCommands )
                    {
                        if( c.ParentStrategy != InvertStrategy )
                        {
                            NextCommand = c;
                            break;
                        }
                    }
                }
                Music.SetNextBlock( NextCommand.GetBlockName() );
            }
        }
        else
        {
            bool willEclipse = false;
            if( GameContext.PlayerConductor.CanUseInvert && ( NextCommand.IsLinkedTo( InvertStrategy ) || ( NextCommand.ParentStrategy != null && NextCommand.ParentStrategy.IsLinkedTo( InvertStrategy ) ) ) )
            {
                willEclipse = GameContext.VoxSystem.WillEclipse;
                if( willEclipse )
                {
                    RemainInvertTime = GameContext.VoxSystem.InvertTime;
                }
            }
            Music.SetNextBlock( NextCommand.GetBlockName() + (willEclipse ? "Trans" : "") );
        }
        */
    }

    void SetCommandIcons( GameObject CommandText, PlayerCommand command )
    {
        StatusIcon[] icons = new StatusIcon[3];
        if( command == null )
        {
            icons[0] = icons[1] = icons[2] = StatusIcon.none;
        }
        else
        {
            for( int i = 0; i < 3; i++ )
            {
                if( i < command.icons.Count )
                {
                    icons[2 - i] = command.icons[i];
                }
                else
                {
                    icons[2 - i] = StatusIcon.none;
                }
            }
        }
        int index = 0;
        foreach( SpriteRenderer spriteRenderer in CommandText.GetComponentsInChildren<SpriteRenderer>() )
        {
            spriteRenderer.sprite = CommandIcons.Find( ( Sprite sprite ) => sprite.name == icons[index].ToString() );
            if( spriteRenderer.sprite == null ) spriteRenderer.sprite = CommandIcons[CommandIcons.Count-1];
            ++index;
        }
    }

    public void CheckCommand()
    {
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
            foreach( PlayerCommand c in GetLinkedCommands() )
            {
                c.SetLink( true );
            }
            CurrentCommand.SetCurrent();
            NextCommand = null;
            CurrentCommandText.text = CurrentCommand.name;
            NextCommandText.text = initialNextText;
            CurrentBar.transform.localPosition = initialNextBarPosition;
            SetCommandIcons( CurrentCommandText.gameObject, CurrentCommand );
            SetCommandIcons( NextCommandText.gameObject, null );
        }
        else
        {
            CurrentCommandText.text = "";
            SetCommandIcons( CurrentCommandText.gameObject, null );
        }

        VoxState desiredState = GetDesiredVoxState();
        if( GameContext.VoxSystem.state != desiredState )
        {
            GameContext.VoxSystem.SetState( desiredState );
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

    public void OnBattleStart()
    {
        foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
        {
            command.SetLink(false);
        }
        OldCommand = null;
        NextCommand = null;
        CommandLoopCount = 0;
        Select( IntroCommand );
        CheckCommand();
        transform.rotation = Quaternion.Inverse( CurrentCommand.transform.localRotation );// *offsetRotation;
        targetTimeBarScale.x = 0;
        CurrentBar.transform.localPosition = initialCurrentBarPosition;
        TimeBar.transform.localScale = targetTimeBarScale;
    }

    public void Select( PlayerCommand command )
    {
        if( NextCommand != null ) NextCommand.Deselect();
        NextCommand = command;
        NextCommand.Select();
        NextCommandText.text = NextCommand.name;
        SetCommandIcons( NextCommandText.gameObject, NextCommand );
        //targetRotation = Quaternion.Inverse( command.transform.localRotation ) * offsetRotation;
    }


    VoxState GetDesiredVoxState()
    {
        if( !GameContext.PlayerConductor.CanUseInvert ) return VoxState.Sun;
        /*if( IsLinkedToInvert )
        {
            if( GameContext.VoxSystem.WillEclipse )
            {
                return VoxState.Eclipse;
            }
            else
            {
                return VoxState.Sun;
            }
        }
        else */if( IsInvert )
        {
            return VoxState.Invert;
        }
        else
        {
            return VoxState.Sun;
        }
    }
}
