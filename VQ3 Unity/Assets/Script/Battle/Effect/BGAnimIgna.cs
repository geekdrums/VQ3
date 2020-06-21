using UnityEngine;
using System.Collections;

public class BGAnimIgna : BGAnimBase
{
	public float Size = 16.0f;
	public float MinWitdh = 0.1f;
	public float Witdh = 0.2f;
	public float Offset = 0;

	protected override void SetParams(MidairPrimitive primitive, float t, bool accent)
	{
		primitive.SetSize(Offset + Mathf.Lerp(0, Size, t*t*t));
		primitive.SetWidth(Mathf.Lerp(MinWitdh, Witdh, t));
		primitive.SetColor(accent ? ColorManagerObsolete.Theme.Bright : Color.Lerp(ColorManagerObsolete.Theme.Light, ColorManagerObsolete.Theme.Shade, t));// AnimType.Linear
	}
}
