using UnityEngine;
using System.Collections;

public class GaugeRenderer : MonoBehaviour {

	public GameObject LineMesh;
	public GameObject LineParent;
	public float Length = 2.0f;
	public float Rate = 1.0f;
	public float Width = 1;
	public Vector3 Direction = Vector3.right;

	float baseRate_ = 1.0f;
	float targetRate_ = 1.0f;
	float animTime_, remainTime_;

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
		if( remainTime_ > 0 )
		{
			remainTime_ -= Time.deltaTime;
			Rate = Mathf.Lerp(targetRate_, baseRate_, Mathf.Max(0, remainTime_/animTime_));
		}
		if( IsHorizontal )
		{
			LineParent.transform.localScale = new Vector3(Length * Rate, Width, 1);
		}
		else
		{
			LineParent.transform.localScale = new Vector3(Width, Length * Rate, 1);
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

	public void SetColor(Color color)
	{
		LineMesh.GetComponent<Renderer>().material.color = color;
	}
}
