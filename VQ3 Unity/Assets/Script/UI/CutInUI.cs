using UnityEngine;
using System.Collections;

public class CutInUI : MonoBehaviour {

	public TextMesh Text;
	public CounterSprite Counter;

	// Use this for initialization
	void Start () {
		transform.localScale = Vector3.zero;
		GetComponent<Animation>()["CutInAnim"].speed = 1 / (float)(Music.CurrentUnitPerBeat * Music.MusicalTimeUnit);
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
		GetComponent<Animation>().Play();
	}

	public void SetDanger()
	{
		transform.localScale = Vector3.one;
		Counter.CounterColor = Color.clear;
		Text.text = "DANGER";
		Text.anchor = TextAnchor.MiddleCenter;
		Text.color = Color.red;
		GetComponent<Animation>().Play();
	}

	public void SetOverflow()
	{
		transform.localScale = Vector3.one;
		Counter.CounterColor = Color.clear;
		Text.text = "OVERFLOW";
		Text.anchor = TextAnchor.MiddleCenter;
		Text.color = Color.white;
		GetComponent<Animation>().Play();
	}

	public void SetShieldRecover()
	{
		transform.localScale = Vector3.one;
		Counter.CounterColor = Color.clear;
		Text.text = "SHIELD RECOVER";
		Text.anchor = TextAnchor.MiddleCenter;
		Text.color = ColorManager.Accent.Time;
		GetComponent<Animation>().Play();
	}
}
