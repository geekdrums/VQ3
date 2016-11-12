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
			AnimManager.AddAnim(gameObject, Vector3.zero, ParamType.Position, AnimType.Linear, 0.2f);
			AnimManager.AddAnim(text_.gameObject, new Vector3(0, -0.7f, -1), ParamType.Position, AnimType.Linear, 0.2f);
			AnimManager.AddAnim(text_.gameObject, ColorManager.Base.Dark, ParamType.TextColor, AnimType.Linear, 0.2f);
			AnimManager.AddAnim(gameObject, 0.2f, ParamType.GaugeWidth, AnimType.Linear, 0.3f);

			AnimManager.AddAnim(text_.gameObject, Color.clear, ParamType.TextColor, AnimType.Linear, 0.2f, (length * 16 - 8) * (float)Music.MusicalTimeUnit);
			AnimManager.AddAnim(gameObject, 0.0f, ParamType.GaugeLength, AnimType.BounceOut, 0.2f, (length * 16 - 8) * (float)Music.MusicalTimeUnit);
		}
		else
		{
			AnimManager.AddAnim(gameObject, new Vector3(0, 2.3f, 0), ParamType.Position, AnimType.Linear, 0.2f);
			AnimManager.AddAnim(text_.gameObject, new Vector3(0, 0.9f, -1), ParamType.Position, AnimType.Linear, 0.2f);
			AnimManager.AddAnim(text_.gameObject, Color.white, ParamType.TextColor, AnimType.Linear, 0.2f);
			AnimManager.AddAnim(gameObject, Color.white, ParamType.Color, AnimType.Linear, 0.2f);
			AnimManager.AddAnim(gameObject, 0.2f, ParamType.GaugeWidth, AnimType.Linear, 0.3f);

			AnimManager.AddAnim(text_.gameObject, Color.clear, ParamType.TextColor, AnimType.Linear, 0.2f, (length * 16 - 8) * (float)Music.MusicalTimeUnit);
			AnimManager.AddAnim(gameObject, 0.0f, ParamType.GaugeLength, AnimType.BounceOut, 0.2f, (length * 16 - 8) * (float)Music.MusicalTimeUnit);
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
					AnimManager.RemoveAnim(text_.gameObject);
					AnimManager.RemoveAnim(gameObject);
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
