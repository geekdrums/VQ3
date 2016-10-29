using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour {
	
	public int length;
	public Color textColor;
	public Color baseColor;
	public bool isPassive;

	public void Set(string name, bool isPassive, int length, Color baseColor, Color textColor)
	{
		this.name = name;
		this.isPassive = isPassive;
		this.length = length;
		this.baseColor = baseColor;
		this.textColor = textColor;
	}

	public void Reset()
	{
		this.name = "";
		this.isPassive = false;
		this.length = 0;
		this.baseColor = Color.clear;
		this.textColor = Color.clear;
	}

	public void Show()
	{
		if( isPassive )
		{
			line_.Width = 1.9f;
			transform.localPosition = new Vector3(transform.localPosition.x, 1.0f, 0);
		}
		else
		{
			line_.Width = 1.5f;
			transform.localPosition = new Vector3(transform.localPosition.x, 1.2f, 0);
		}

		line_.SetColor(baseColor);
		line_.Length = Mathf.Max(0, 5.0f * length - 0.5f);

		text_.text = name;
		text_.color = textColor;
	}

	GaugeRenderer line_;
	TextMesh text_;

	// Use this for initialization
	void Start () {
		line_ = GetComponentInChildren<GaugeRenderer>();
		text_ = GetComponentInChildren<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
