using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {
    public GameObject skillParent;    
    List<Pair<Timing, Skill>> Skills;

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
		case GameContext.GameState.Intro:
            if( Music.IsJustChangedAt( 0 ) || Music.IsJustChangedAt( 4 ) )
            {
                if( Music.GetCurrentBlockName() != "intro" )
                {
                    GameContext.ChangeState( GameContext.GameState.Battle );
                    UpdateBattle();
                }
			}
			break;
		case GameContext.GameState.Battle:
            UpdateBattle();
			if ( Music.IsJustChangedAt(0) && Music.GetCurrentBlockName() == "endro" )
			{
                GameContext.VoxSystem.SetState( VoxState.SunSet );
				GameContext.ChangeState( GameContext.GameState.Endro );
                TextWindow.ChangeMessage( "てきを　やっつけた！" );
			}
            break;
		case GameContext.GameState.Endro:
			if ( !Music.IsPlaying() )
			{
                ClearSkills();
				GameContext.ChangeState( GameContext.GameState.Field );
			}
            break;
        case GameContext.GameState.Continue:
            break;
		}
    }

    void UpdateBattle()
    {
        if( Music.GetCurrentBlockName() == "endro" || Music.GetNextBlockName() == "endro" )
        {
            Skills.RemoveAll( ( Pair<Timing, Skill> cmd ) => cmd.Get<Skill>().CheckIsEnd( cmd.Get<Timing>() ) );
            return;
        }

        if( Music.IsJustChangedAt( 0 ) )
        {
            GameContext.PlayerConductor.CheckCommand();
            GameContext.EnemyConductor.CheckCommand();
        }
        if( Music.isJustChanged )
        {
            GameContext.PlayerConductor.CheckSkill();
            GameContext.EnemyConductor.CheckSkill();

            List<Pair<ActionSet, Skill>> CurrentActions = new List<Pair<ActionSet, Skill>>();
            foreach( Pair<Timing, Skill> stPair in Skills )
            {
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
                isSucceeded |=  GameContext.EnemyConductor.ReceiveAction( act.Get<ActionSet>(), act.Get<Skill>() );
                if( isSucceeded )
                {
                    act.Get<Skill>().OnExecuted( act.Get<ActionSet>() );
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
        TextWindow.ChangeMessage( "オクスは　しんでしまった", "ボールを　おして　ふっかつ　できます");
        GameContext.PlayerConductor.OnPlayerLose();
        GameContext.EnemyConductor.OnPlayerLose();
        GameContext.VoxSystem.SetState( VoxState.SunSet );
        GameContext.ChangeState( GameContext.GameState.Continue );
        Music.Play( "Continue" );
        ClearSkills();
	}
}
