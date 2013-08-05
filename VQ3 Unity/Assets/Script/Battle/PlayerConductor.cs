using UnityEngine;
using System.Collections;

public class PlayerConductor : MonoBehaviour {

	public GameObject MainCamera;
	Player Player;

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = MainCamera.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnBarStarted( int CurrentIndex )
	{
		Player.OnBarStarted( CurrentIndex );
	}

	public bool ReceiveAction( ActionSet Action, Command command )
	{
		bool isSucceeded = false;
		AttackModule attack = Action.GetModule<AttackModule>();
        if( attack != null && !command.isPlayerAction )
		{
			Player.BeAttacked( attack );
			isSucceeded = true;
		}
		DefendModule defend = Action.GetModule<DefendModule>();
        if( defend != null && command.isPlayerAction )
		{
			Player.Defend( defend );
			isSucceeded = true;
		}
		HealModule heal = Action.GetModule<HealModule>();
        if( heal != null && command.isPlayerAction )
		{
			Player.Heal( heal );
			isSucceeded = true;
		}
		return isSucceeded;
	}
}
