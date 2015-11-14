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

public class MessageIndexed
{
	public string Text;
	public int CurrentIndex;

	int endIndex_;

	public bool IsEnd { get { return CurrentIndex >= endIndex_; } }
    public string DisplayText
    {
        get
        {
            int textIndex = -1;
            int tagStartIndex = -1;
            bool isTagClosed = true;
            string tagText = "";
            for( int i = 0; i <= CurrentIndex; i++ )
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
					++textIndex;
                }
            }
            string res = Text.Substring( 0, Mathf.Min( textIndex + 1, Text.Length ) );
            if( !isTagClosed )
            {
                res += "</" + tagText + ">";
            }
            return res;
        }
    }

	public MessageIndexed(string text, int startIndex = 0)
	{
		Init(text, startIndex);
	}

	public void Init(string text, int index = 0)
	{
		this.Text = text;
		this.CurrentIndex = index;
		endIndex_ = 0;
		string[] texts = Text.Split(new char[]{'<', '>'}, StringSplitOptions.RemoveEmptyEntries);
		for( int i=0; i<texts.Length; ++i )
		{
			if( (Text[0] == '<' && i%2 == 1) || (Text[0] != '<' && i%2 == 0) )
			{
				endIndex_ += texts[i].Length;
			}
		}
	}
	public void Reset()
	{
		Text = "";
		CurrentIndex = 0;
		endIndex_ = 0;
	}
	public void End()
	{
		this.CurrentIndex = endIndex_;
	}
}

public class TextWindow : MonoBehaviour
{
	public GameObject TextBase;
	public GameObject CommandBase, IconParent;
	public float BlinkInterval;

	static TextWindow instance_;

	TextMesh displayText_, commandText_;
	//GaugeRenderer line_;
	ButtonUI cancelButton_;
	MessageIndexed message_ = new MessageIndexed("");
	MessageIndexed commandName_= new MessageIndexed("");
	bool useNextCursor_ = false;
	float blinkTime_;
	PlayerCommand displayCommnd_;
	GameObject commandIconObj_;

	// Use this for initialization
	void Start()
	{
		instance_ = this;

		TextBase.SetActive(true);
		CommandBase.SetActive(true);
		//line_ = GetComponentInChildren<GaugeRenderer>();
		displayText_ = TextBase.GetComponentInChildren<TextMesh>();
		displayText_.text = "";
		commandText_ = CommandBase.GetComponentInChildren<TextMesh>();
		cancelButton_ = CommandBase.GetComponentInChildren<ButtonUI>();
		cancelButton_.OnPushed += this.OnPushedCancel;
		TextBase.SetActive(false);
		CommandBase.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		if( message_.IsEnd == false )
		{
			message_.CurrentIndex++;
			displayText_.text = message_.DisplayText;
		}
		else if( useNextCursor_ )
		{
			blinkTime_ += Time.deltaTime;
			bool blink = (blinkTime_ % (BlinkInterval * 2)) >= BlinkInterval;
			displayText_.text = message_.DisplayText + ( blink ? " >" : "" );
		}

		if( displayCommnd_ != null )
		{
			if( commandIconObj_.transform.localPosition.magnitude > 0.5f )
			{
				commandIconObj_.transform.localPosition *= 0.7f;
			}
			else if( commandName_.IsEnd == false )
			{
				commandIconObj_.transform.localPosition = Vector3.zero;
				commandName_.CurrentIndex++;
				commandText_.text = commandName_.DisplayText;
			}
		}
	}

	void SetMessage_(string text)
	{
		TextBase.SetActive(true);
		//CommandBase.SetActive(false);

		displayCommnd_ = null;

		if( message_.Text == text ) return;
		message_.Init(text);
		
		//line_.SetColor(Color.white);
	}

	void Reset_()
	{
		TextBase.SetActive(false);
		CommandBase.SetActive(false);
		message_.Reset();
		SetNextCursor_(false);
	}

	void SetNextCursor_(bool use)
	{
		useNextCursor_ = use;
		blinkTime_ = 0;
	}

	void SetCommand_(PlayerCommand command)
	{
		//TextBase.SetActive(false);
		CommandBase.SetActive(command != null);

		displayCommnd_ = command;

		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		if( command == null ) return;

		commandIconObj_ = command.InstantiateIconObj(IconParent);
		commandIconObj_.transform.position = command.transform.position;

		Color commandColor = ColorManager.GetThemeColor(command.themeColor).Bright;
		if( command.currentLevel == 0 ) commandColor = ColorManager.GetThemeColor(command.themeColor).Shade;
		commandName_.Init(command.nameText.ToUpper());
		commandText_.text = "";
		commandText_.color = commandColor;
		//line_.SetColor(commandColor);
	}

	void OnPushedCancel(object sender, EventArgs e)
	{
		GameContext.PlayerConductor.OnDeselectedCommand();
	}

	public static void SetMessage(MessageCategory type, string NewMessage)
	{
		instance_.SetMessage_(NewMessage);
	}
	public static void SetNextCursor(bool use)
	{
		instance_.SetNextCursor_(use);
	}
	public static void SetCommand(PlayerCommand command)
	{
		instance_.SetCommand_(command);
	}
	public static void Reset()
	{
		instance_.Reset_();
	}
}
