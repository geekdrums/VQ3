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

[ExecuteInEditMode]
public class ResultConductor : MonoBehaviour {

    public ResultState State { get; private set; }

	// Use this for initialization
    void Awake()
    {
		GameContext.ResultConductor = this;
	}

	// Update is called once per frame
    void Update()
    {
	}

	public void CheckResult()
	{
		//GameContext.PlayerConductor.CheckResult(CurrentLevel.Encounters[encounterCount-1].AcquireStars);
		//if( encounterCount >= CurrentLevel.Encounters.Count )
		//{
		//	GameContext.PlayerConductor.Level++;
		//	GameContext.PlayerConductor.OnLevelUp();
		//	encounterCount = 0;
		//}
		//State = ResultState.Memory;
	}

    public void MoveNextResult()
    {
        ++State;
        if( State == ResultState.End )
		{
			GameContext.SetState(GameState.Setting);
        }
    }
}
