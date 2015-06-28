using UnityEngine;
using System.Collections;

public class SPPanel : MonoBehaviour {

	public CounterSprite RemainSP, MaxSP;
	public TextMesh CommandName;
	public GameObject[] LevelInfos;
	public GameObject SPBar;
	public GameObject SPSlimBar;
	public GameObject SPPanelBase;
	public MidairPrimitive BattleButton;
	public MidairPrimitive BaseFrame;
	public MidairPrimitive WindowFrame;
	public Material BGMaterial;
	public TextMesh BattleText;
	public float MaxBarSP;


	Vector3 spZeroPosition_;
	Vector3 spMaxPosition_;
	Vector3 spBarTargetScale = new Vector3(1, 0, 1);
	//Vector3 spSlimBarTargetScale = new Vector3(1, 0, 1);
	Vector3 spPanelBaseTargetScale;
	Vector3 panelTargetScale = Vector3.one;

	PlayerCommand playerCommand_;
	public enum ButtonType
	{
		None,
		Battle
	};
	ButtonType buttonType_;

	// Use this for initialization
	void Start () {
		spZeroPosition_ = CommandName.transform.localPosition;
		spMaxPosition_ = LevelInfos[LevelInfos.Length-1].transform.localPosition;
		spZeroPosition_.x = spMaxPosition_.x;
		Reset();
		SPBar.transform.localScale = spBarTargetScale;
		SPPanelBase.transform.localScale = spPanelBaseTargetScale;
		this.transform.localScale = panelTargetScale;
		Hide();
	}
	
