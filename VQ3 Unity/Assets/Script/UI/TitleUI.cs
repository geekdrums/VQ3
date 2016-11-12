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
				AnimManager.AddAnim(ClickToStart.gameObject, ColorManager.MakeAlpha(Color.white, 0.0f), ParamType.TextColor, AnimType.Linear, 0.1f);
				AnimManager.AddAnim(TitleText.gameObject, TitleText.transform.localPosition + Vector3.down * 10, ParamType.Position, AnimType.BounceOut, 0.3f);
				AnimManager.AddAnim(gameObject, 0.0f, ParamType.PrimitiveWidth, AnimType.Time, 0.5f, 0, true);
				GameContext.SetState(GameState.Battle);
			}
		}
	}
}
