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

	public void ReceiveAction( ActionSet Action, bool isPlayerAction )
	{
		AttackModule attack = Action.GetModule<AttackModule>();
		if ( attack != null && !isPlayerAction )
		{
			Player.BeAttacked( attack );
		}
		DefendModule defend = Action.GetModule<DefendModule>();
		if ( defend != null && isPlayerAction )
		{
			Player.Defend( defend );
		}
		HealModule heal = Action.GetModule<HealModule>();
		if ( heal != null && isPlayerAction )
		{
			Player.Heal( heal );
		}
	}
}
