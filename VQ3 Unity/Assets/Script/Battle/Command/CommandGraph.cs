using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum VoxButton
{
    None,
    Ball,
    ArrowRight,
    ArrowLeft,
    Enter,
    Count
}

public class CommandGraph : MonoBehaviour {

    public GameObject EdgePrefab;
    public Command IntroCommand;
    public Command DefaultCommand;
    public Strategy InvertStrategy;
    public GameObject VoxBall;
    public GameObject SelectSpot;
    public GameObject RightArrow;
    public float MAX_LATITUDE;
    public float ROTATE_COEFF;

    public List<Command> CommandNodes { get; private set; }
    public List<Strategy> StrategyNodes { get; private set; }
    public Command NextCommand { get; private set; }
    public Command CurrentCommand { get; private set; }
    public VoxButton CurrentButton { get; private set; }

    bool IsLinkedToInvert { get { return ( CurrentCommand.ParentStrategy != null && CurrentCommand.ParentStrategy.IsLinkedTo( InvertStrategy ) ) || CurrentCommand.IsLinkedTo( InvertStrategy ); } }
    bool IsInvert { get { return CurrentCommand.ParentStrategy == InvertStrategy; } }
    int RemainInvertTime;

    Camera MainCamera;
    Timing AllowInputEnd = new Timing( 3, 3, 2 );
    Timing AllowInputStart = new Timing( 0, 0, 1 );
    Vector3 oldMousePosition;
    Quaternion targetRotation;
    Quaternion offsetRotation;
    Vector3 initialPosition;
    Vector3 initialRightArrowPosition;

	// Use this for initialization
	void Start () {
        MainCamera = GameObject.Find( "Main Camera" ).GetComponent<Camera>();

        StrategyNodes = new List<Strategy>();
        foreach( Strategy strategy in GetComponentsInChildren<Strategy>() )
        {
            StrategyNodes.Add( strategy );
            foreach( MonoNode link in strategy.links )
            {
                InstantiateLine( strategy, link );
            }
            foreach( Command command in strategy.Commands )
            {
                foreach( MonoNode link in command.links )
                {
                    InstantiateLine( command, link );
                }
                command.SetLink( false );
            }
        }
        CommandNodes = new List<Command>();
        foreach( Command command in GetComponentsInChildren<Command>() )
        {
            CommandNodes.Add( command );
            foreach( MonoNode link in command.links )
            {
                InstantiateLine( command, link );
            }
            command.SetLink( false );
        }
        foreach( MonoNode link in IntroCommand.links )
        {
            InstantiateLine( IntroCommand, link );
        }
        offsetRotation = Quaternion.LookRotation( transform.position - SelectSpot.transform.position );
        initialPosition = transform.localPosition;
        initialRightArrowPosition = RightArrow.transform.localPosition;
        CurrentButton = VoxButton.None;
    }

