using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum EventState
{
	None,
	Messaging,
	Macro,
	Hiding
}

public class EventConductor : MonoBehaviour
{
	public EventState State { get; private set; }

	public float CharacterReadTime;
	public float BlinkInterval;
	public TextMesh DisplayText;
	public TextMesh SenderText;
	public MidairPrimitive Cursor;
	public GaugeRenderer VLine;
	public GameObject MessageParent;
	public ButtonUI OKButton;

	public CommandExplanation CommandExp;
	public GameObject Title;


	private EventData currentEvent_;
	private int messageIndex_;
	private float readTime_;
	private MessageIndexed message_ = new MessageIndexed("");
	private Animation animation_;

	private float coroutineTime_ = 0;
	private int coroutinePhase_ = 0;
	private string[] eventArgs_;
	private event System.Action OnPushedOK_Coroutine;


	private EventData.Message CurrentMessage { get { return currentEvent_.Messages[messageIndex_]; } }

	void Awake()
	{
		GameContext.EventConductor = this;
		OKButton.OnPushed += this.OnPushedOK;
	}

	void Update()
	{
		if( GameContext.State == GameState.Event )
		{
			switch( State )
			{
			case EventState.Messaging:
				//read
				if( message_.IsEnd )
				{
					readTime_ += Time.deltaTime;
					Cursor.GetComponent<Renderer>().enabled = (readTime_ % (BlinkInterval * 2)) >= BlinkInterval;
				}
				else
				{
					readTime_ -= Time.deltaTime;
					if( readTime_ <= 0 )
					{
						message_.CurrentIndex++;
						if( message_.IsEnd )
						{
							SEPlayer.Play("cursorSound");
						}
						else
						{
							SEPlayer.Play("charRead");
						}
						readTime_ = CharacterReadTime;
						string str = message_.DisplayText;
						char ch = str[str.Length-1];
						if( ch == ',' || ch == '、' ) readTime_ += 0.1f;
						else if( ch == '.' || ch == '?' || ch == '!' || ch == '。' || ch == '？' || ch == '！' ) readTime_ += 0.2f;
						else if( ch == '…' ) readTime_ += 0.3f;
						DisplayText.text = message_.DisplayText;
					}
				}
				break;
			case EventState.Macro:
				break;
			case EventState.Hiding:
				if( animation_.isPlaying == false )
				{
					State = EventState.None;
					MessageParent.SetActive(false);
					GameContext.SetState(currentEvent_.NextState);
				}
				break;
			}
		}
	}

	void OnPushedOK(object sender, System.EventArgs e)
	{
		if( State == EventState.Messaging )
		{
			if( message_.IsEnd )
			{
				if( CurrentMessage.Macro != "" )
				{
					State = EventState.Macro;
					coroutineTime_ = 0;
					OnPushedOK_Coroutine = null;
					eventArgs_ = CurrentMessage.Macro.Split(' ');
					StartCoroutine(eventArgs_[0]);
				}
				else
				{
					NextMessage();
				}
			}
			else
			{
				message_.End();
				DisplayText.text = message_.DisplayText;
			}
		}
		else if( State == EventState.Macro )
		{
			if( OnPushedOK_Coroutine != null )
				OnPushedOK_Coroutine();
		}
	}

	public void OnEnterEvent(EventData data)
	{
		currentEvent_ = data;
		messageIndex_ = -1;
		MessageParent.SetActive(true);
		animation_ = GetComponentInChildren<Animation>();
		animation_.Play("EventShowAnim");
		NextMessage();
		OKButton.SetMode(ButtonMode.Active, true);
		TextWindow.Reset();
		if( Music.IsPlaying == false || Music.CurrentMusicName != data.MusicName )
		{
			Music.Play(data.MusicName);
		}
	}

	void NextMessage()
	{
		++messageIndex_;
		readTime_ = 0;
		if( messageIndex_ >= currentEvent_.Messages.Length )
		{
			DisplayText.text = "";
			SenderText.text = "";
			Cursor.GetComponent<Renderer>().enabled = false;
			State = EventState.Hiding;
			currentEvent_.Watched = true;
			animation_.Play("EventHideAnim");
		}
		else
		{
			State = EventState.Messaging;
			int numLine = CurrentMessage.Text.Split(new string[] { "<br/>" }, System.StringSplitOptions.None).Length;
			message_.Init(CurrentMessage.Text.Replace("<br/>",System.Environment.NewLine));
			SenderText.text = "from: " + CurrentMessage.Sender;
			Cursor.GetComponent<Renderer>().enabled = false;
			VLine.Length = 2 + 1.5f * numLine;
			if( animation_ == null || animation_.IsPlaying("EventShowAnim") == false )
			{
				VLine.SetRate(0);
				VLine.AnimateRate(1, time: 0.3f);
			}
			Cursor.transform.localPosition = new Vector3(Cursor.transform.localPosition.x, -2 - 1.5f * numLine, 0);
		}
	}

	void EndMacro()
	{
		StopCoroutine(CurrentMessage.Macro);
		NextMessage();
	}


	#region event macro

	IEnumerator Event_Example()
	{
		EndMacro();
		while( true )
		{
			yield return null;
		}
	}

	IEnumerator Event_ShowBPMeter()
	{
		//GameContext.PlayerConductor.VPMeter.SetActive(true);
		//GameContext.PlayerConductor.MemoryPanel.Hide();
		GameContext.LuxSystem.Event_ShowBPMeter(init: true);
		while( true )
		{
			GameContext.LuxSystem.Event_ShowBPMeter();
			if( coroutinePhase_ == 1 )
			{
				coroutineTime_ += Time.deltaTime;
				if( coroutineTime_ > 0.75f )
				{
					EndMacro();
				}
			}
			else if( GameContext.LuxState == LuxState.Overflow )
			{
				if( OnPushedOK_Coroutine == null )
				{
					OnPushedOK_Coroutine += OnPushedOK_Event_ShowBPMeter;
				}
			}
			else
			{
				if( Music.IsJustChangedBeat() )
				{
					GameContext.LuxSystem.AddVP(20, 30);
				}
			}
			yield return null;
		}
	}

	void OnPushedOK_Event_ShowBPMeter()
	{
		coroutinePhase_ = 1;
		GameContext.LuxSystem.ResetBreak();
	}


	IEnumerator Event_ShowCommand()
	{
		CommandExp.Set(GameContext.PlayerConductor.CommandGraph.CommandNodes.Find((PlayerCommand command) => command.name == eventArgs_[1]));
		OnPushedOK_Coroutine += this.OnPushedOK_Event_ShowCommand;
		while( true )
		{
			yield return null;
		}
	}

	void OnPushedOK_Event_ShowCommand()
	{
		CommandExp.Reset();
		EndMacro();
	}

	IEnumerator Event_ShowTitle()
	{
		Title.SetActive(true);
		Title.GetComponent<Animation>().Play("titleEndAnim");
		while( true )
		{
			yield return null;
		}
	}


	#endregion
}
