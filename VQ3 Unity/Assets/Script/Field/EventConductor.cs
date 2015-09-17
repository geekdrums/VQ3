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

	private EventData currentEvent_;
	private int messageIndex_;
	private float readTime_;
	private MessageIndexed message_ = new MessageIndexed("");
	private Animation animation_;

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
					StartCoroutine(CurrentMessage.Macro);
				}
				else
				{
					NextMessage();
				}
			}
			else
			{
				message_.End();
			}
		}
	}

	public void OnEnterEvent(EventData data)
	{
		currentEvent_ = data;
		messageIndex_ = -1;
		MessageParent.SetActive(true);
		animation_ = GetComponentInChildren<Animation>();
		NextMessage();
		OKButton.SetMode(ButtonMode.Active, true);
		animation_.Play("EventShowAnim");
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
			VLine.SetRate(0);
			VLine.SetRate(1, 0.3f);
			Cursor.transform.localPosition = new Vector3(Cursor.transform.localPosition.x, -2 - 1.5f * numLine, 0);
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
