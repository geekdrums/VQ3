using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {

    //public TitleLogo titleLogo;

    public GameObject skillParent;
    List<Pair<Timing, Skill>> Skills;

    void Awake()
    {
        if( Application.platform == RuntimePlatform.WindowsPlayer ||
        Application.platform == RuntimePlatform.OSXPlayer ||
        Application.platform == RuntimePlatform.LinuxPlayer )
        {
            Screen.SetResolution( 480, 720, false );
            //Screen.SetResolution( 400, 600, false );
        }
    }

	// Use this for initialization
	void Start ()
	{
		GameContext.BattleConductor = this;
        Skills = new List<Pair<Timing, Skill>>();
    }
	
	// Update is called once per frame
    void Update()
    {
		switch ( GameContext.CurrentState )
		{
		case GameState.Intro:
            if( Music.IsJustChangedAt( 0 ) || Music.IsJustChangedAt( 4 ) )
            {
                if( Music.CurrentBlockName != "intro" )
                {
                    GameContext.ChangeState( GameState.Battle );
                    UpdateBattle();
                }
			}
			break;
		case GameState.Battle:
            UpdateBattle();
            if( Music.IsJustChangedAt( 0 ) && Music.CurrentBlockName == "endro" )
			{
                TextWindow.ClearTutorialMessage();
                GameContext.VoxSystem.SetState( VoxState.SunSet );
				GameContext.ChangeState( GameState.Endro );
                TextWindow.ChangeMessage( BattleMessageType.Result, "てきを　たおした" );
			}
            break;
		case GameState.Endro:
            if( Music.IsJustChangedAt( 0, 2 ) )
            {
                TextWindow.SetNextCursor( true );
                //titleLogo.animation.Play();
            }
            if( !Music.IsPlaying() || ( Music.Just.totalUnit > 8 && Input.GetMouseButtonUp( 0 ) && GameContext.PlayerConductor.commandGraph.CurrentButton != VoxButton.None) )
            {
                ClearSkills();
                GameContext.ChangeState( GameState.Field );
            }
            break;
        case GameState.Continue:
            break;
		}
    }

    void UpdateBattle()
    {
        if( Music.CurrentBlockName == "endro" || Music.NextBlockName == "endro" )
        {
            Skills.RemoveAll( ( Pair<Timing, Skill> cmd ) => cmd.Get<Skill>().CheckIsEnd( cmd.Get<Timing>() ) );
            return;
        }
        else if( Music.CurrentBlockName == "intro" )
        {
            OnPlayerRunaway();
            return;
        }

        if( Music.IsJustChangedAt( 0 ) )
        {
            if( Music.CurrentBlockName == "wait" )
            {
                GameContext.PlayerConductor.CheckWaitCommand();
                GameContext.EnemyConductor.CheckWaitCommand();
            }
            else
            {
                GameContext.PlayerConductor.CheckCommand();
                GameContext.EnemyConductor.CheckCommand();
            }
        }
        if( Music.CurrentBlockName == "wait" )
        {
        }
        else if( Music.isJustChanged )
        {
            GameContext.PlayerConductor.CheckSkill();
            GameContext.EnemyConductor.CheckSkill();
            GameContext.PlayerConductor.UpdateHealHP();
            GameContext.EnemyConductor.UpdateHealHP();

            List<Pair<ActionSet, Skill>> CurrentActions = new List<Pair<ActionSet, Skill>>();
            foreach( Pair<Timing, Skill> stPair in Skills )
            {
                if( !stPair.Get<Skill>().OwnerCharacter.isAlive ) continue;
                ActionSet act = stPair.Get<Skill>().GetCurrentAction( stPair.Get<Timing>() );
                if( act != null )
                {
                    CurrentActions.Add( new Pair<ActionSet, Skill>( act, stPair.Get<Skill>() ) );
                }
            }
            //Add setoff logic if needed.
            foreach( Pair<ActionSet, Skill> act in CurrentActions )
            {
                bool isSucceeded = false;
                isSucceeded |= GameContext.PlayerConductor.ReceiveAction( act.Get<ActionSet>(), act.Get<Skill>() );
                if( GameContext.CurrentState == GameState.Continue ) break;
                isSucceeded |=  GameContext.EnemyConductor.ReceiveAction( act.Get<ActionSet>(), act.Get<Skill>() );
                if( GameContext.CurrentState == GameState.Endro ) break;
                if( isSucceeded )
                {
                    act.Get<Skill>().OnExecuted( act.Get<ActionSet>() );
                }
                if( Music.NextBlockName == "endro" )
                {
                    break;
                }
            }

			Skills.RemoveAll( ( Pair<Timing, Skill> cmd ) => cmd.Get<Skill>().CheckIsEnd( cmd.Get<Timing>() ) );
        }
    }

    void ClearSkills()
    {
        for( int i = 0; i < skillParent.transform.childCount; i++ )
        {
            Destroy( skillParent.transform.GetChild( i ).gameObject );
        }
        Skills.Clear();
    }

	public void ExecSkill( Skill NewSkill )
	{
        NewSkill.gameObject.transform.parent = skillParent.transform;
        Skills.Add( new Pair<Timing, Skill>( new Timing( Music.Just ), NewSkill ) );
	}

	public void OnPlayerWin()
	{
        Music.SetNextBlock("endro");
        GameContext.PlayerConductor.OnPlayerWin();
        GameContext.EnemyConductor.OnPlayerWin();
	}
	public void OnPlayerLose()
    {
        TextWindow.ClearTutorialMessage();
        TextWindow.ChangeMessage( BattleMessageType.Result, "オクスは　ちからつきた" );
        GameContext.PlayerConductor.OnPlayerLose();
        GameContext.EnemyConductor.OnPlayerLose();
        GameContext.VoxSystem.SetState( VoxState.SunSet );
        GameContext.ChangeState( GameState.Continue );
        Music.Play( "Continue" );
        ClearSkills();
	}
    public void OnPlayerRunaway()
    {
        TextWindow.ClearTutorialMessage();
        TextWindow.ChangeMessage( BattleMessageType.Result, "オクスは　にげだした" );
        GameContext.PlayerConductor.OnPlayerLose();
        GameContext.EnemyConductor.OnPlayerLose();
        GameContext.VoxSystem.SetState( VoxState.SunSet );
        GameContext.ChangeState( GameState.Continue );
        Music.Stop();
        SEPlayer.Play( "runaway" );
        ClearSkills();
    }
}
