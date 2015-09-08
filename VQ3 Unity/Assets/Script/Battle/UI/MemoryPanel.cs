﻿using UnityEngine;
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
	public MidairPrimitive BaseFrame;
	public Material BGMaterial;
	public Material GaugeMaterial;

	public CommandExplanation CommandExp;

	Vector3 memoryZeroPosition_;
	Vector3 memoryMaxPosition_;

	PlayerCommand playerCommand_;

	// Use this for initialization
	void Start () {
		//Reset();
		BattleButton.OnPushed += this.OnBattleButtonPushed;
		UpButton.OnPushed += this.OnUpButtonPushed;
		DownButton.OnPushed += this.OnDownButtonPushed;
		memoryZeroPosition_ = LevelInfos[0].transform.localPosition;
		memoryMaxPosition_ = LevelInfos[LevelInfos.Length-1].transform.localPosition;
	}
	
	// Update is called once per frame
	void Update()
	{
		if( GameContext.State == GameState.Setting )
		{
			float SPRatio = (float)GameContext.PlayerConductor.RemainSP / GameContext.PlayerConductor.TotalSP;
			if( GameContext.PlayerConductor.RemainSP > 0 )
			{
				GaugeMaterial.color = Color.Lerp(ColorManager.Base.Shade, ColorManager.Base.Light, Music.MusicalCos(4) * Mathf.Clamp(SPRatio + 0.3f, 0.5f, 1.0f));
			}
			else
			{
				GaugeMaterial.color = ColorManager.Base.Shade;
			}
		}
	}

	void OnBattleButtonPushed(object sender, EventArgs e)
	{
		GameContext.SetState(GameState.Battle);
		Hide();
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

	public void Set(PlayerCommand command)
	{
		UpdateSP();
		playerCommand_ = command;
		BattleButton.SetMode(ButtonMode.Hide);
		bool enableUp = command.currentLevel < command.commandData.Count;
		if( enableUp )
		{
			int needSP = command.commandData[command.currentLevel].RequireSP - (command.currentLevel == 0 ? 0 : command.commandData[command.currentLevel-1].RequireSP);
			enableUp &= needSP <= GameContext.PlayerConductor.RemainSP;
		}
		bool enableDown = command.currentLevel > 0 && command.currentData.RequireSP > 0;
		UpButton.SetMode(enableUp ? ButtonMode.Active : ButtonMode.Disable);
		DownButton.SetMode(enableDown ? ButtonMode.Active: ButtonMode.Disable);
		float maxLvMemory = command.commandData[command.commandData.Count-1].RequireSP;
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
				levelInfo.transform.localPosition = memoryZeroPosition_ + (memoryMaxPosition_ - memoryZeroPosition_)*((float)command.commandData[i].RequireSP / maxLvMemory);
			}
			else
			{
				levelInfo.transform.localScale = Vector3.zero;
			}
		}
		MemorySlimGauge.SetRate(1, 0.1f);
		MemoryGauge.SetRate((playerCommand_.currentLevel > 0 && maxLvMemory > 0 ? (0.9f * (float)playerCommand_.commandData[playerCommand_.currentLevel-1].RequireSP / maxLvMemory + 0.1f) : 0), 0.1f);

		CommandExp.Set(command);
	}

	public void Reset()
	{
		UpdateSP();
		BattleButton.SetMode(ButtonMode.Active);
		UpButton.SetMode(ButtonMode.Hide);
		DownButton.SetMode(ButtonMode.Hide);
		MemoryGauge.SetRate(0);
		MemorySlimGauge.SetRate(0);
		foreach( GameObject levelInfo in LevelInfos )
		{
			levelInfo.transform.localScale = Vector3.zero;
		}
		CommandExp.Hide();
	}

	public void Hide()
	{
		this.transform.localScale = Vector3.zero;
		BattleButton.SetMode(ButtonMode.Hide);
		UpButton.SetMode(ButtonMode.Hide);
		DownButton.SetMode(ButtonMode.Hide);
		BaseFrame.SetTargetWidth(0);
	}

	public void Show()
	{
		this.transform.localScale = Vector3.one;
		BaseFrame.SetTargetWidth(7.5f);
		BattleButton.SetMode(ButtonMode.Active);
		UpButton.SetMode(ButtonMode.Hide);
		DownButton.SetMode(ButtonMode.Hide);
		UpdateSP();
		RemainMemoryGauge.EndAnim();
		MemoryGauge.SetRate(0);
		MemorySlimGauge.SetRate(0);
	}

	public void UpdateSP()
	{
		RemainMemory.Count = GameContext.PlayerConductor.RemainSP;
		MemorySize.Count = GameContext.PlayerConductor.TotalSP;
		if( GameContext.PlayerConductor.RemainSP <= 0 )
		{
			RemainMemory.CounterColor = ColorManager.Base.Light;
			MemorySize.CounterColor = ColorManager.Base.Light;
		}
		else
		{
			RemainMemory.CounterColor = Color.white;
			MemorySize.CounterColor = Color.white;
		}
		RemainMemoryGauge.SetRate((float)GameContext.PlayerConductor.RemainSP/GameContext.PlayerConductor.TotalSP, 0.1f);
	}
}