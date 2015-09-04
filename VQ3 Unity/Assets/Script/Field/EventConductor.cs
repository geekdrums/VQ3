using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum EventState
{
	None,
	Messaging,
	Macro
}

public class EventConductor : MonoBehaviour
{
	public EventState State { get; private set; }

	public float CharacterReadTime;
	public float BlinkInterval;

	private EventData currentEvent_;
	private int messageIndex_;
	private float readTime_;
	private MessageIndexed displayingMessage_ = new MessageIndexed("");

	private EventData.Message CurrentMessage { get { return currentEvent_.Messages[messageIndex_]; } }

	void Awake()
	{
		GameContext.EventConductor = this;
		State = EventState.None;
	}

	void Update()
	{
		if( GameContext.State == GameState.Event )
		{
			switch( State )
			{
			case EventState.Messaging:
				//read
				if( displayingMessage_.IsEnd )
				{
					readTime_ += Time.deltaTime;
					if( (readTime_ % (BlinkInterval * 2)) >= BlinkInterval )
					{
						//displayText.text += " >";
					}
				}
				else
				{
					readTime_ -= Time.deltaTime;
					if( readTime_ <= 0 )
					{
						displayingMessage_.CurrentIndex++;
						readTime_ = CharacterReadTime;
						string str = displayingMessage_.DisplayText;
						char ch = str[str.Length-1];
						if( ch == ',' || ch == '、' ) readTime_ += 0.1f;
						else if( ch == '.' || ch == '?' || ch == '!' || ch == '。' || ch == '？' || ch == '！' ) readTime_ += 0.2f;
						else if( ch == '…' ) readTime_ += 0.3f;
						else if( ch == '\n' ) readTime_ += 0.4f;
					}
				}

				//click
				if( Input.GetMouseButtonUp(0) )
				{
					if( displayingMessage_.IsEnd )
					{
						if( CurrentMessage.Macro != "" )
						{
							State = EventState.Macro;
							StartCoroutine(CurrentMessage.Macro);
						}
						else
						{
							NextMessage();
						}
					}
					else
					{
						displayingMessage_.End();
					}
				}
				break;
			case EventState.Macro:
				break;
			}
		}
	}

	public void OnEnterEvent(EventData data)
	{
		currentEvent_ = data;
		messageIndex_ = -1;
		NextMessage();
	}

	void NextMessage()
	{
		++messageIndex_;
		readTime_ = 0;
		if( messageIndex_ >= currentEvent_.Messages.Length )
		{
			State = EventState.None;
			GameContext.SetState(currentEvent_.NextState);
		}
		else
		{
			State = EventState.Messaging;
			displayingMessage_.Init(CurrentMessage.Text);
		}
	}

	void EndMacro()
	{
		StopCoroutine(CurrentMessage.Macro);
		NextMessage();
	}


	#region event macro

	IEnumerator Event_ShowCommandSet()
	{
		EndMacro();
		yield return null;
	}

	IEnumerator Event_ShowEnemyInfo()
	{
		EndMacro();
		yield return null;
	}
	#endregion
}
