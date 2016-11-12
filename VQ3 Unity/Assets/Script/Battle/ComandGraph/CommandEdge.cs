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
	static Color NotSelectedLineColor = ColorManager.MakeAlpha(Color.white, 0.9f);
	static Color InvertLinkedLineColor = Color.black;
	static Color InvertUnlinkedLineColor = ColorManager.MakeAlpha(Color.black, 0.2f);
	static Color InvertPrelinkedLineColor = ColorManager.MakeAlpha(Color.black, 0.2f);
	static float LinkedLineWidth = 0.3f;
	static float PrelinkedLineWidth = 0.1f;
	static float UnlinkedLineWidth = 0.05f;
	static float NotSelectedLineWidth = 0.15f;

	LineRenderer line_;
	EdgeState state_ = EdgeState.None;
	public bool IsInvert { get { return Command1 is InvertCommand || Command2 is InvertCommand; } }
	public bool IsUsable { get { return Command1.IsUsable() && Command2.IsUsable(); } }
	public PlayerCommand Command1;
	public PlayerCommand Command2;

	// Use this for initialization
	void Awake()
	{
		line_ = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		float width = 0;
		Color color = Color.white;
		switch( state_ )
		{
		case EdgeState.Linked:
			width = LinkedLineWidth;
			color = LinkedLineColor;
			if( IsInvert )
			{
				color = InvertLinkedLineColor;
			}
			else
			{
				if( Command1.state == CommandState.Selected || Command2.state == CommandState.Selected )
				{
					width = LinkedLineWidth;
				}
				else if( Command1.state == CommandState.NotSelected || Command2.state == CommandState.NotSelected )
				{
					color = NotSelectedLineColor;
					width = NotSelectedLineWidth;
				}
			}
			break;
		case EdgeState.PreLinked:
			width = PrelinkedLineWidth;
			if( IsInvert )
			{
				color = InvertPrelinkedLineColor;
			}
			else
			{
				color = PrelinkedLineColor;
			}
			break;
		case EdgeState.Unlinked:
			width = UnlinkedLineWidth;
			if( IsInvert )
			{
				color = InvertUnlinkedLineColor;
			}
			else
			{
				color = UnlinkedLineColor;
			}
			break;
		case EdgeState.Unacquired:
			width = UnlinkedLineWidth;
			color = UnlinkedLineColor;
			break;
		case EdgeState.DontKnow:
			break;
		}
		line_.SetWidth(width * Command1.transform.localScale.x * 4, width * Command2.transform.localScale.x * 4);
		line_.SetColors(color, color);
		line_.SetPosition(0, Command1.transform.localPosition);
		line_.SetPosition(1, Command2.transform.localPosition);
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
		set { state_ = value; }
	}

	public PlayerCommand GetOtherSide( PlayerCommand oneSide )
	{
		if( Command1 == oneSide ) return Command2;
		else if( Command2 == oneSide ) return Command1;
		else return null;
	}
}
