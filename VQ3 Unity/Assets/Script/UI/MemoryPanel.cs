using UnityEngine;
using System;
using System.Collections;

public class MemoryPanel : MonoBehaviour {

	public CounterSprite RemainMemory, MemorySize;
	public GameObject[] LevelInfos;
	public GaugeRenderer MemoryGauge;
	public GaugeRenderer MemorySlimGauge;
	public GaugeRenderer RemainMemoryGauge;
	public ButtonUI BattleButton;
	public ButtonUI UpButton;
	public ButtonUI DownButton;
	public ButtonUI BackButton;
	public MidairPrimitive BaseFrame;

	public CommandExplanation CommandExp;

	Vector3 memoryZeroPosition_;
	Vector3 memoryMaxPosition_;

	PlayerCommand playerCommand_;

	// Use this for initialization
	void Start () {
		BattleButton.OnPushed += this.OnBattleButtonPushed;
		UpButton.OnPushed += this.OnUpButtonPushed;
		DownButton.OnPushed += this.OnDownButtonPushed;
		BackButton.OnPushed += this.OnBackButtonPushed;
		memoryZeroPosition_ = LevelInfos[0].transform.localPosition;
		memoryMaxPosition_ = LevelInfos[LevelInfos.Length-1].transform.localPosition;
	}
	
	// Update is called once per frame
	void Update()
	{
		if( GameContext.State == GameState.Setting )
		{
			float MemoryRatio = (float)GameContext.PlayerConductor.RemainMemory / GameContext.PlayerConductor.TotalMemory;
			if( GameContext.PlayerConductor.RemainMemory > 0 )
			{
				RemainMemoryGauge.SetColor(Color.Lerp(ColorManager.Base.Middle, ColorManager.Base.Front, Music.MusicalCos(16) * Mathf.Clamp(MemoryRatio + 0.3f, 0.0f, 1.0f)));
			}
		}
	}

	void OnBattleButtonPushed(object sender, EventArgs e)
	{
		if( GameContext.State == GameState.Setting )
		{
			GameContext.SetState(GameState.Battle);
			Hide();
		}
	}

	void OnUpButtonPushed(object sender, EventArgs e)
	{
		SEPlayer.Play("commandLevelUp");
		playerCommand_.LevelUp();
		Set(playerCommand_);
		GameContext.PlayerConductor.CommandGraph.CheckLinkedFromIntro();
	}

	void OnDownButtonPushed(object sender, EventArgs e)
	{
		SEPlayer.Play("commandLevelDown");
		playerCommand_.LevelDown();
		Set(playerCommand_);
		GameContext.PlayerConductor.CommandGraph.CheckLinkedFromIntro();
	}

	void OnBackButtonPushed( object sender, EventArgs e )
	{
		SEPlayer.Play("tickback");
		GameContext.PlayerConductor.CommandGraph.Deselect();
		Reset();
	}

	public void Set(PlayerCommand command)
	{
		UpdateMemory();
		playerCommand_ = command;
		BattleButton.SetMode(ButtonMode.Hide);
		bool enableUp = command.currentLevel < command.commandData.Count;
		if( enableUp )
		{
			int needMemory = command.commandData[command.currentLevel].RequireSP - (command.currentLevel == 0 ? 0 : command.commandData[command.currentLevel-1].RequireSP);
			enableUp &= needMemory <= GameContext.PlayerConductor.RemainMemory;
		}
		bool enableDown = command.currentLevel > 0 && command.currentData.RequireSP > 0;
		UpButton.SetMode(enableUp ? ButtonMode.Active : ButtonMode.Disable);
		DownButton.SetMode(enableDown ? ButtonMode.Active: ButtonMode.Disable);
		BackButton.SetMode(ButtonMode.Active);
		float maxLvMemory = command.commandData[command.commandData.Count-1].RequireSP;
		for( int i=0; i<LevelInfos.Length; ++i )
		{
			GameObject levelInfo = LevelInfos[i];
			if( i < playerCommand_.commandData.Count && playerCommand_.commandData[i].RequireSP  > 0 )
			{
				levelInfo.transform.localScale = Vector3.one;
				bool isCurrentData = command.currentLevel-1 == i;
				levelInfo.GetComponentInChildren<MidairPrimitive>().SetColor(isCurrentData ? ColorManager.Base.Front : ColorManager.Base.Shade);
				levelInfo.GetComponentInChildren<CounterSprite>().Count = command.commandData[i].RequireSP;
				levelInfo.GetComponentInChildren<CounterSprite>().CounterColor = isCurrentData ? Color.white : ColorManager.Base.Shade;
				levelInfo.transform.localPosition = memoryZeroPosition_ + (memoryMaxPosition_ - memoryZeroPosition_)*((float)command.commandData[i].RequireSP / maxLvMemory);
			}
			else
			{
				levelInfo.transform.localScale = Vector3.zero;
			}
		}
		MemorySlimGauge.SetColor(ColorManager.GetThemeColor(command.themeColor).Bright);
		MemorySlimGauge.SetRate(1, 0.1f);
		MemoryGauge.SetColor(ColorManager.GetThemeColor(command.themeColor).Bright);
		MemoryGauge.SetRate((playerCommand_.currentLevel > 0 && maxLvMemory > 0 ? (0.9f * (float)playerCommand_.commandData[playerCommand_.currentLevel-1].RequireSP / maxLvMemory + 0.1f) : 0), 0.1f);

		CommandExp.Set(command);
	}

	public void Reset()
	{
		this.transform.localScale = Vector3.one;
		BaseFrame.SetTargetWidth(7.5f);
		BaseFrame.SetColor(ColorManager.Base.Back);
		UpdateMemory();
		BattleButton.SetMode((GameContext.PlayerConductor.CanStartBattle() ? ButtonMode.Active : ButtonMode.Disable));
		UpButton.SetMode(ButtonMode.Hide);
		DownButton.SetMode(ButtonMode.Hide);
		BackButton.SetMode(ButtonMode.Hide);
		MemoryGauge.SetRate(0);
		MemorySlimGauge.SetRate(0);
		foreach( GameObject levelInfo in LevelInfos )
		{
			levelInfo.transform.localScale = Vector3.zero;
		}
		CommandExp.Hide();
		TextWindow.SetMessage(MessageCategory.Help, "コマンドにメモリーを分配できます");
	}

	public void Hide()
	{
		this.transform.localScale = Vector3.zero;
		BattleButton.SetMode(ButtonMode.Hide);
		UpButton.SetMode(ButtonMode.Hide);
		DownButton.SetMode(ButtonMode.Hide);
		BackButton.SetMode(ButtonMode.Hide);
		BaseFrame.SetTargetWidth(0);
		CommandExp.Hide();
	}

	public void UpdateMemory()
	{
		RemainMemory.Count = GameContext.PlayerConductor.RemainMemory;
		MemorySize.Count = GameContext.PlayerConductor.TotalMemory;
		RemainMemory.CounterColor = ColorManager.Base.Shade;
		MemorySize.CounterColor = ColorManager.Base.Shade;
		RemainMemoryGauge.SetRate((float)GameContext.PlayerConductor.RemainMemory/GameContext.PlayerConductor.TotalMemory, 0.1f);
	}
}
