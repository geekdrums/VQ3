using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum MessageCategory
{
	Help,
    Damage,
    Heal,
    Enhance,
    EnemyEmerge,
    EnemyCommand,
    PlayerCommand,
    PlayerWait,
    CommandSelect,
    Invert,
    Result,
    AcquireCommand
}

public class GUIMessage
{
	public string Text;
	public int CurrentIndex;
    public MessageCategory Type;

	public bool IsEnd { get { return CurrentIndex >= Text.Length - 1; } }
    public string DisplayText
    {
        get
        {
            int textIndex = -1;
            int tagStartIndex = -1;
            bool isTagClosed = true;
            string tagText = "";
            for( int i = 0; i < CurrentIndex; i++ )
            {
                if( textIndex + 1 >= Text.Length ) break;
                ++textIndex;
                if( Text[textIndex] == '<' )
                {
                    isTagClosed = !isTagClosed;
                    tagStartIndex = textIndex + 1;
                    while( Text[textIndex] != '>' )
                    {
                        if( !isTagClosed && Text[textIndex] == '=' )
                        {
                            tagText = Text.Substring( tagStartIndex, textIndex - tagStartIndex );
                        }
                        ++textIndex;
                    }
                }
            }
            string res = Text.Substring( 0, Mathf.Min( textIndex + 1, Text.Length ) );
            if( !isTagClosed )
            {
                res += "</" + tagText + ">";
            }
            //TODO: replace [Icon:name] to spaces
            return res;
        }
    }

	public GUIMessage( string Text, MessageCategory type, int startIndex = 0 )
	{
		this.Text = Text;
        this.CurrentIndex = startIndex;
        this.Type = type;
	}
}

public class TextWindow : MonoBehaviour {

	static TextWindow instance;
    TextMesh displayText;

	GUIMessage message = new GUIMessage( "", MessageCategory.Damage );
    bool useNextCursor = false;
    float blinkTime;
    float displayMusicTime;

    public GameObject textBase;
    public float BlinkInterval;

	// Use this for initialization
	void Start () {
		instance = this;
        displayText = GetComponentInChildren<TextMesh>();
        displayText.text = "";
	}
	
	// Update is called once per frame
	void Update () {
        ++message.CurrentIndex;
        displayText.text = message.DisplayText;
        if( Music.IsJustChanged )
        {
            ++displayMusicTime;
        }
        if( useNextCursor )
        {
            blinkTime += Time.deltaTime;
            if( message.IsEnd && (blinkTime % (BlinkInterval * 2)) >= BlinkInterval )
            {
                displayText.text += " >";
            }
        }
	}

    void ChangeMessage_( string text, MessageCategory type )
    {
        this.message.Text = text;
        this.message.Type = type;
        this.message.CurrentIndex = 0;
        displayMusicTime = 0;
    }
    void SetNextCursor_( bool use )
    {
        useNextCursor = use;
        blinkTime = 0;
    }

    public static void ChangeMessage( MessageCategory type, string NewMessage )
    {
        instance.ChangeMessage_( NewMessage, type );
    }
    public static void SetNextCursor( bool use )
    {
        instance.SetNextCursor_( use );
    }
}
