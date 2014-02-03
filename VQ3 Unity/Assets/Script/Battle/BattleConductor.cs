using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {
    public GameObject skillParent;
    public VoxonSystem voxonSystem;

    BGEffect CurrentBGEffect;
    
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
			if ( Music.IsJustChangedAt( 0 ) && Music.GetCurrentBlockName() != "intro" )
			{
				GameContext.ChangeState( GameContext.GameState.Battle );
				UpdateBattle();
			}
			break;
		case GameContext.GameState.Battle:
            UpdateBattle();
			if ( Music.IsJustChangedAt(0) && Music.GetCurrentBlockName() == "endro" )
			{
				voxonSystem.SetState( VoxonSystem.VoxonState.HideBreak );
				GameContext.ChangeState( GameContext.GameState.Endro );
				TextWindow.AddMessage( "てきを　やっつけた！", "３のけいけんちを　えた！" );
			}
            break;
		case GameContext.GameState.Endro:
			if ( !Music.IsPlaying() )
			{
				GameContext.ChangeState( GameContext.GameState.Field );
			}
            break;
		}
    }

    void UpdateBattle()
    {
        if( Music.GetCurrentBlockName() == "endro" ) return;

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

	public void ExecSkill( Skill NewSkill )
	{
        NewSkill.gameObject.transform.parent = skillParent.transform;
        Skills.Add( new Pair<Timing, Skill>( new Timing( Music.Just ), NewSkill ) );
	}

    public void SetBGEffect( GameObject BGEffectPrefab )
    {
        if( CurrentBGEffect != null && (BGEffectPrefab == null || CurrentBGEffect.GetType() != BGEffectPrefab.GetComponent<BGEffect>().GetType()) )
        {
            CurrentBGEffect.Hide();
        }
        if( BGEffectPrefab != null && (CurrentBGEffect == null || CurrentBGEffect.GetType() != BGEffectPrefab.GetComponent<BGEffect>().GetType()) )
        {
            GameObject bgObj = Instantiate( BGEffectPrefab, Vector3.zero, BGEffectPrefab.transform.rotation ) as GameObject;
            bgObj.transform.parent = transform;
            CurrentBGEffect = bgObj.GetComponent<BGEffect>();
        }
    }


	public void OnPlayerWin()
	{
        Music.SetNextBlock("endro");
	}
	public void OnPlayerLose()
	{
	}
}
