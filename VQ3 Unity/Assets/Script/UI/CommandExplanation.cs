using UnityEngine;
using System.Collections;

public class CommandExplanation : MonoBehaviour {

	public GaugeRenderer NameBase;
	public TextMesh CommandName;
	public TextMesh Explanation;
	public TextMesh NewCommmandText;
	public CounterSprite LVCount;
	public TextMesh CommandText, LVText;
	public GameObject IconParent;

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
		}
	}

	public CommandParam ATParam;
	public CommandParam HLParam;
	public CommandParam DFParam;
	public CommandParam VTParam;
	public CommandParam VPParam;

	public enum Phase
	{
		Showing,
		Wait,
		Hiding,
		Hide
	}

	public Phase CurrentPhase { get; private set; }

	PlayerCommandData commandData_;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if( CurrentPhase == Phase.Showing && GetComponent<Animation>().isPlaying == false )
		{
			CurrentPhase = Phase.Wait;
		}
		else if( CurrentPhase == Phase.Hiding && GetComponent<Animation>().isPlaying == false )
		{
			CurrentPhase = Phase.Hide;
			Reset();
		}
	}

	public void Set(PlayerCommand command)
	{
		gameObject.SetActive(true);
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		commandData_ = command.currentData;
		CommandName.text = command.nameText.ToUpper();
		CommandName.color = command.themeColor == EThemeColor.White ? Color.black : Color.white;
		ThemeColor themeColor = ColorManager.GetThemeColor(command.themeColor);

		if( GameContext.State == GameState.Result || GameContext.State == GameState.Event )
		{
			NameBase.SetColor(themeColor.Bright);
			NewCommmandText.color = ColorManager.Base.Bright;
			LVText.color = Color.clear;
			LVCount.CounterColor= Color.clear;
			CommandText.color = Color.clear;
			Explanation.text = "";
			commandData_ = command.commandData[command.commandData.Count-1];
		}
		else if( commandData_ != null )
		{
			NameBase.SetColor(themeColor.Bright);
			CommandText.color = ColorManager.Base.Bright;
			LVText.color = ColorManager.Base.Bright;
			LVCount.CounterColor= ColorManager.Base.Bright;
			Explanation.text = commandData_.ExplanationText.Replace("<br/>", System.Environment.NewLine);
			if( GameContext.FieldConductor.EncountIndex == 0 )
			{
				Explanation.text = commandData_.ExplanationText.Split(new string[]{"<br/>"}, System.StringSplitOptions.RemoveEmptyEntries)[0];
			}
			NewCommmandText.color = Color.clear;
		}
		else
		{
			NameBase.SetColor(themeColor.Shade);
			CommandText.color = ColorManager.Base.Shade;
			LVText.color = ColorManager.Base.Shade;
			LVCount.CounterColor= ColorManager.Base.Shade;
			Explanation.text = "未習得";
			NewCommmandText.color = Color.clear;
		}
		command.GetIconObj(IconParent);

		ATParam.SetParam(command.GetAtk());
		ATParam.SetColor(command.GetAtkColor());
		DFParam.SetParam(command.GetDefend());
		DFParam.SetColor(command.GetDefColor());
		HLParam.SetParam(command.GetHeal());
		HLParam.SetColor(command.GetHealColor());
		VTParam.SetParam(command.GetVT());
		VTParam.SetColor(command.GetVTColor());
		VPParam.SetParam(command.GetVP());
		VPParam.SetColor(command.GetVPColor());
		LVCount.Count = command.currentLevel;

		if( CurrentPhase != Phase.Wait || GameContext.State == GameState.Result )
		{
			Show();
		}
	}


	public void Show()
	{
		GetComponent<Animation>().Play("ShowCommandExp");
		CurrentPhase = Phase.Showing;
		transform.localScale = Vector3.one;
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		GetComponent<Animation>().Play("HideCommandExp");
		CurrentPhase = Phase.Hiding;
	}

	public void Reset()
	{
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		CommandName.text = "";
		transform.localScale = Vector3.zero;
		commandData_ = null;
		CurrentPhase = Phase.Hide;
		Explanation.text = "";
	}
}
