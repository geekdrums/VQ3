using UnityEngine;
using System.Collections;

public class CommandExplanation : MonoBehaviour, IColoredObject
{

	public GaugeRenderer NameBase;
	public TextMesh CommandName;
	public TextMesh Explanation;
	public CounterSprite LVCount;
	public TextMesh LVText;
	//public GameObject IconParent;
	public GaugeRenderer TopLine;//, BottomLine;

	[System.Serializable]
	public class CommandParam
	{
		public CounterSprite Counter;
		public GaugeRenderer Line;
		public TextMesh Text;

		public void SetColor(Color color)
		{
			Counter.CounterColor = color;
			Line.SetColor(color);
			Text.color = color;
		}

		public void SetParam(float param)
		{
			Counter.Count = param;
			Line.SetRate(0, 0);
			Line.SetRate(Mathf.Clamp01(param/100.0f));
		}

		public void SetZero()
		{
			SetColor(ColorManager.Base.MiddleBack);
			SetParam(0);
		}

		public void Hide()
		{
			SetColor(Color.clear);
			SetParam(0);
		}
	}

	public CommandParam ATParam;
	public CommandParam HLParam;
	public CommandParam DFParam;
	public CommandParam VTParam;
	public CommandParam VPParam;
	public SkillListUI SkillListUI;

	public enum Phase
	{
		Showing,
		Wait,
		Hiding,
		Hide
	}

	public Phase CurrentPhase { get; private set; }

	PlayerCommandData commandData_;
	Color currentColor_ = Color.clear;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if( CurrentPhase == Phase.Showing && AnimManager.IsAnimating(gameObject) == false )
		{
			CurrentPhase = Phase.Wait;
		}
		else if( CurrentPhase == Phase.Hiding && AnimManager.IsAnimating(gameObject) == false )
		{
			CurrentPhase = Phase.Hide;
			Reset();
		}
	}

	public void Set(PlayerCommand command)
	{
		gameObject.SetActive(true);
		TopLine.gameObject.SetActive(true);
		//if( IconParent.transform.childCount > 0 )
		//{
		//	Destroy(IconParent.transform.GetChild(0).gameObject);
		//}
		commandData_ = command.currentData;
		CommandName.text = command.nameText.ToUpper();
		ThemeColor themeColor = ColorManager.GetThemeColor(command.themeColor);

		//GameObject iconObj = command.InstantiateIconObj(IconParent);
		if( GameContext.State == GameState.Result || GameContext.State == GameState.Event )
		{
			NameBase.SetColor(themeColor.Bright);
			LVText.gameObject.SetActive(false);
			Explanation.text = "";
			commandData_ = command.commandData[command.commandData.Count-1];
			//iconObj.GetComponent<PlayerCommand>().maskPlane.SetActive(false);
			if( GameContext.State == GameState.Event )
			{
				TopLine.gameObject.SetActive(false);
			}
		}
		else if( commandData_ != null )
		{
			NameBase.SetColor(themeColor.Bright);
			LVText.gameObject.SetActive(true);
			LVText.color = ColorManager.Base.Bright;
			LVCount.CounterColor= ColorManager.Base.Bright;
			Explanation.text = commandData_.ExplanationText.Replace("<br/>", System.Environment.NewLine);
			if( GameContext.FieldConductor.EncounterIndex == 0 )
			{
				Explanation.text = commandData_.ExplanationText.Split(new string[]{"<br/>"}, System.StringSplitOptions.RemoveEmptyEntries)[0];
			}
			SkillListUI.Set(commandData_);
		}
		else
		{
			NameBase.SetColor(themeColor.Shade);
			LVText.gameObject.SetActive(true);
			LVText.color = ColorManager.Base.Shade;
			LVCount.CounterColor= ColorManager.Base.Shade;
			Explanation.text = "未習得";
		}

		if( commandData_ != null )
		{
			ATParam.SetParam(commandData_.GetAtk());
			ATParam.SetColor(commandData_.GetAtkColor());
			DFParam.SetParam(commandData_.GetDefend());
			DFParam.SetColor(commandData_.GetDefColor());
			HLParam.SetParam(commandData_.GetHeal());
			HLParam.SetColor(commandData_.GetHealColor());
			if( GameContext.FieldConductor.CurrentEncounter.Version < LuxVersion.Shield )
			{
				VPParam.Hide();
			}
			else
			{
				VPParam.SetParam(commandData_.GetVP());
				VPParam.SetColor(commandData_.GetVPColor());
			}
			if( GameContext.FieldConductor.CurrentEncounter.Version < LuxVersion.AutoShield )
			{
				VTParam.Hide();
			}
			else
			{
				VTParam.SetParam(commandData_.GetVT());
				VTParam.SetColor(commandData_.GetVTColor());
			}
		}
		else
		{
			ATParam.SetZero();
			DFParam.SetZero();
			HLParam.SetZero();
			if( GameContext.FieldConductor.CurrentEncounter.Version < LuxVersion.Shield )
			{
				VPParam.Hide();
			}
			else
			{
				VPParam.SetZero();
			}
			if( GameContext.FieldConductor.CurrentEncounter.Version < LuxVersion.AutoShield )
			{
				VTParam.Hide();
			}
			else
			{
				VTParam.SetZero();
			}
		}

		LVCount.Count = command.currentLevel;

		TopLine.SetColor(themeColor.Bright);
		//BottomLine.SetColor(themeColor.Bright);

		if( CurrentPhase != Phase.Wait || GameContext.State == GameState.Result )
		{
			Show();
		}
	}


	public void Show()
	{
		TopLine.SetRate(0);
		TopLine.SetRate(1, 0.2f);
		//BottomLine.SetRate(0);
		//BottomLine.SetRate(1, 0.2f);
		currentColor_ = Color.clear;
		AnimManager.AddAnim(gameObject, Color.white, ParamType.Color);
		//IconParent.transform.localScale = Vector3.zero;
		//AnimManager.AddAnim(IconParent, 0.3f, ParamType.Scale, AnimType.BounceIn, 0.2f);

		CurrentPhase = Phase.Showing;
		transform.localScale = Vector3.one;
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		TopLine.SetRate(0, 0.2f);
		//BottomLine.SetRate(0, 0.2f);
		currentColor_ = Color.white;
		AnimManager.AddAnim(gameObject, Color.white, ParamType.Color);
		//AnimManager.AddAnim(IconParent, 0.0f, ParamType.Scale, AnimType.BounceOut, 0.2f);

		CurrentPhase = Phase.Hiding;
	}

	public void Reset()
	{
		//if( IconParent.transform.childCount > 0 )
		//{
		//	Destroy(IconParent.transform.GetChild(0).gameObject);
		//}
		CommandName.text = "";
		transform.localScale = Vector3.zero;
		commandData_ = null;
		CurrentPhase = Phase.Hide;
		Explanation.text = "";
	}

	public void SetColor(Color color)
	{
		currentColor_ = color;
		LVText.color = currentColor_;
		LVCount.CounterColor = currentColor_;
		CommandName.color = currentColor_;
		Explanation.color = currentColor_;
	}

	public Color GetColor()
	{
		return currentColor_;
	}
}
