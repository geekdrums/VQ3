using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BattleState
{
	None,
	Intro,
	Battle,
	Wait,
	ShieldBreak,
	Eclipse,
	Continue,
	Win,
	Endro
}

public class BattleConductor : MonoBehaviour {

	public BattleState State { get; private set; }

    public GameObject SkillParent;
	public ButtonUI OKButton;

    List<Pair<Timing, Skill>> Skills;

	void Awake()
	{
		GameContext.BattleConductor = this;
	}

	// Use this for initialization
	void Start ()
	{
        Skills = new List<Pair<Timing, Skill>>();
		OKButton.OnPushed += this.OnPushedOKButton;
    }
	
	// Update is called once per frame
	void Update()
	{
		switch( State )
		{
		case BattleState.None:
			return;
		case BattleState.Intro:
			if( Music.IsJustChangedAt(0) || Music.IsJustChangedAt(4) )
			{
				if( Music.CurrentBlockName != "Intro" )
				{
					SetState(BattleState.Battle);
					UpdateBattle();
				}
			}
			break;
		case BattleState.ShieldBreak:
			break;
		case BattleState.Wait:
			if( Music.IsJustChangedAt(0) )
			{
				GameContext.PlayerConductor.ExecWaitCommand();
				GameContext.EnemyConductor.CheckWaitCommand();
			}
			break;
		case BattleState.Battle:
		case BattleState.Eclipse:
			if( Music.IsJustChangedAt(0) )
			{
				GameContext.PlayerConductor.ExecCommand();
				GameContext.EnemyConductor.CheckCommand();
			}
			UpdateBattle();
			break;
		case BattleState.Continue:
			break;
		case BattleState.Win:
			UpdateBattle();
			break;
		case BattleState.Endro:
			if( Music.IsJustChangedAt(0, 2) )
			{
				TextWindow.SetNextCursor(true);
			}
			break;
		}
	}


	void OnPushedOKButton(object sender, System.EventArgs e)
	{
		switch( GameContext.BattleState )
		{
		case BattleState.Endro:
			if( !Music.IsPlaying || Music.Just.MusicalTime >= 8 )
			{
				ClearSkills();
				GameContext.SetState(GameState.Result);
				ColorManager.SetBaseColor(EBaseColor.Black);
				SetState(BattleState.None);
			}
			break;
		case BattleState.Continue:
			if( !Music.IsPlaying || Music.Just.MusicalTime > 4 )
			{
				GameContext.SetState(GameState.Setting);
			}
			break;
		}
	}

    void UpdateBattle()
    {
		if( Music.IsJustChanged )
		{
			GameContext.PlayerConductor.CheckSkill();
			GameContext.EnemyConductor.CheckSkill();
			GameContext.PlayerConductor.UpdateHealHP();
			GameContext.EnemyConductor.UpdateHealHP();

			List<Pair<ActionSet, Skill>> CurrentActions = new List<Pair<ActionSet, Skill>>();
			foreach( Pair<Timing, Skill> stPair in Skills )
			{
				if( !stPair.Get<Skill>().OwnerCharacter.isAlive ) continue;
				ActionSet act = stPair.Get<Skill>().GetCurrentAction(stPair.Get<Timing>());
				if( act != null )
				{
					CurrentActions.Add(new Pair<ActionSet, Skill>(act, stPair.Get<Skill>()));
				}
			}
			foreach( Pair<ActionSet, Skill> act in CurrentActions )
			{
				bool isSucceeded = false;
				isSucceeded |= GameContext.PlayerConductor.ReceiveAction(act.Get<ActionSet>(), act.Get<Skill>());
				if( State == BattleState.Continue ) break;
				isSucceeded |=  GameContext.EnemyConductor.ReceiveAction(act.Get<ActionSet>(), act.Get<Skill>());
				if( isSucceeded )
				{
					act.Get<Skill>().OnExecuted(act.Get<ActionSet>());
				}
				if( State == BattleState.Win ) break;
			}

			Skills.RemoveAll((Pair<Timing, Skill> cmd) => cmd.Get<Skill>().CheckIsEnd(cmd.Get<Timing>()));
		}
    }

    public void ClearSkills()
    {
		for( int i = 0; i < SkillParent.transform.childCount; i++ )
		{
			Skill skill = SkillParent.transform.GetChild(i).GetComponent<Skill>();
			skill.OwnerCharacter.OnSkillEnd(skill);
			Destroy(SkillParent.transform.GetChild(i).gameObject);
		}
		Skills.Clear();
    }

	public void ExecSkill(Skill NewSkill)
	{
		NewSkill.gameObject.transform.parent = SkillParent.transform;
		NewSkill.gameObject.transform.localPosition = new Vector3(NewSkill.gameObject.transform.localPosition.x, NewSkill.gameObject.transform.localPosition.y, NewSkill.isPlayerSkill ? -1 : 0);
		Skills.Add(new Pair<Timing, Skill>(new Timing(Music.Just), NewSkill));
	}


	public void SetState(BattleState NewState)
	{
		if( State == NewState ) return;

		//Leave State
		switch( State )
		{
		case BattleState.Continue:
			GameContext.EnemyConductor.OnContinue();
			GameContext.PlayerConductor.OnContinue();
			GameContext.FieldConductor.OnContinue();
			break;
		}

		State = NewState;
		
		//Enter  State
		switch( State )
		{
		case BattleState.Intro:
			OKButton.SetMode(ButtonMode.Hide);
			break;
		case BattleState.Battle:
			break;
		case BattleState.Wait:
			break;
		case BattleState.Eclipse:
			break;
		case BattleState.ShieldBreak:
			break;
		case BattleState.Continue:
			TextWindow.SetMessage(MessageCategory.Result, "戦闘不能、緊急離脱。");
			GameContext.PlayerConductor.OnPlayerLose();
			GameContext.EnemyConductor.OnPlayerLose();
			GameContext.LuxSystem.SetState(LuxState.SunSet);
			Music.Play("Continue");
			ClearSkills();
			OKButton.SetText("Continue");
			OKButton.SetMode(ButtonMode.Active, true);
			break;
		case BattleState.Win:
			Music.SetNextBlock("endro", new System.EventHandler((object sender, System.EventArgs e) => { SetState(BattleState.Endro); }));
			GameContext.PlayerConductor.OnPlayerWin();
			GameContext.EnemyConductor.OnPlayerWin();
			break;
		case BattleState.Endro:
			ClearSkills();
			GameContext.LuxSystem.SetState(LuxState.SunSet);
			TextWindow.SetMessage(MessageCategory.Result, "敵の殲滅を確認。");
			OKButton.SetText("OK");
			OKButton.SetMode(ButtonMode.Active, true);
			GameContext.PlayerConductor.OnEndro();
			break;
		}
		Debug.Log("Enter BattleState: " + State.ToString());
	}

	//public void OnPlayerRunaway()
	//{
	//	TextWindow.ChangeMessage( MessageCategory.Result, "オクスは　にげだした" );
	//	GameContext.PlayerConductor.OnPlayerLose();
	//	GameContext.EnemyConductor.OnPlayerLose();
	//	GameContext.VoxSystem.SetState( VoxState.SunSet );
	//	SetState(BattleState.Continue);
	//	Music.Stop();
	//	SEPlayer.Play( "runaway" );
	//	ClearSkills();
	//}
}
