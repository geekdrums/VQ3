using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum ResultState
{
	None,
	Memory,
	Level,
    Command,
	End
}

public class ResultConductor : MonoBehaviour {

	public ButtonUI OKButton;
	public MemoryResult MemoryResult;
	public LevelResult LevelResult;
	public CommandExplanation CommandExp;

    public ResultState State { get; private set; }

    void Awake()
    {
		GameContext.ResultConductor = this;
	}


	// Use this for initialization
	void Start()
	{
		OKButton.OnPushed += this.OnPushedOKButton;
	}

	// Update is called once per frame
    void Update()
	{
	}

	public void OnEnterResult(int memory)
	{
		MemoryResult.Show(memory);
		State = ResultState.Memory;
	}
	

	public void OnPushedOKButton(object sender, System.EventArgs e)
	{
		if( GameContext.State == GameState.Result )
		{
			switch( State )
			{
			case ResultState.Memory:
				if( MemoryResult.CurrentPhase == MemoryResult.Phase.Wait )
				{
					MemoryResult.Hide();
					if( MemoryResult.IsLevelUp )
					{
						State = ResultState.Level;
						GameContext.PlayerConductor.OnLevelUp();
						LevelResult.Show();
					}
					else
					{
						EndResult();
					}
				}
				break;
			case ResultState.Level:
				if( LevelResult.CurrentPhase == LevelResult.Phase.Wait )
				{
					State = ResultState.Command;
					LevelResult.Hide();
					CheckAcquireCommands();
				}
				break;
			case ResultState.Command:
				if( CommandExp.CurrentPhase == CommandExplanation.Phase.Wait )
				{
					CheckAcquireCommands();
				}
				break;
			}
		}
	}

	void CheckAcquireCommands()
	{
		PlayerCommand acquiredCommand = GameContext.PlayerConductor.CheckAcquireCommand();
		if( acquiredCommand != null )
		{
			acquiredCommand.Acquire();
			GameContext.PlayerConductor.CommandGraph.ShowAcquireCommand(acquiredCommand);
			//TextWindow.SetCommand(acquiredCommand);
			TextWindow.SetMessage(MessageCategory.Result, acquiredCommand.name + "が習得可能になった");
			CommandExp.Set(acquiredCommand);
			OKButton.Primitive.AnimateWidth(0);// AnimType.Linear
			OKButton.SetText("");
		}
		else
		{
			CommandExp.Hide();
			EndResult();
		}
	}


	void EndResult()
	{
		GameContext.SetState(GameState.Setting);
		OKButton.Primitive.SetWidth(7.3f);
		OKButton.SetText("OK");
	}
}
