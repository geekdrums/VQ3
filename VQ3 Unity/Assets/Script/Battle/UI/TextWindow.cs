using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum BattleMessageType
{
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
    Tutorial,
    AcquireCommand
}

public class GUIMessage
{
	public string Text;
	public int CurrentIndex;
    public BattleMessageType Type;

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

	public GUIMessage( string Text, BattleMessageType type, int startIndex = 0 )
	{
		this.Text = Text;
        this.CurrentIndex = startIndex;
        this.Type = type;
	}
}

public enum TutorialMessageType
{
    None,
    Const,
    Loop,
}

[System.Serializable]
public class TutorialMessage
{
    public string[] Texts;
    public Color BaseColor;
    public List<BattleMessageType> ApproveMessageTypes;
    public TutorialMessageType Type;

    public string DisplayText
    {
        get
        {
            switch( Type )
            {
            case TutorialMessageType.Const:
                return Texts[0];
            default:
                return Texts[0];
            }
        }
    }
}

public class TextWindow : MonoBehaviour {

	static TextWindow instance;
    TextMesh displayText;

	GUIMessage message = new GUIMessage( "", BattleMessageType.Damage );
    TutorialMessage tutorialMessage;
    bool useNextCursor = false;
    float blinkTime;
    float displayMusicTime;
    //Vector3 initialTutorialBasePosition;

    public GameObject textBase;
    //public GameObject textBaseTutorial;
    public float BlinkInterval;

	// Use this for initialization
	void Start () {
		instance = this;
        displayText = GetComponentInChildren<TextMesh>();
        displayText.text = "";
        //initialTutorialBasePosition = textBaseTutorial.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        ++message.CurrentIndex;
        displayText.text = message.DisplayText;
        if( Music.isJustChanged )
        {
            ++displayMusicTime;
            if( tutorialMessage != null && message.Type != BattleMessageType.Tutorial && displayMusicTime >= 16 )
            {
                ChangeMessage_( tutorialMessage.DisplayText, BattleMessageType.Tutorial );
            }
        }
        if( useNextCursor )
        {
            blinkTime += Time.deltaTime;
            if( message.IsEnd && (blinkTime % (BlinkInterval * 2)) >= BlinkInterval )
            {
                displayText.text += " >";
            }
        }
        //textBaseTutorial.transform.position = Vector3.Lerp( textBaseTutorial.transform.position, (message.Type == BattleMessageType.Tutorial ? textBase.transform.position : initialTutorialBasePosition), 0.2f );
	}

    void ChangeMessage_( string text, BattleMessageType type )
    {
        if( tutorialMessage != null )
        {
            if( type != BattleMessageType.Tutorial && !tutorialMessage.ApproveMessageTypes.Contains( type ) ) return;
        }
        this.message.Text = text;
        this.message.Type = type;
        this.message.CurrentIndex = 0;
        displayMusicTime = 0;
        //displayText.color = Color.white;//(type == BattleMessageType.Tutorial ? Color.white : (tutorialMessage != null ? tutorialMessage.BaseColor : Color.black));
    }
    void SetNextCursor_( bool use )
    {
        useNextCursor = use;
        blinkTime = 0;
    }
    void SetTutorialMessage_( TutorialMessage tm )
    {
        tutorialMessage = tm;
        //textBaseTutorial.renderer.material.color = tm.BaseColor;
        ChangeMessage_( tm.DisplayText, BattleMessageType.Tutorial );
    }
    void ClearTutorialMessage_()
    {
        tutorialMessage = null;
    }

    public static void ChangeMessage( BattleMessageType type, string NewMessage )
    {
        instance.ChangeMessage_( NewMessage, type );
    }
    public static void SetNextCursor( bool use )
    {
        instance.SetNextCursor_( use );
    }
    public static void SetTutorialMessage( TutorialMessage tm )
    {
        instance.SetTutorialMessage_( tm );
    }
    public static void ClearTutorialMessage()
    {
        instance.ClearTutorialMessage_();
    }
}
