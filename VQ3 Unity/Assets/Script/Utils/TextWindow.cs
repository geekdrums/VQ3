using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GUIMessage
{
	public readonly string Text;
	public int CurrentIndex;
	Func<bool> isEndPred;
	Action OnEndShowEvent;

	public bool IsEnd { get { return CurrentIndex >= Text.Length - 1 && isEndPred(); } }
	public void OnEndShow()
	{
		if ( OnEndShowEvent != null )
		{
			OnEndShowEvent();
		}
	}

	public GUIMessage( string Text, Func<bool> isEnd = null, Action OnEndShow = null )
	{
		this.Text = Text;
		if ( isEnd != null )
		{
			this.isEndPred = isEnd;
		}
		else
		{
			this.isEndPred = EndNextBar;
		}
		this.OnEndShowEvent = OnEndShow;
	}

	public readonly static Func<bool> EndNextBar = () => Music.IsNowChangedBar();
	public readonly static Func<bool> EndByTouch = () => Input.GetMouseButtonDown( 0 );//Input.GetTouch(0).tapCount > 0;
	public readonly static Action<string> Nextmessage = ( string message ) => TextWindow.AddMessage( new GUIMessage( message ) );
}

public class TextWindow : MonoBehaviour {

	static TextWindow instance;
	GUIText displayText;
	bool isOpened;

	GUIMessage CurrentMessage;
	Queue<GUIMessage> NextMessages = new Queue<GUIMessage>();

	// Use this for initialization
	void Start () {
		instance = this;

		displayText = GetComponentInChildren<GUIText>();
		displayText.material.color = Color.black;
		displayText.enabled = false;
		guiTexture.pixelInset = new Rect( 0, 0, Screen.width, guiTexture.pixelInset.height );
		guiTexture.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if ( ( isOpened && !displayText.enabled ) && !animation.isPlaying )
		{
			OnEndOpened();
		}
		else if ( ( !isOpened && guiTexture.enabled ) && !animation.isPlaying )
		{
			OnEndClosed();
		}

		if ( isOpened )
		{
			if ( displayText.enabled && ( Music.isNowChanged || Music.isJustChanged ) )
			{
				//TODO: Sound
				++CurrentMessage.CurrentIndex;
			}
			if ( CurrentMessage.IsEnd )
			{
				Close();
			}
			displayText.text = CurrentMessage.Text.Substring( 0, Mathf.Min( CurrentMessage.CurrentIndex+1, CurrentMessage.Text.Length ) );
		}

	}

	public static void AddMessage( params string[] NewMessages )
	{
        /*
		foreach ( string message in NewMessages )
		{
			instance.AddMessage_( new GUIMessage(message) );
		}
        */
	}
	public static void AddMessage( GUIMessage NewMessage )
	{
		//instance.AddMessage_( NewMessage );
	}

	void AddMessage_( GUIMessage NewMessage )
	{
		if ( !isOpened )
		{
			CurrentMessage = NewMessage;
			Open();
		}
		else
		{
			NextMessages.Enqueue( NewMessage );
		}
	}

	void Open()
	{
		animation.Play("TextOpenAnim");
		isOpened = true;
		guiTexture.enabled = true;
	}
	void Close()
	{
		CurrentMessage.OnEndShow();
		if ( NextMessages.Count == 0 )
		{
			animation.Play( "TextCloseAnim" );
			isOpened = false;
			displayText.enabled = false;
		}
		else
		{
			CurrentMessage = NextMessages.Dequeue();
		}
	}
	void OnEndOpened()
	{
		displayText.enabled = true;
		guiTexture.enabled = true;
	}
	void OnEndClosed()
	{
		displayText.enabled = false;
		guiTexture.enabled = false;
	}
}
