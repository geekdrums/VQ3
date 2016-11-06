using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour {
	
	public int length;
	public Color textColor;
	public Color baseColor;

	bool isExecuting_;
	int remainLength_;

	public void Set(string name, int length, Color baseColor, Color textColor)
	{
		this.name = name;
		this.length = length;
		this.baseColor = baseColor;
		this.textColor = textColor;
	}

	public void Reset()
	{
		this.name = "";
		this.length = 0;
		this.baseColor = Color.clear;
		this.textColor = Color.clear;
	}

	public void Show()
	{
		line_.SetColor(baseColor);
		line_.Length = Mathf.Max(0, 5.0f * length - 0.5f);

		text_.text = name;
		text_.color = textColor;
	}

	public void Execute()
	{
		isExecuting_ = true;
		text_.transform.parent = transform.parent;
		text_.color = line_.LineColor;
		AnimManager.AddAnim(text_.gameObject, new Vector3(-49, 1.2f, 0), ParamType.Position, AnimType.Linear, 0.2f);
		AnimManager.AddAnim(gameObject, new Vector3(-50, 0.3f, 0), ParamType.Position, AnimType.Linear, 0.2f);
		AnimManager.AddAnim(gameObject, 7.0f, ParamType.GaugeLength, AnimType.Linear, 0.1f);
		AnimManager.AddAnim(gameObject, 0.2f, ParamType.GaugeWidth, AnimType.Linear, 0.3f);

		AnimManager.AddAnim(text_.gameObject, new Vector3(-100, 1.2f, 0), ParamType.Position, AnimType.Linear, 0.1f, (length * 16 - 8) * (float)Music.MusicalTimeUnit);
		AnimManager.AddAnim(gameObject, new Vector3(-100, 0.3f, 0), ParamType.Position, AnimType.Linear, 0.1f, (length * 16 - 8) * (float)Music.MusicalTimeUnit);

		remainLength_ = length;
	}

	GaugeRenderer line_;
	TextMesh text_;

	// Use this for initialization
	void Awake () {
		line_ = GetComponentInChildren<GaugeRenderer>();
		text_ = GetComponentInChildren<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		if( isExecuting_ )
		{
			if( Music.IsJustChangedBar() )
			{
				--remainLength_;
				if( remainLength_ < 0 )
				{
					Destroy(this.gameObject);
				}
			}
		}
	}
}
