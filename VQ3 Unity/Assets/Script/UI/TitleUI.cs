using UnityEngine;
using System.Collections;

public class TitleUI : MonoBehaviour {

	public enum Phase
	{
		None,
		Title,
		Anim,
		Ending
	}
	public GameObject Teams;
	Phase CurrentPhase = Phase.Title;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if( GameContext.State == GameState.Title )
		{
			Teams.SetActive(false);
			if( Input.GetMouseButtonDown(0) )
			{
				GetComponent<Animation>().Play("titleAnim");
				CurrentPhase = Phase.Anim;
			}
			if( CurrentPhase == Phase.Anim && GetComponent<Animation>().isPlaying == false )
			{
				CurrentPhase = Phase.None;
				GameContext.SetState(GameState.Setting);
				gameObject.SetActive(false);
				Teams.SetActive(true);
			}
		}
	}
}
