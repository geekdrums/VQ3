using UnityEngine;
using System;
using System.Collections;

public enum EdgeState
{
	Linked,
	PreLinked,
	Unlinked,
	Unacquired,
	DontKnow,
	None,
}

public class CommandEdge : MonoBehaviour {

	static Color LinkedLineColor = Color.white;
	static Color UnlinkedLineColor = ColorManager.MakeAlpha(Color.white, 0.2f);
	static Color PrelinkedLineColor = ColorManager.MakeAlpha(Color.white, 0.2f);
	static Color InvertLinkedLineColor = Color.black;
	static Color InvertUnlinkedLineColor = ColorManager.MakeAlpha(Color.black, 0.2f);
	static Color InvertPrelinkedLineColor = ColorManager.MakeAlpha(Color.black, 0.2f);
	static float LinkedLineWidth = 0.3f;
	static float PrelinkedLineWidth = 0.1f;
	static float UnlinkedLineWidth = 0.05f;
	
	LineRenderer line_;
	EdgeState state_ = EdgeState.None;
	public bool IsInvert { get { return Command1 is InvertCommand || Command2 is InvertCommand; } }
	public bool IsUsable { get { return Command1.state <= CommandState.Acquired && Command2.state <= CommandState.Acquired; } }
	public PlayerCommand Command1;
	public PlayerCommand Command2;

	// Use this for initialization
	void Start () {
		line_ = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
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
			{
				state_ = value;
				switch( state_ )
				{
				case EdgeState.Linked:
					if( IsInvert )
					{
						line_.SetColors(InvertLinkedLineColor, InvertLinkedLineColor);
					}
					else
					{
						line_.SetColors(LinkedLineColor, LinkedLineColor);
					}
					line_.SetWidth(LinkedLineWidth, LinkedLineWidth);
					break;
				case EdgeState.PreLinked:
					if( IsInvert )
					{
						line_.SetColors(InvertPrelinkedLineColor, InvertPrelinkedLineColor);
					}
					else
					{
						line_.SetColors(PrelinkedLineColor, PrelinkedLineColor);
					}
					line_.SetWidth(PrelinkedLineWidth, PrelinkedLineWidth);
					break;
				case EdgeState.Unlinked:
					if( IsInvert )
					{
						line_.SetColors(InvertUnlinkedLineColor, InvertUnlinkedLineColor);
					}
					else
					{
						line_.SetColors(UnlinkedLineColor, UnlinkedLineColor);
					}
					line_.SetWidth(UnlinkedLineWidth, UnlinkedLineWidth);
					break;
				case EdgeState.Unacquired:
					line_.SetColors(UnlinkedLineColor, UnlinkedLineColor);
					line_.SetWidth(UnlinkedLineWidth, UnlinkedLineWidth);
					break;
				case EdgeState.DontKnow:
					line_.SetWidth(0, 0);
					break;
				}
			}
		}
	}

	public PlayerCommand GetOtherSide( PlayerCommand oneSide )
	{
		if( Command1 == oneSide ) return Command2;
		else if( Command2 == oneSide ) return Command1;
		else return null;
	}
}
