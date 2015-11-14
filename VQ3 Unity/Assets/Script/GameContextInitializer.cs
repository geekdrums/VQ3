using UnityEngine;
using System.Collections;

public class GameContextInitializer : MonoBehaviour
{
	public GameState GameState;
	public int ScreenWidth = 400;
	public int ScreenHeight = 710;
	public int EncounterIndex = 0;

	void Awake()
	{
		if( Application.platform == RuntimePlatform.WindowsPlayer ||
        Application.platform == RuntimePlatform.OSXPlayer ||
        Application.platform == RuntimePlatform.LinuxPlayer )
		{
			Screen.SetResolution(ScreenWidth, ScreenHeight, false);
		}
		GameContext.FieldConductor.EncounterIndex = EncounterIndex;
	}

	void Start()
	{
		GameContext.SetState(GameState);
	}
}
