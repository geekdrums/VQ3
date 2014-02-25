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
    TextMesh[] displayTexts;

	List<GUIMessage> Messages = new List<GUIMessage>();

	// Use this for initialization
	void Start () {
		instance = this;
        displayTexts = GetComponentsInChildren<TextMesh>();
        for( int i = 0; i < displayTexts.Length; i++ )
        {
            displayTexts[i].text = "";
        }
	}
	
	// Update is called once per frame
	void Update () {
        for( int i = 0; i < Messages.Count; i++ )
        {
            ++Messages[i].CurrentIndex;
            displayTexts[i].text = Messages[i].Text.Substring( 0, Mathf.Min( Messages[i].CurrentIndex + 1, Messages[i].Text.Length ) );
        }
	}

    public static void ClearMessages()
    {
        instance.ClearMessages_();
    }
	public static void AddMessage( params string[] NewMessages )
	{
		foreach ( string message in NewMessages )
		{
			instance.AddMessage_( new GUIMessage(message) );
		}
	}
	public static void AddMessage( GUIMessage NewMessage )
	{
		instance.AddMessage_( NewMessage );
	}
    public static void ChangeMessage( params string[] NewMessages )
    {
        ClearMessages();
        AddMessage( NewMessages );
    }

	void AddMessage_( GUIMessage NewMessage )
	{
        Messages.Insert( 0, NewMessage );
        if( Messages.Count > displayTexts.Length ) Messages.RemoveAt( Messages.Count - 1 );
	}
    void ClearMessages_()
    {
        Messages.Clear();
        for( int i = 0; i < displayTexts.Length; i++ )
        {
            displayTexts[i].text = "";
        }
    }
}
