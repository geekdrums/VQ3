using UnityEngine;
using System.Collections;

public class GameContextInitializer : MonoBehaviour
{
	public GameState GameState;

	void Awake()
	{
		if( Application.platform == RuntimePlatform.WindowsPlayer ||
        Application.platform == RuntimePlatform.OSXPlayer ||
        Application.platform == RuntimePlatform.LinuxPlayer )
		{
			//Screen.SetResolution(320 * 2, 568 * 2, false);
			Screen.SetResolution(400, 710, false);
		}
	}

	void Start()
	{
		//for(int i=0;i<GameContext.FieldConductor.EncountIndex; ++i)
		//{
		//	Encounter encounter = GameContext.FieldConductor.StageData[0].Encounters[i];
		//	add encounter.AcquireMemory;
		//}
		//foreach( PlayerCommand command in GameContext.PlayerConductor.CommandGraph.CommandNodes )
		//{
		//	sub command.numSP;
		//}
		//GameContext.PlayerConductor.CommandGraph.CheckLinkedFromIntro();

		GameContext.SetState(GameState);
	}
}
