using UnityEngine;
using System.Collections;

public class GameContextInitializer : MonoBehaviour
{
	public GameState GameState;

	void Start()
	{
		GameContext.SetState(GameState);
	}
}
