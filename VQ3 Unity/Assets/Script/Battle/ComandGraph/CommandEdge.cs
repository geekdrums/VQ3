using UnityEngine;
using System;
using System.Collections;

public enum EdgeState
{
	Linked,
	Prelinked,
	Unlinked,
	Unacquired,
	DontKnow,
	None,
}

public class CommandEdge : MonoBehaviour {
	static float LinkedLineWidth = 0.2f;
	static float PrelinkedLineWidth = 0.1f;
	static float UnlinkedLineWidth = 0.0f;
	static float NotSelectedLineWidth = 0.1f;

	LineRenderer line_;
	EdgeState state_ = EdgeState.None;
	ColorSourceBase colorSource_;
	public bool IsInvert { get { return Command1 is InvertCommand || Command2 is InvertCommand; } }
	public bool IsUsable { get { return Command1.IsUsable() && Command2.IsUsable(); } }
	public PlayerCommand Command1;
	public PlayerCommand Command2;

	// Use this for initialization
	void Awake()
	{
		line_ = GetComponent<LineRenderer>();
		colorSource_ = GetComponent<ColorSourceBase>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnValidate()
	{
		if( Command1 != null && Command2 != null )
		{
			line_ = GetComponent<LineRenderer>();
			colorSource_ = GetComponent<ColorSourceBase>();
			colorSource_.SetState("EdgeState", state_.ToString());
			UpdatePosition();
		}
	}

	public void SetEnabled( bool enable )
	{
		line_.GetComponent<Renderer>().enabled = enable;
	}

	public void SetParent( PlayerCommand command )
	{
		if( Command1 ==null ) Command1 = command;
		else if( Command2 == null ) Command2 = command;
	}

	public EdgeState State
	{
		get { return state_; }
		set
		{
			state_ = value;
			float width = 0;
			switch( state_ )
			{
				case EdgeState.Linked:
					width = LinkedLineWidth;
					if( Command1.state == CommandState.Selected || Command2.state == CommandState.Selected )
					{
						width = LinkedLineWidth;
					}
					else if( Command1.state == CommandState.NotSelected || Command2.state == CommandState.NotSelected )
					{
						width = NotSelectedLineWidth;
					}
					break;
				case EdgeState.Prelinked:
					width = PrelinkedLineWidth;
					break;
				case EdgeState.Unlinked:
					width = UnlinkedLineWidth;
					break;
				case EdgeState.Unacquired:
					width = UnlinkedLineWidth;
					break;
				case EdgeState.DontKnow:
					break;
			}
			if( line_ == null )
			{
				line_ = GetComponent<LineRenderer>();
				colorSource_ = GetComponent<ColorSourceBase>();
			}
			line_.startWidth = width;
			line_.endWidth = width;
			colorSource_.SetState("EdgeState", state_.ToString());
			UpdatePosition();
		}
	}

	public void UpdatePosition()
	{
		line_.SetPosition(0, Command1.transform.localPosition);
		line_.SetPosition(1, Command2.transform.localPosition);
	}

	public PlayerCommand GetOtherSide( PlayerCommand oneSide )
	{
		if( Command1 == oneSide ) return Command2;
		else if( Command2 == oneSide ) return Command1;
		else return null;
	}
}
