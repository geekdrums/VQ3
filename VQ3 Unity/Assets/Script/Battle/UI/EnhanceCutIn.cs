using UnityEngine;
using System.Collections;

public class EnhanceCutIn : MonoBehaviour {

	public TextMesh Text;
	public CounterSprite Counter;

	// Use this for initialization
	void Start () {
		transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Set( string text, int phase, int percent )
	{
		transform.localScale = Vector3.one;
		Text.text = text;
		Counter.Count = percent;
		Counter.CounterColor = (phase >= 0 ? ColorManager.Accent.Buff : ColorManager.Accent.DeBuff);
		Text.color = (phase >= 0 ? ColorManager.Accent.Buff : ColorManager.Accent.DeBuff);
		Text.anchor = TextAnchor.MiddleRight;
		animation["EnhCutInAnim"].speed = 1 / (float)(Music.mtBeat * Music.MusicTimeUnit);
		animation.Play();
	}

	public void SetDanger()
	{
		transform.localScale = Vector3.one;
		Counter.CounterColor = Color.clear;
		Text.text = "DANGER";
		Text.anchor = TextAnchor.MiddleCenter;
		Text.color = Color.red;
		animation["EnhCutInAnim"].speed = 1 / (float)(Music.mtBeat * Music.MusicTimeUnit);
		animation.Play();
	}
}
