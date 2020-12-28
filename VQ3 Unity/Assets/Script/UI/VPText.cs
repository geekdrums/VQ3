using UnityEngine;
using System.Collections;

public class VPText : MonoBehaviour
{
	public CounterSprite VPCount;
	public CounterSprite VTCount;

	public float MaxCounterScale;
	public float MidCounterScale;
	public float MinCounterScale;

	float time_ = 0;
	bool isEnd_ = false;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
#if UNITY_EDITOR
		if( UnityEditor.EditorApplication.isPlaying == false )
		{
			return;
		}
#endif

		if( isEnd_ )
		{
			return;
		}

		bool isEnd = false;
		time_ += Time.deltaTime;
		isEnd |= time_ >= 2.0f;
		isEnd |= GameContext.LuxSystem.IsOverFlow;
		isEnd |= GameContext.LuxSystem.State != LuxState.Overload && Music.IsJustChangedAt(3, 3, 0);
		if( isEnd )
		{
			isEnd_ = true;
			Destroy(this.gameObject);
		}
	}
	
	public void AddVP(int VP, int Time)
	{
		VTCount.Count += Time / LuxSystem.TurnMusicalUnits;
		VTCount.gameObject.SetActive(VTCount.Count > 0);
		VPCount.Count += VP;
		time_ = 0;
		UpdateColors(VP, Time / LuxSystem.TurnMusicalUnits);

		VPCount.Shake(0.3f, 1.0f);
		VTCount.Shake(0.3f, 1.0f);
	}

	public void InitializeVP(int VP, int Time)
	{
		VTCount.Count = Time / LuxSystem.TurnMusicalUnits;
		VTCount.gameObject.SetActive(VTCount.Count > 0);
		VPCount.Count = VP;

		VPCount.Shake(0.4f, 1.0f);
		VTCount.Shake(0.4f, 1.0f);

		UpdateColors(VP, Time / LuxSystem.TurnMusicalUnits);
	}

	void UpdateColors(int VP, float Time)
	{
		Color vpColor = Color.white;
		if( VP >= 10 )
		{
			vpColor = ColorManager.GetColor("Theme").ResultColor;
			VPCount.CounterScale = MaxCounterScale;
		}
		else if( VP >= 4 )
		{
			vpColor = ColorManager.GetColor("ThemeShade").ResultColor;
			VPCount.CounterScale = MidCounterScale;
		}
		else
		{
			vpColor = ColorManager.GetColor("BaseMiddleBack").ResultColor;
			VPCount.CounterScale = MinCounterScale;
		}
		VPCount.CounterColor = vpColor;
		VPCount.GetComponentInChildren<TextMesh>().color = vpColor;
		
		Color timeColor = Color.white;
		if( Time >= 0.5f )
		{
			timeColor = ColorManager.GetColor("Theme").ResultColor;
			VTCount.CounterScale = MaxCounterScale;
		}
		else if( Time >= 0.2f )
		{
			timeColor = ColorManager.GetColor("ThemeShade").ResultColor;
			VTCount.CounterScale = MidCounterScale;
		}
		else
		{
			timeColor = ColorManager.GetColor("BaseMiddleBack").ResultColor;
			VTCount.CounterScale = MinCounterScale;
		}
		VTCount.CounterColor = timeColor;
		VTCount.GetComponentInChildren<TextMesh>().color = timeColor;
	}

}
