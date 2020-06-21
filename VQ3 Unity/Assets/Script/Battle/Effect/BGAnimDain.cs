using UnityEngine;
using System.Collections;

public class BGAnimDain : BGAnimBase
{
	public float Size = 16.0f;
	public float MinWitdh = 0.1f;
	public float Witdh = 0.2f;
	public float Angle = 360.0f;
	public float AngleSinCoeff = 0.05f;
	public float Offset = 1.0f;

	protected override void SetParams(MidairPrimitive primitive, float t, bool accent)
	{
		primitive.SetSize(Offset + Mathf.Lerp(0, Size, t * t));
		primitive.SetWidth(Mathf.Lerp(MinWitdh, Witdh, t));
		primitive.SetColor(accent ? ColorManagerObsolete.Theme.Bright : Color.Lerp(ColorManagerObsolete.Theme.Light, ColorManagerObsolete.Theme.Shade, t));
		primitive.transform.localRotation = Quaternion.AngleAxis(Mathf.Lerp(0, Angle, t + Mathf.Sin(t*4*Mathf.PI)*AngleSinCoeff), Vector3.forward);
	}
}