    void InstantiateLine( MonoNode from, MonoNode to )
    {
        LineRenderer edge = (Instantiate( EdgePrefab ) as GameObject).GetComponent<LineRenderer>();
        edge.transform.position = from.transform.position;
        edge.transform.parent = from.transform;
        Vector3 direction = to.transform.position - from.transform.position;
        edge.SetPosition( 0, direction.normalized * from.radius );
        edge.SetPosition( 1, direction.normalized * ( direction.magnitude - to.radius ) );
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInput();


        if( GameContext.CurrentState == GameState.Intro )
        {
            if( Music.GetNextBlockName() == "intro" && Music.Just.totalUnit > 4 )
            {
                if( Music.isJustChanged && NextCommand != IntroCommand && !Input.GetMouseButton( 0 ))
                {
                    SetNextBlock();
                    SEPlayer.Play( "select" );
                }
            }
            else return;
        }
        else if( GameContext.CurrentState == GameState.Continue )
        {
            if( GameContext.CurrentState == GameState.Continue && ( !Music.IsPlaying() || Music.Just.totalUnit > 4 )
                && Input.GetMouseButtonUp( 0 ) && (transform.localPosition - initialPosition).magnitude > 0.03f )
            {
                GameContext.ChangeState( GameState.Intro );
            }
            return;
        }

        if( AllowInputStart <= Music.Just && Music.Just < AllowInputEnd )
        {
            if( GameContext.VoxSystem.state == VoxState.Eclipse )
            {
            }
            else if( GameContext.VoxSystem.state == VoxState.Invert )
            {
                if( RemainInvertTime == 1 )
                {
                    if( Music.isJustChanged ) SelectNearestNode();
                    if( Music.IsJustChangedAt( 3, 2 ) )
                    {
                        GameContext.VoxSystem.SetState( VoxState.Revert );
                    }
                }
            }
            else
            {
                if( Music.isJustChanged ) SelectNearestNode();
            }
        }
        if( Music.IsJustChangedAt( AllowInputEnd ) )
        {
            SetNextBlock();
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

        if( Input.GetMouseButton( 0 ) && CurrentButton == VoxButton.Ball )
        {
            Vector3 deltaV = Input.mousePosition - oldMousePosition;
            //Quaternion oldRotation = transform.rotation;
            transform.rotation *= (Quaternion.Inverse( transform.rotation )
                * Quaternion.AngleAxis( deltaV.y * ROTATE_COEFF, Vector3.right )
                * Quaternion.AngleAxis( deltaV.x * ROTATE_COEFF, -transform.up ) * transform.rotation);
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
            oldMousePosition = Input.mousePosition;

            transform.localPosition = Vector3.MoveTowards( transform.localPosition, initialPosition + Vector3.forward * 0.2f, 0.1f );
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards( transform.localPosition, initialPosition, 0.1f );
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 0.1f );
        }

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

    void SelectNearestNode()
    {
        Command selectedCommand = CurrentCommand;
        float minDistance = (SelectSpot.transform.position - CurrentCommand.transform.position).magnitude;
        foreach( Command command in CurrentCommand.LinkedCommands )
        {
            if( !command.IsUsable() || command.ParentStrategy == InvertStrategy ) continue;
            float d = (SelectSpot.transform.position - command.transform.position).magnitude;
            if( d < minDistance )
            {
                minDistance = d;
                selectedCommand = command;
            }
        }
        if( NextCommand != selectedCommand && selectedCommand != IntroCommand )
        {
            Select( selectedCommand );
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
            Select( DefaultCommand );
        }
        else if( GameContext.VoxSystem.state == VoxState.Eclipse )
        {
            Select( InvertStrategy.Commands[0] );
        }

        if( IsInvert )
        {
            --RemainInvertTime;
            if( RemainInvertTime == 0 )
            {
                if( NextCommand.ParentStrategy == InvertStrategy )
                {
                    foreach( Command c in InvertStrategy.LinkedCommands )
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
                    RemainInvertTime = 2;
                }
            }
            Music.SetNextBlock( NextCommand.GetBlockName() + (willEclipse ? "Trans" : "") );
        }
    }

    public void CheckCommand()
    {
        if( NextCommand != CurrentCommand )
        {
            if( CurrentCommand != null )
            {
                CurrentCommand.SetLink( false );
                foreach( Command c in CurrentCommand.LinkedCommands )
                {
                    c.SetLink( false );
                }
            }
            foreach( Command c in NextCommand.LinkedCommands )
            {
                c.SetLink( true );
            }
        }

        CurrentCommand = NextCommand;
        CurrentCommand.SetCurrent();

        VoxState desiredState = GetDesiredVoxState();
        if( GameContext.VoxSystem.state != desiredState )
        {
            GameContext.VoxSystem.SetState( desiredState );
        }
    }

    public Command CheckAcquireCommand( int Level )
    {
        foreach( Command command in CommandNodes )
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
        foreach( Command command in CommandNodes )
        {
            command.SetLink(false);
        }
        NextCommand = null;
        Select( IntroCommand );
        CheckCommand();
        //transform.rotation = targetRotation;
    }

    public void Select( Command command )
    {
        if( NextCommand != null ) NextCommand.Deselect();
        NextCommand = command;
        NextCommand.Select();
        targetRotation = Quaternion.Inverse( command.transform.localRotation ) * offsetRotation;
        GameContext.EnemyConductor.OnNextCommandChanged( NextCommand );
    }


    VoxState GetDesiredVoxState()
    {
        if( !GameContext.PlayerConductor.CanUseInvert ) return VoxState.Sun;
        if( IsLinkedToInvert )
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
