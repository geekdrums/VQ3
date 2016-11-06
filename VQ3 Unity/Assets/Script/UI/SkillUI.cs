using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour {
	
	public int length;
	public Color textColor;
	public Color baseColor;

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

	GaugeRenderer line_;
	TextMesh text_;

	// Use this for initialization
	void Awake () {
		line_ = GetComponentInChildren<GaugeRenderer>();
		text_ = GetComponentInChildren<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
