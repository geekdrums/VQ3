using UnityEngine;
using System.Collections;

public class GaugeRenderer : MonoBehaviour {

	public GameObject LineMesh;
	public GameObject LineParent;
	public Color LineColor;
	public float Length = 2.0f;
	public float Rate = 1.0f;
	public float Width = 1;
	public Vector3 Direction = Vector3.right;

	float baseRate_ = 1.0f;
	float targetRate_ = 1.0f;
	float animTime_, remainTime_;
	Color baseColor_ = Color.white;
	Color targetColor_ = Color.white;
	float colorAnimTime_, colorRemainTime_;

	bool IsHorizontal { get { return Mathf.Abs(Direction.x) > 0; } }

	void OnValidate()
	{
		if( LineParent == null && LineMesh != null )
		{
			LineParent = new GameObject("LineParent");
			LineParent.transform.parent = this.transform;
			LineMesh.transform.parent = LineParent.transform;
			LineParent.transform.localPosition = Vector3.zero;
			LineMesh.transform.localPosition = Vector3.zero;
		}

		if( LineParent != null )
		{
			UpdateLine();
		}
	}

	// Use this for initialization
	void Start () {
		LineColor = LineMesh.GetComponent<Renderer>().material.color;
	}
	
	// Update is called once per frame
	void Update () {
		if( LineParent != null )
		{
			UpdateLine();
		}
	}

	void UpdateLine()
	{
		LineMesh.transform.localPosition = Direction * 0.5f;
		if( IsHorizontal )
		{
			LineParent.transform.localScale = new Vector3(Length * Rate, Width, 1);
		}
		else
		{
			LineParent.transform.localScale = new Vector3(Width, Length * Rate, 1);
		}

		if( remainTime_ > 0 )
		{
			remainTime_ -= Time.deltaTime;
			Rate = Mathf.Lerp(targetRate_, baseRate_, Mathf.Max(0, remainTime_/animTime_));
		}

		if( colorRemainTime_ > 0 )
		{
			colorRemainTime_ -= Time.deltaTime;
			LineColor = Color.Lerp(targetColor_, baseColor_, Mathf.Max(0, colorRemainTime_/colorAnimTime_));
			LineMesh.GetComponent<Renderer>().material.color = LineColor;
		}
	}

	public void EndAnim()
	{
		animTime_ = 0;
		remainTime_ = 0;
		baseRate_ = targetRate_;
		Rate = targetRate_;
	}

	public void SetRate(float rate, float animTime = 0)
	{
		if( animTime > 0 )
		{
			animTime_ = animTime;
			remainTime_ = animTime;
			targetRate_ = rate;
			baseRate_ = Rate;
		}
		else
		{
			animTime_ = 0;
			remainTime_ = 0;
			targetRate_ = rate;
			baseRate_ = rate;
			Rate = rate;
		}
	}

	public void SetColor(Color color, float animTime = 0)
	{
		if( animTime > 0 )
		{
			colorAnimTime_ = animTime;
			colorRemainTime_ = animTime;
			targetColor_ = color;
			baseColor_ = LineColor;
		}
		else
		{
			colorAnimTime_ = 0;
			colorRemainTime_ = 0;
			targetColor_ = color;
			baseColor_ = color;
			LineColor = color;
			LineMesh.GetComponent<Renderer>().material.color = LineColor;
		}
	}
}
