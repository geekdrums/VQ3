using UnityEngine;
using System.Collections;

public enum UIState
{
	Showing,
	Show,
	Hiding,
	Hide
}

public class CommandExplanation : MonoBehaviour {

	public GaugeRenderer NameBase;
	public TextMesh CommandName;
	public TextMesh Explanation;
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
	
	PlayerCommandData commandData_;
	UIState uiState_;

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void Update () {
		if( uiState_ == UIState.Showing && GetComponent<Animation>().isPlaying == false )
		{
			uiState_ = UIState.Show;
		}
		else if( uiState_ == UIState.Hiding && GetComponent<Animation>().isPlaying == false )
		{
			uiState_ = UIState.Hide;
			Reset();
		}
	}

	public void Set(PlayerCommand command)
	{
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		commandData_ = command.currentData;
		CommandName.text = command.nameText.ToUpper();
		CommandName.color = command.themeColor == EThemeColor.White ? Color.black : Color.white;
		ThemeColor themeColor = ColorManager.GetThemeColor(command.themeColor);
		if( commandData_ != null )
		{
			NameBase.SetColor(themeColor.Bright);
			CommandText.color = ColorManager.Base.Bright;
			LVText.color = ColorManager.Base.Bright;
			LVCount.CounterColor= ColorManager.Base.Bright;
			Explanation.text = commandData_.ExplanationText;
		}
		else
		{
			NameBase.SetColor(themeColor.Shade);
			CommandText.color = ColorManager.Base.Shade;
			LVText.color = ColorManager.Base.Shade;
			LVCount.CounterColor= ColorManager.Base.Shade;
			Explanation.text = "未習得";
		}
		GameObject iconObj = Instantiate(command.gameObject) as GameObject;
		Destroy(iconObj.transform.FindChild("nextRect").gameObject);
		iconObj.transform.parent = IconParent.transform;
		iconObj.transform.localPosition = Vector3.zero;
		iconObj.transform.localScale = Vector3.one;
		iconObj.transform.localRotation = Quaternion.identity;
		iconObj.GetComponent<PlayerCommand>().enabled = false;

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

		if( uiState_ != UIState.Show )
		{
			Show();
		}
	}


	public void Show()
	{
		GetComponent<Animation>().Play("ShowCommandExp");
		uiState_ = UIState.Showing;
		transform.localScale = Vector3.one;
	}

	public void Hide()
	{
		GetComponent<Animation>().Play("HideCommandExp");
		uiState_ = UIState.Hiding;
	}

	//public void SetEnemy( Enemy enemy )
	//{
	//	enemyData_ = enemy;
	//	if( IconParent.transform.childCount > 0 )
	//	{
	//		Destroy(IconParent.transform.GetChild(0).gameObject);
	//	}
	//	CommandName.text = enemy.name.ToUpper();
	//	Explanation.text = enemy.ExplanationText;
	//	GameObject enemyObj = Instantiate(enemy.gameObject) as GameObject;
	//	enemyObj.transform.parent = IconParent.transform;
	//	enemyObj.transform.localPosition = Vector3.zero;
	//	enemyObj.transform.localScale = enemy.transform.localScale * 2.5f;
	//	enemyObj.transform.localRotation = Quaternion.identity;
	//	enemyObj.GetComponent<Enemy>().enabled = false;
	//	enemyObj.GetComponent<SpriteRenderer>().color = Color.white;
	//}
	public void Reset()
	{
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		CommandName.text = "";
		transform.localScale = Vector3.zero;
		commandData_ = null;
		uiState_ = UIState.Hide;
		Explanation.text = "";
	}
}
