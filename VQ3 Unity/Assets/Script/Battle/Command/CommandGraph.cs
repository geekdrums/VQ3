using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandGraph : MonoBehaviour {

    public GameObject EdgePrefab;
    public GameObject VoxSystem;
    public Command IntroCommand;
    public Strategy[] StrategyNodes;

    Vector3 targetPosition;
    Command CurrentCommand;

	// Use this for initialization
	void Start () {
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
        foreach( MonoNode link in IntroCommand.links )
        {
            InstantiateLine( IntroCommand, link );
        }
        targetPosition = transform.position;
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
	void Update () {
        transform.position = Vector3.Lerp( transform.position, targetPosition, 0.1f );
	}

    public void Select( Command command )
    {
        Command OldCommand = CurrentCommand;
        CurrentCommand = command;
        if( OldCommand != null && CurrentCommand != OldCommand )
        {
            OldCommand.SetLink( false );
            foreach( Command c in OldCommand.LinkedCommands )
            {
                c.SetLink( false );
            }
        }
        if( CurrentCommand != OldCommand )
        {
            foreach( Command c in CurrentCommand.LinkedCommands )
            {
                c.SetLink( true );
            }
        }
        CurrentCommand.SetCurrent();
        targetPosition = VoxSystem.transform.position + (transform.position - command.transform.position);
        targetPosition.z = transform.position.z;
    }

    public void OnBattleStart()
    {
        Select( IntroCommand );
        transform.position = targetPosition;
    }
}