	// Update is called once per frame
	void Update()
	{
		if( GameContext.CurrentState == GameState.SetMenu )
		{
			float SPRatio = (float)GameContext.PlayerConductor.RemainSP / GameContext.PlayerConductor.TotalSP;
			if( GameContext.PlayerConductor.RemainSP > 0 )
			{
				SPPanelBase.GetComponentInChildren<MidairPrimitive>().SetColor(Color.Lerp(ColorManager.Base.Shade, ColorManager.Base.Light, Music.MusicalCos(4) * Mathf.Clamp(SPRatio + 0.3f, 0.5f, 1.0f)));
			}
			else
			{
				SPPanelBase.GetComponentInChildren<MidairPrimitive>().SetColor(ColorManager.Base.Shade);
			}
			if( BattleButton.GetComponent<Renderer>().enabled )
			{
				Ray ray = GameContext.MainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);
				if( Input.GetMouseButtonDown(0) )
				{
					if( hit.collider == BattleButton.GetComponent<Collider>() )
					{
						buttonType_ = ButtonType.Battle;
					}
					else
					{
						buttonType_ = ButtonType.None;
					}
				}
				if( Input.GetMouseButton(0) )
				{
					if( hit.collider == BattleButton.GetComponent<Collider>() && buttonType_ == ButtonType.Battle )
					{
						BattleButton.SetTargetSize(7.0f);
						BattleButton.SetColor(ColorManager.Base.Light);
						BattleText.color = Color.black;
					}
					else
					{
						BattleButton.SetColor(ColorManager.Base.Shade);
						BattleText.color = Color.white;
					}
				}
				else if( Input.GetMouseButtonUp(0) )
				{
					if( hit.collider == BattleButton.GetComponent<Collider>() && buttonType_ == ButtonType.Battle )
					{
						GameContext.ChangeState(GameState.Field);
						GameContext.FieldConductor.CommandExp.Reset();
						Hide();
					}
				}
				else
				{
					BattleButton.SetTargetSize(7.5f);
					BattleButton.SetColor(Color.Lerp(ColorManager.Base.Shade, ColorManager.Base.Light, Music.MusicalCos(4) * Mathf.Clamp(0.3f - SPRatio, 0.0f, 0.3f)));
					BattleText.color = Color.white;
				}
			}
		}
		SPBar.transform.localScale = Vector3.Lerp(SPBar.transform.localScale, spBarTargetScale, 0.2f);
		SPPanelBase.transform.localScale = Vector3.Lerp(SPPanelBase.transform.localScale, spPanelBaseTargetScale, 0.2f);
		if( (panelTargetScale - this.transform.localScale).magnitude > 0.1f )
		{
			this.transform.localScale = Vector3.Lerp(this.transform.localScale, panelTargetScale, 0.2f);
		}
		else
		{
			this.transform.localScale = panelTargetScale;
		}
	}

	public void Set( PlayerCommand command, bool userColorAnim = false )
	{
		UpdateSP();
		playerCommand_ = command;
		BattleButton.GetComponent<Renderer>().enabled = false;
		BattleText.GetComponent<Renderer>().enabled = false;
		CommandName.text = playerCommand_.nameText.ToUpper();
		CommandName.color = playerCommand_.currentLevel == 0 ? ColorManager.Base.Shade : Color.white;
		for( int i=0; i<LevelInfos.Length; ++i )
		{
			GameObject levelInfo = LevelInfos[i];
			if( i < playerCommand_.commandData.Count && playerCommand_.commandData[i].RequireSP  > 0 )
			{
				levelInfo.transform.localScale = Vector3.one;
				bool isCurrentData = command.currentLevel-1 == i;
				levelInfo.GetComponentInChildren<MidairPrimitive>().SetColor(isCurrentData ? ColorManager.GetThemeColor(command.themeColor).Bright : ColorManager.Base.Shade);
				levelInfo.GetComponentInChildren<CounterSprite>().Count = command.commandData[i].RequireSP;
				levelInfo.GetComponentInChildren<CounterSprite>().CounterColor = isCurrentData ? Color.white : ColorManager.Base.Shade;
				levelInfo.GetComponentInChildren<TextMesh>().transform.localPosition = new Vector3(levelInfo.GetComponentInChildren<CounterSprite>().Width + 0.5f, -0.2f, 0);
				levelInfo.GetComponentInChildren<TextMesh>().color = isCurrentData ? Color.white : ColorManager.Base.Shade;
				levelInfo.transform.localPosition = spZeroPosition_ + (spMaxPosition_ - spZeroPosition_)*((float)command.commandData[i].RequireSP / MaxBarSP);
			}
			else
			{
				levelInfo.transform.localScale = Vector3.zero;
			}
		}
		spBarTargetScale = new Vector3(1, 0.9f * ((float)playerCommand_.numSP / MaxBarSP) + (command.currentLevel > 0 ? 0.1f : 0.0f), 1);
		SPSlimBar.transform.localScale = new Vector3(1, 0.9f * ((float)playerCommand_.commandData[command.commandData.Count-1].RequireSP / MaxBarSP) + 0.1f, 1);
		if( userColorAnim )
		{
			SPBar.GetComponentInChildren<MidairPrimitive>().SetAnimationColor(Color.white, ColorManager.Base.Shade);
			SPPanelBase.GetComponentInChildren<MidairPrimitive>().SetAnimationColor(Color.white, ColorManager.Base.Shade);
		}
	}
	public void Reset()
	{
		UpdateSP();
		BattleButton.GetComponent<Renderer>().enabled = true;
		BattleText.GetComponent<Renderer>().enabled = true;
		BattleButton.SetAnimationSize(0, 7.5f);
		CommandName.text = "";
		spBarTargetScale = new Vector3(1,0,1);
		SPSlimBar.transform.localScale = Vector3.zero;
		foreach( GameObject levelInfo in LevelInfos )
		{
			levelInfo.transform.localScale = Vector3.zero;
		}
	}
	public void Hide()
	{
		BattleButton.SetTargetWidth(0);
		BaseFrame.SetTargetWidth(0);
		//WindowFrame.SetTargetWidth(1.0f);
		BGMaterial.color = ColorManager.Theme.Light;
		BattleText.color = Color.clear;
		panelTargetScale = Vector3.zero;
	}
	public void ShowSPPanel()
	{
		BaseFrame.SetTargetWidth(7.5f);
		panelTargetScale = Vector3.one;
	}
	public void ShowBattleButton()
	{
		//WindowFrame.SetTargetWidth(12.0f);
		BGMaterial.color = Color.black;
		BattleButton.SetTargetWidth(7.5f);
		BattleText.color = Color.white;
	}
	public void UpdateSP()
	{
		RemainSP.Count = GameContext.PlayerConductor.RemainSP;
		MaxSP.Count = GameContext.PlayerConductor.TotalSP;
		if( GameContext.PlayerConductor.RemainSP <= 0 )
		{
			RemainSP.CounterColor = ColorManager.Base.Shade;
			MaxSP.CounterColor = ColorManager.Base.Shade;
		}
		else
		{
			RemainSP.CounterColor = Color.white;
			MaxSP.CounterColor = Color.white;
		}
		spPanelBaseTargetScale = new Vector3(1, (float)GameContext.PlayerConductor.RemainSP/GameContext.PlayerConductor.TotalSP, 1);
	}
}
