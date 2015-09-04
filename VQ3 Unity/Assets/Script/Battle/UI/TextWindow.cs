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

	public MessageIndexed(string text, int startIndex = 0)
	{
		this.Text = text;
		this.CurrentIndex = startIndex;
	}

	public void Init(string text)
	{
		this.Text = text;
		this.CurrentIndex = 0;
	}
	public void End()
	{
		this.CurrentIndex = Text.Length - 1;
	}
}

public class TextWindow : MonoBehaviour
{
	public GameObject TextBase;
	public GameObject CommandBase, IconParent;
	public float BlinkInterval;

	static TextWindow instance_;

	TextMesh displayText_, commandText_;
	GaugeRenderer line_;
	ButtonUI cancelButton_;
	MessageIndexed message_ = new MessageIndexed("");
	bool useNextCursor_ = false;
	float blinkTime_;
	float displayMusicTime_;
	PlayerCommand displayCommnd_;

	// Use this for initialization
	void Start()
	{
		instance_ = this;

		TextBase.SetActive(true);
		CommandBase.SetActive(true);
		line_ = GetComponentInChildren<GaugeRenderer>();
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
		++message_.CurrentIndex;
		displayText_.text = message_.DisplayText;
		if( Music.IsJustChanged )
		{
			++displayMusicTime_;
		}
		if( useNextCursor_ )
		{
			blinkTime_ += Time.deltaTime;
			if( message_.IsEnd && (blinkTime_ % (BlinkInterval * 2)) >= BlinkInterval )
			{
				displayText_.text += " >";
			}
		}
	}

	void SetMessage_(string text)
	{
		TextBase.SetActive(true);
		CommandBase.SetActive(false);

		this.message_.Text = text;
		this.message_.CurrentIndex = 0;
		displayMusicTime_ = 0;
		
		line_.SetColor(Color.white);
	}

	void SetNextCursor_(bool use)
	{
		useNextCursor_ = use;
		blinkTime_ = 0;
	}

	void SetCommand_(PlayerCommand command)
	{
		TextBase.SetActive(false);
		CommandBase.SetActive(true);

		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		GameObject iconObj = Instantiate(command.gameObject) as GameObject;
		Destroy(iconObj.transform.FindChild("nextRect").gameObject);
		if( iconObj.transform.FindChild("currentRect") != null )
		{
			Destroy(iconObj.transform.FindChild("currentRect").gameObject);
		}
		iconObj.transform.parent = IconParent.transform;
		iconObj.transform.localPosition = Vector3.zero;
		iconObj.transform.localScale = Vector3.one;
		iconObj.transform.localRotation = Quaternion.identity;
		iconObj.GetComponent<PlayerCommand>().enabled = false;

		Color commandColor = ColorManager.GetThemeColor(command.themeColor).Bright;
		if( command.currentLevel == 0 ) commandColor = ColorManager.GetThemeColor(command.themeColor).Shade;
		commandText_.text = command.nameText.ToUpper();
		commandText_.color = commandColor;
		line_.SetColor(commandColor);
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
}
