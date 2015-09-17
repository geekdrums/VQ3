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
			Screen.SetResolution(320, 568, false);
		}
	}

	void Start()
	{
		GameContext.SetState(GameState);
	}
}
