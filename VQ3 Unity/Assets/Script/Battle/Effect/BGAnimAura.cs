using UnityEngine;
using System.Collections;

public class BGAnimAura : MonoBehaviour {

	public int Cycle = 2;
	public float Size = 16.0f;
	public float MinWitdh = 0.1f;
	public float Witdh = 0.2f;
	public float Angle = 360.0f;
	public float AngleSinCoeff = 0.05f;

	private MidairPrimitive[] primitives_;

	// Use this for initialization
	void Start () {
		primitives_ = GetComponentsInChildren<MidairPrimitive>();
		transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if( GameContext.BattleState == BattleState.Battle )
		{
			transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.2f);
			for( int i=0; i<primitives_.Length; ++i )
			{
				SetParams(primitives_[i], ((Music.MusicalTimeBar + Cycle * (float)i/primitives_.Length)%Cycle)/Cycle, i % 4 == 0);
			}
		}
	}

	void SetParams(MidairPrimitive primitive, float t, bool accent)
	{
		primitive.SetSize(Mathf.Lerp(0, Size, Mathf.Sqrt(t)));
		primitive.SetWidth(accent ? Witdh : Mathf.Lerp(MinWitdh, Witdh, t));
		primitive.SetTargetColor(accent ? ColorManager.Theme.Bright : Color.Lerp(ColorManager.Theme.Light, ColorManager.Theme.Shade, t));
		primitive.transform.localRotation = Quaternion.AngleAxis(Mathf.Lerp(0, Angle, t + Mathf.Sin(t*4*Mathf.PI)*AngleSinCoeff), Vector3.forward);
	}
}
