using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyCommandPattern : MonoBehaviour {

	public EnemyCommandState Data;

	List<GameObject> rings_ = new List<GameObject>();
	int index_;

	// Use this for initialization
	void Start () {
		transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update()
	{
		if( GameContext.State == GameState.Battle )
		{
			if( GameContext.BattleState == BattleState.Eclipse )
			{
			}
			else if( GameContext.LuxState == LuxState.Overload && GameContext.LuxSystem.BreakTime > 1 )
			{
				transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.05f);
			}
			else
			{
				transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.05f);
			}
		}
	}


	public void Initialize(EnemyCommandState data)
	{
		Data = data;
		index_ = 0;
		for( int i=0; i<data.Pattern.Length; ++i )
		{
			GameObject ring = Instantiate(data.Pattern[i].RingPrefab);
			ring.transform.parent = this.transform;
			ring.transform.localPosition = Vector3.back;
			ring.transform.localScale = Vector3.one;
			rings_.Add(ring);
			ring.GetComponent<MidairPrimitive>().SetSize(GetTargetSize(i));
			ring.GetComponent<MidairPrimitive>().SetWidth(GetTargetWidth(i));
			ring.GetComponent<MidairPrimitive>().SetColor(GetTargetColor(i));
		}
	}

	public void SetCurrent()
	{
		int prevIndex = (index_ - 1 + rings_.Count)%rings_.Count;
		rings_[prevIndex].GetComponent<MidairPrimitive>().SetColor(GetTargetColor(prevIndex));
		rings_[prevIndex].GetComponent<MidairPrimitive>().SetSize(GetTargetSize(prevIndex));
		rings_[prevIndex].GetComponent<MidairPrimitive>().SetWidth(GetTargetWidth(prevIndex));
	}

	public void SetNext()
	{
		int prevIndex = index_;
		index_ = (index_ + 1)%rings_.Count;
		for( int i=index_; i<index_ + rings_.Count; ++i )
		{
			MidairPrimitive primitive = rings_[i%rings_.Count].GetComponent<MidairPrimitive>();
			if( i%rings_.Count == prevIndex )
			{
				primitive.SetTargetColor(ColorManager.MakeAlpha(primitive.Color, 0));
			}
			else
			{
				primitive.SetTargetColor(GetTargetColor(i));
				AnimManager.AddAnim(primitive.gameObject, GetTargetSize(i), ParamType.PrimitiveRadius, AnimType.Linear, 0.1f);
				AnimManager.AddAnim(primitive.gameObject, GetTargetWidth(i), ParamType.PrimitiveWidth, AnimType.Linear, 0.1f);
			}
		}
	}

	private Color GetTargetColor(int i)
	{
		return Color.Lerp(Color.white, Color.black, Mathf.Pow(Data.Pattern[i%rings_.Count].Threat/100.0f, 2));
	}

	private float GetTargetSize(int i)
	{
		return (4 / Mathf.Pow(2, (i - index_+ rings_.Count)%rings_.Count));
	}

	private float GetTargetWidth(int i)
	{
		return (0.3f - ((i - index_+ rings_.Count)%rings_.Count) * 0.05f);
	}

	public float GetCurrentThreat()
	{
		if( GameContext.LuxState == LuxState.Overload || GameContext.LuxSystem.IsInverting ) return 0;
		float threat = 0;
		for( int i=index_; i<index_ + rings_.Count; ++i )
		{
			threat += Data.Pattern[i%rings_.Count].Threat / Mathf.Pow(2, (i-index_));
			if( i >= index_ + 3 )
			{
				break;
			}
		}
		return threat;
	}
}
