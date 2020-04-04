using UnityEngine;
using System.Collections;

public class TitleUI : MonoBehaviour {

	public TextMesh ClickToStart;
	public TextMesh TitleText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if( GameContext.State == GameState.Title )
		{
			ClickToStart.color = ColorManager.MakeAlpha(Color.white, 0.7f + 0.3f * Music.MusicalCos());
			if( Input.GetMouseButtonDown(0) )
			{
				ClickToStart.AnimateColor(ColorManager.MakeAlpha(Color.white, 0.0f), time: 0.1f);//AnimType.Linear
				TitleText.transform.AnimatePosition(TitleText.transform.localPosition + Vector3.down * 10, InterpType.BackIn, time: 0.3f);
				AnimManager.AddAnim(gameObject, 0.0f, AnimParamType.PrimitiveWidth, InterpType.Linear, time: 0.5f, endOption: AnimEndOption.Destroy);
				GameContext.SetState(GameState.Battle);
			}
		}
	}
}
