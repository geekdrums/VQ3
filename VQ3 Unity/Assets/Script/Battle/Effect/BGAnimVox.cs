using UnityEngine;
using System.Collections;

public class BGAnimVox : BGAnimBase
{
	public float Size = 16.0f;
	public float MinWitdh = 0.1f;
	public float Witdh = 0.2f;
	public float Offset = 1.0f;

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();

		if( GameContext.BattleState == BattleState.Battle && IsActive )
		{
			for( int i=0; i<primitives_.Length; ++i )
			{
				Quaternion targetRot = Quaternion.AngleAxis((45 * (int)(Music.MusicalTime + i/32.0f))%180, Vector3.forward);
				primitives_[i].transform.localRotation = Quaternion.Lerp(primitives_[i].transform.localRotation, targetRot, 0.2f);
			}
		}
		else if( GameContext.BattleState == BattleState.Endro && IsActive )
		{
			Quaternion targetRot = Quaternion.AngleAxis(45, Vector3.forward);
			for( int i = 0; i < primitives_.Length; ++i )
			{
				primitives_[i].transform.localRotation = targetRot;
			}
		}
	}

	protected override void SetParams(MidairPrimitive primitive, float t, bool accent)
	{
		primitive.SetSize(Offset + Mathf.Lerp(0, Size, 1.0f - Mathf.Sqrt(t)));
		primitive.SetWidth(Mathf.Lerp(MinWitdh, Witdh, 1.0f - t));
		primitive.SetTargetColor(accent ? ColorManager.Theme.Bright : Color.Lerp(ColorManager.Theme.Light, ColorManager.Theme.Shade, t));
	}
}
