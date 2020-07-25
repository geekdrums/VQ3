using UnityEngine;
using System.Collections;

public class CommandInfoUI : MonoBehaviour
{
	public ColorSourceBase BrightSource, LightSource, ShadeSource, TextColor;
	public TextMesh CommandName;
	public ColorSourceBase Roll;
	public ColorSourceBase Roll1;
	public ColorSourceBase Roll2;
	public SkillListUI SkillListUI;
	public float PreviewAlpha = 0.5f;
	public bool IsSelectedCommand; // else CurrentCommand
	public AnimComponent Animation;

	bool IsCurrentCommand { get { return !IsSelectedCommand; } }
	PlayerCommandData commandData_;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void SetTheme(string themeState)
	{
		BrightSource.SetState("LocalTheme", themeState);
		LightSource.SetState("LocalTheme", themeState);
		ShadeSource.SetState("LocalTheme", themeState);
		TextColor.SetState("LocalTheme", themeState);
	}

	void SetAlpha(float alpha)
	{
		BrightSource.SetParameter("Alpha", alpha);
		LightSource.SetParameter("Alpha", alpha);
		ShadeSource.SetParameter("Alpha", alpha);
		TextColor.SetParameter("Alpha", alpha);
	}

	static string GetRollText(ResonantRoll roll, string unitqueName = "")
	{
		switch( roll )
		{
			case ResonantRoll.Attacker:
				return "アタッカー";
			case ResonantRoll.Breaker:
				return "ブレイカー";
			case ResonantRoll.Healer:
				return "ヒーラー";
			case ResonantRoll.Defender:
				return "ディフェンダー";
			case ResonantRoll.Enhancer:
				return "エンハンサー";
			case ResonantRoll.Unique:
				return unitqueName;
			case ResonantRoll.None:
			default:
				return "";
		}
	}

	public void Set(PlayerCommand command, bool isPreview = false)
	{
		gameObject.SetActive(true);
		commandData_ = command.currentData;
		CommandName.text = command.name;

		if( commandData_.Roll2 == ResonantRoll.None )
		{
			Roll.gameObject.SetActive(true);
			Roll1.gameObject.SetActive(false);
			Roll2.gameObject.SetActive(false);
			Roll.SetState("Roll", commandData_.Roll1.ToString());
			Roll.GetComponentInChildren<TextMesh>().text = GetRollText(commandData_.Roll1, commandData_.UniqueRollName);
		}
		else
		{
			Roll.gameObject.SetActive(false);
			Roll1.gameObject.SetActive(true);
			Roll2.gameObject.SetActive(true);
			Roll1.SetState("Roll", commandData_.Roll1.ToString());
			Roll2.SetState("Roll", commandData_.Roll2.ToString());
			Roll1.GetComponentInChildren<TextMesh>().text = GetRollText(commandData_.Roll1);
			Roll2.GetComponentInChildren<TextMesh>().text = GetRollText(commandData_.Roll2);
		}

		if( commandData_ != null )
		{
			SetTheme(commandData_.OwnerCommand.themeColor.ToString());
			/*
			Explanation.text = commandData_.ExplanationText.Replace("<br/>", System.Environment.NewLine);
			if( GameContext.FieldConductor.EncounterIndex == 0 )
			{
				Explanation.text = commandData_.ExplanationText.Split(new string[]{"<br/>"}, System.StringSplitOptions.RemoveEmptyEntries)[0];
			}
			*/
			SkillListUI.Set(commandData_);
			SkillListUI.gameObject.SetActive(true);
		}
		else
		{
			SetTheme("White");
		}
		SetAlpha(isPreview ? PreviewAlpha : 1.0f);
		if( IsCurrentCommand || isPreview == false )
		{
			if( Animation.State == AnimComponent.AnimState.Ready )
			{
				Animation.Play();
			}
		}
	}

	public void HideCommand()
	{
		CommandName.text = "";
		Roll.gameObject.SetActive(false);
		Roll1.gameObject.SetActive(false);
		Roll2.gameObject.SetActive(false);
		Animation.ResetAnim();
		if( IsSelectedCommand )
		{
			ResetSkillList();
		}
	}

	public void ResetSkillList()
	{
		SetTheme("White");
		SkillListUI.Set(null);
		if( IsSelectedCommand )
		{
			SkillListUI.gameObject.SetActive(false);
		}
	}
}
