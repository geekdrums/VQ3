using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandGraph : MonoBehaviour {


    public GameObject EdgePrefab;
    public Command IntroCommand;
    public Command DefaultCommand;
    public Strategy BreakStrategy;
    public Command[] CommandNodes;
    public Strategy[] StrategyNodes;
    public GameObject VoxBall;
    public GameObject SelectSpot;
    public float MAX_LATITUDE;
    public float ROTATE_COEFF;

    public Command NextCommand { get; private set; }
    public Command CurrentCommand { get; private set; }

    bool IsLinkedToBreak { get { return ( CurrentCommand.ParentStrategy != null && CurrentCommand.ParentStrategy.IsLinkedTo( BreakStrategy ) ) || CurrentCommand.IsLinkedTo( BreakStrategy ); } }
    bool IsBreaking { get { return CurrentCommand.ParentStrategy == BreakStrategy; } }
    bool CanUseBreak { get { return GameContext.PlayerConductor.Level >= 8; } }
    int RemainBreakTime;

    Camera MainCamera;
    Timing AllowInputTime = new Timing( 3, 3, 2 );
    Vector3 oldMousePosition;
    Quaternion targetRotation;

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
        transform.rotation = targetRotation;
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
        if( Music.Just < AllowInputTime )
        {
            if( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.ShowBreak )
            {
                if( Music.IsJustChangedAt( 3 ) ) NextCommand = BreakStrategy.Commands[0];
            }
            else if( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.Break )
            {
                if( RemainBreakTime == 1 )
                {
                    if( Music.isJustChanged ) SelectNearestNode();
                    if( Music.IsJustChangedAt( 3, 2 ) )
                    {
                        GameContext.VoxonSystem.SetState( VoxonSystem.VoxonState.HideBreak );
                    }
                }
            }
            else
            {
                if( Music.isJustChanged ) SelectNearestNode();
            }
        }
        if( Music.IsJustChangedAt( AllowInputTime ) )
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
                //float eulerY = transform.rotation.eulerAngles.y;
                //transform.rotation *= Quaternion.Euler( 0, -eulerAngles.y, 0 ) * Quaternion.Euler( deltaV.y * ROTATE_COEFF, 0, 0 ) * Quaternion.Euler( 0, eulerAngles.y, 0 );
                //transform.rotation *= Quaternion.Euler( -eulerAngles.x, 0, -eulerAngles.z ) * Quaternion.Euler( 0, -deltaV.x * ROTATE_COEFF, 0 ) * Quaternion.Euler( eulerAngles.x, 0, eulerAngles.z );
                //transform.rotation *= (Quaternion.Inverse( transform.rotation )
                //    * Quaternion.AngleAxis( deltaV.y * ROTATE_COEFF, Vector3.right )
                //    * Quaternion.AngleAxis( deltaV.x * ROTATE_COEFF, -transform.up ) * transform.rotation);
                transform.rotation *= (Quaternion.Inverse( transform.rotation )
                    * Quaternion.AngleAxis( deltaV.x * ROTATE_COEFF, -transform.up ) * transform.rotation);
                Quaternion oldRotation = transform.rotation;
                transform.rotation *= (Quaternion.Inverse( transform.rotation )
                    * Quaternion.AngleAxis( deltaV.y * ROTATE_COEFF, Vector3.right ) * transform.rotation);
                //Vector3 up = transform.up;
                //transform.rotation = Quaternion.LookRotation( transform.forward, new Vector3( 0, up.y, up.z ) );
                Quaternion up = Quaternion.LookRotation( Vector3.up );
                Quaternion down = Quaternion.LookRotation( Vector3.down );
                Quaternion rotUp = Quaternion.LookRotation( transform.up );
                Quaternion rotDown = Quaternion.LookRotation( -transform.up );
                float angleUp = Quaternion.Angle( rotUp, up );
                float angleDown = Quaternion.Angle( rotDown, down );
                float angle = Mathf.Min( angleUp, angleDown );
                if( angle > MAX_LATITUDE )
                {
                    transform.rotation = oldRotation;
                    //transform.rotation *= Quaternion.FromToRotation( transform.up, Quaternion.RotateTowards( up, rotUp, MAX_LATITUDE ) * Vector3.up );
                }
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
            float d = (SelectSpot.transform.position - command.transform.position).magnitude;
            if( d < minDistance )
            {
                minDistance = d;
                selectedCommand = command;
            }
        }
        if( NextCommand != selectedCommand )
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

        if( IsBreaking )
        {
            --RemainBreakTime;
            if( RemainBreakTime == 0 )
            {
                //if ( NextStrategy == EStrategy.Break )
                //{
                //    NextStrategy = EStrategy.Magic;
                //    NextCommand = Strategies[(int)NextStrategy].Commands[0];
                //}
                Music.SetNextBlock( NextCommand.GetBlockName() );
            }
        }
        else
        {
            bool willShowBreak = false;
            if( CanUseBreak )
            {
                willShowBreak = GameContext.VoxonSystem.DetermineWillShowBreak( NextCommand.GetWillGainVoxon() );
                if( willShowBreak )
                {
                    RemainBreakTime = 2;
                }
            }
            Music.SetNextBlock( NextCommand.GetBlockName() + (willShowBreak ? "Trans" : "") );
        }
    }

    public void CheckCommand()
    {
        Command OldCommand = CurrentCommand;
        if( OldCommand != null && NextCommand != OldCommand )
        {
            OldCommand.SetLink( false );
            foreach( Command c in OldCommand.LinkedCommands )
            {
                c.SetLink( false );
            }
        }
        if( NextCommand != OldCommand )
        {
            foreach( Command c in NextCommand.LinkedCommands )
            {
                c.SetLink( true );
            }
        }

        CurrentCommand = NextCommand;
        CurrentCommand.SetCurrent();

        VoxonSystem.VoxonState desiredState = GetDesiredVoxonState();
        if( GameContext.VoxonSystem.state != desiredState )
        {
            GameContext.VoxonSystem.SetState( desiredState );
        }
    }

    public void OnBattleStart()
    {
        Select( IntroCommand );
        CheckCommand();
        NextCommand = DefaultCommand;
        transform.rotation = targetRotation;
    }

    void Select( Command command )
    {
        if( NextCommand != null ) NextCommand.Deselect();
        NextCommand = command;
        NextCommand.Select();
        targetRotation = Quaternion.Inverse( command.transform.localRotation );
    }


    VoxonSystem.VoxonState GetDesiredVoxonState()
    {
        if( !CanUseBreak ) return VoxonSystem.VoxonState.Hide;
        if( IsLinkedToBreak )
        {
            if( GameContext.VoxonSystem.state != VoxonSystem.VoxonState.ShowBreak )
            {
                return VoxonSystem.VoxonState.Show;
            }
            else
            {
                return VoxonSystem.VoxonState.ShowBreak;
            }
        }
        else if( IsBreaking )
        {
            return VoxonSystem.VoxonState.Break;
        }
        else
        {
            return VoxonSystem.VoxonState.Hide;
        }
    }
}
