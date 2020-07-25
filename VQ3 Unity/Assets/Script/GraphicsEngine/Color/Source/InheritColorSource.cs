using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InheritColorSource : ColorSourceBase
{
	[SerializeField]
	protected ColorSourceBase Source;

	public override Color SourceColor
	{
		get
		{
			return Source != null ? Source : Color.clear;
		}
	}

	public override void GetSourceHSVA(out float H, out float S, out float V, out float A)
	{
		if( Source != null )
		{
			H = Source.H;
			S = Source.S;
			V = Source.V;
			A = Source.A;
		}
		else
		{
			H = S = V = A = 0;
		}
	}

	public override IEnumerable<ColorSourceBase> GetReferenceColors()
	{
		if( Source == null )
		{
			yield break;
		}
		else
		{
			yield return Source;
			foreach( ColorSourceBase sourceRefColors in Source.GetReferenceColors() )
			{
				yield return sourceRefColors;
			}
		}
	}

	public override void ApplySourceInstance()
	{
		if( SourceInstance is InheritColorSource == false )
		{
			return;
		}

		base.ApplySourceInstance();

		this.Source = (SourceInstance as InheritColorSource).Source;
	}
}
