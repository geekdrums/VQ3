using UnityEngine;
using System.Collections;

public class GameContextInitializer : MonoBehaviour
{
	public GameState GameState;
	public int EncounterIndex = 0;

	int ScreenWidth = 1366;
	int ScreenHeight = 768;

	void Awake()
	{
		if( Application.platform == RuntimePlatform.WindowsPlayer ||
        Application.platform == RuntimePlatform.OSXPlayer ||
        Application.platform == RuntimePlatform.LinuxPlayer )
		{
			Screen.SetResolution(ScreenWidth, ScreenHeight, false);
			//Screen.fullScreen = true;
		}
		GameContext.FieldConductor.EncounterIndex = EncounterIndex;
		//Cursor.visible = false;
	}

	void Start()
	{
		GameContext.SetState(GameState);
	}
}
