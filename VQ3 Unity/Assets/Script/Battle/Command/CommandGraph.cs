using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandGraph : MonoBehaviour {


    public GameObject EdgePrefab;
    public Command IntroCommand;
    public Command DefaultCommand;
    public Strategy InvertStrategy;
    public Command[] CommandNodes;
    public Strategy[] StrategyNodes;
    public GameObject VoxBall;
    public GameObject SelectSpot;
    public float MAX_LATITUDE;
    public float ROTATE_COEFF;

    public Command NextCommand { get; private set; }
    public Command CurrentCommand { get; private set; }

    bool IsLinkedToInvert { get { return ( CurrentCommand.ParentStrategy != null && CurrentCommand.ParentStrategy.IsLinkedTo( InvertStrategy ) ) || CurrentCommand.IsLinkedTo( InvertStrategy ); } }
    bool IsInvert { get { return CurrentCommand.ParentStrategy == InvertStrategy; } }
    bool CanUseInvert { get { return GameContext.PlayerConductor.Level >= 8; } }
    int RemainInvertTime;

    Camera MainCamera;
    Timing AllowInputEnd = new Timing( 3, 3, 2 );
    Timing AllowInputStart = new Timing( 0, 0, 1 );
    Vector3 oldMousePosition;
    Quaternion targetRotation;
    Quaternion offsetRotation;

	// Use this for initialization
	void Start () {
        MainCamera = GameObject.Find( "Main Camera" ).GetComponent<Camera>();

        foreach( Strategy strategy in StrategyNodes )
        {
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
        foreach( Command command in CommandNodes )
        {
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
        transform.rotation = offsetRotation;
        targetRotation = offsetRotation;
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
        if( Input.GetMouseButton( 0 ) )
        {
            Ray ray = MainCamera.ScreenPointToRay( Input.mousePosition );
            RaycastHit hit;
            if( Physics.Raycast( ray.origin, ray.direction, out hit, Mathf.Infinity )
                && hit.collider == VoxBall.collider )
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
            }
            oldMousePosition = Input.mousePosition;
        }
        else
        {
            transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 0.1f );
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
            if( CanUseInvert && NextCommand.ParentStrategy != null && NextCommand.ParentStrategy.IsLinkedTo( InvertStrategy ) )
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

    public void OnBattleStart()
    {
        Select( IntroCommand );
        CheckCommand();
        transform.rotation = targetRotation;
    }

    void Select( Command command )
    {
        if( NextCommand != null ) NextCommand.Deselect();
        NextCommand = command;
        NextCommand.Select();
        targetRotation = Quaternion.Inverse( command.transform.localRotation ) * offsetRotation;
    }


    VoxState GetDesiredVoxState()
    {
        if( !CanUseInvert ) return VoxState.Sun;
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
