using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StaticColorSource : ColorSourceBase
{
	[SerializeField]
	protected Color Source = Color.white;

	public override Color SourceColor
	{
		get
		{
			return Source;
		}
	}

	public override void GetSourceHSVA(out float H, out float S, out float V, out float A)
	{
#if UNITY_EDITOR
		ColorPropertyUtil.ToHSVA(Source, out H, out S, out V, out A);
#else
		H = sourceH;
		S = sourceS;
		V = sourceV;
		A = sourceA;
#endif
	}

	float sourceH, sourceS, sourceV, sourceA;

	protected override void OnValidate()
	{
		base.OnValidate();
		ColorPropertyUtil.ToHSVA(Source, out sourceH, out sourceS, out sourceV, out sourceA);
		GetSourceHSVA(out h_, out s_, out v_, out a_);
	}

	protected override void Awake()
	{
		base.Awake();
		ColorPropertyUtil.ToHSVA(Source, out sourceH, out sourceS, out sourceV, out sourceA);
		GetSourceHSVA(out h_, out s_, out v_, out a_);
	}
}