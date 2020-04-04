using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour {

	public string skillName;
	public int length;
	public int number;
	public Color textColor;
	public Color baseColor;
	public bool isEnemySkill;

	public bool WillBeExecuted { get { return length > 0 && isExecuting_ == false; } }

	bool isExecuting_;
	int remainLength_;
	Vector3 initialPosition_;

	public void Set(string skillName, int length, Color baseColor, Color textColor, bool isEnemySkill = false)
	{
		this.skillName = skillName;
		this.length = length;
		this.baseColor = baseColor;
		this.textColor = textColor;
		this.isEnemySkill = isEnemySkill;
	}

	public void Reset()
	{
		this.skillName = "";
		this.length = 0;
		this.baseColor = Color.clear;
		this.textColor = Color.clear;
	}

	public void Show()
	{
		line_.SetColor(baseColor);
		line_.Length = Mathf.Max(0, 5.0f * length - 0.5f);

		text_.text = skillName;
		text_.color = textColor;
	}

	public void Execute()
	{
		isExecuting_ = true;

		if( isEnemySkill )
		{
			AnimManager.AddAnim(gameObject, Vector3.zero, AnimParamType.Position, InterpType.Linear, time: 0.2f); // AnimType.Linear
			AnimManager.AddAnim(text_.gameObject, new Vector3(0, -0.7f, -1), AnimParamType.Position, InterpType.Linear, time: 0.2f);// AnimType.Linear
			AnimManager.AddAnim(text_.gameObject, ColorManager.Base.Dark, AnimParamType.TextColor, InterpType.Linear, time: 0.2f);// AnimType.Linear
			AnimManager.AddAnim(gameObject, 0.2f, AnimParamType.GaugeWidth, InterpType.Linear, time: 0.3f);// AnimType.Linear

			AnimManager.AddAnim(text_.gameObject, Color.clear, AnimParamType.TextColor, InterpType.Linear, time: 0.2f, delay: (length * 16 - 8) * (float)Music.Meter.SecPerUnit);// AnimType.Linear
			AnimManager.AddAnim(gameObject, 0.0f, AnimParamType.GaugeLength, InterpType.BackIn, time: 0.2f, delay: (length * 16 - 8) * (float)Music.Meter.SecPerUnit);
		}
		else
		{
			AnimManager.AddAnim(gameObject, new Vector3(0, 2.3f, 0), AnimParamType.Position, InterpType.Linear, time: 0.2f);// AnimType.Linear
			AnimManager.AddAnim(text_.gameObject, new Vector3(0, 0.9f, -1), AnimParamType.Position, InterpType.Linear, time: 0.2f);// AnimType.Linear
			AnimManager.AddAnim(text_.gameObject, Color.white, AnimParamType.TextColor, InterpType.Linear, time: 0.2f);// AnimType.Linear
			AnimManager.AddAnim(gameObject, Color.white, AnimParamType.Color, InterpType.Linear, time: 0.2f);// AnimType.Linear
			AnimManager.AddAnim(gameObject, 0.2f, AnimParamType.GaugeWidth, InterpType.Linear, time: 0.3f);// AnimType.Linear

			AnimManager.AddAnim(text_.gameObject, Color.clear, AnimParamType.TextColor, InterpType.Linear, time: 0.2f, delay: (length * 16 - 8) * (float)Music.Meter.SecPerUnit);// AnimType.Linear
			AnimManager.AddAnim(gameObject, 0.0f, AnimParamType.GaugeLength, InterpType.BackIn, time: 0.2f, delay: (length * 16 - 8) * (float)Music.Meter.SecPerUnit);
		}

		remainLength_ = length;
	}

	GaugeRenderer line_;
	TextMesh text_;

	// Use this for initialization
	void Awake () {
		line_ = GetComponentInChildren<GaugeRenderer>();
		text_ = GetComponentInChildren<TextMesh>();
		initialPosition_ = transform.localPosition;
		number = new List<SkillUI>(transform.parent.GetComponentsInChildren<SkillUI>()).IndexOf(this);
	}
	
	// Update is called once per frame
	void Update () {
		if( isExecuting_ )
		{
			if( Music.IsJustChangedBar() && Music.Just.Bar > number )
			{
				--remainLength_;
			}

			if( remainLength_ <= 0 || Music.IsJustChangedAt(CommandGraph.AllowInputEnd) )
			{
				isExecuting_ = false;
				Reset();
				if( isEnemySkill )
				{
					AnimManager.RemoveOtherAnim(text_.gameObject);
					AnimManager.RemoveOtherAnim(gameObject);
					transform.localPosition = initialPosition_;
					text_.transform.localPosition = Vector3.back;
					text_.text = "";
					line_.SetWidth(1.5f);
				}
				else
				{
					Destroy(this.gameObject);
				}
			}
		}
	}
}
