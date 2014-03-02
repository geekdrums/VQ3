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

	public bool IsEnd { get { return CurrentIndex >= Text.Length - 1 && ( isEndPred == null || isEndPred() ); } }
	public void OnEndShow()
	{
		if ( OnEndShowEvent != null )
		{
			OnEndShowEvent();
		}
	}

	public GUIMessage( string Text, Func<bool> isEnd = null, Action OnEndShow = null, int startIndex = 0 )
	{
		this.Text = Text;
        this.CurrentIndex = startIndex;
		if ( isEnd != null )
		{
			this.isEndPred = isEnd;
		}
		this.OnEndShowEvent = OnEndShow;
	}
}

public class TextWindow : MonoBehaviour {

	static TextWindow instance;
    TextMesh[] displayTexts;

	List<GUIMessage> Messages = new List<GUIMessage>();
    bool useNextCursor = false;
    float blinkTime;

    public float BlinkInterval;

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
        if( useNextCursor )
        {
            blinkTime += Time.deltaTime;
            if( Messages[Messages.Count - 1].IsEnd && (blinkTime % (BlinkInterval*2) ) >= BlinkInterval )
            {
                displayTexts[Messages.Count - 1].text += " >";
            }
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
    public static void SetNextCursor( bool use )
    {
        instance.SetNextCursor_( use );
    }

	void AddMessage_( GUIMessage NewMessage )
	{
        Messages.Add( NewMessage );
        if( Messages.Count > displayTexts.Length ) Messages.RemoveAt( 0 );
	}
    void ClearMessages_()
    {
        Messages.Clear();
        for( int i = 0; i < displayTexts.Length; i++ )
        {
            displayTexts[i].text = "";
        }
    }

    void SetNextCursor_( bool use )
    {
        useNextCursor = use;
        blinkTime = 0;
    }
}
