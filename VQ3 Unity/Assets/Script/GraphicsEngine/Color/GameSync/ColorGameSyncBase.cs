using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// ColorのGameSync(State/Parameter)の基底クラス。
/// </summary>
/// <typeparam name="InputType">ゲーム側から受け取る変数のタイプ。</typeparam>
/// <typeparam name="OutputType">PropertyEditに受け渡す変数のタイプ。</typeparam>
/// <typeparam name="PropertyEditType">利用するPropertyEditのタイプ。</typeparam>
[Serializable]
public abstract class ColorGameSync<InputType, OutputType, PropertyEditType> where PropertyEditType : ColorPropertyEditBase<OutputType>
{
	#region editor params

	/// <summary>
	/// 何のパラメータをどれだけ変化させるか、のデータを配列で保持。
	/// </summary>
	public PropertyEditType[] PropertyEditList;
	public float TransitionTime;
	
	public bool IsGlobal;
	public InputType GlobalValue;

#if UNITY_EDITOR
	public bool IsFoldOut = false;
	public bool IsPropertyEditListFoldOut = false;
#endif

	#endregion


	#region property

	public abstract string Name { get; }

	// list interfaces
	public int Count { get { return PropertyEditList.Length; } }
	public ColorPropertyEditBase<OutputType> this[int index] { get { return PropertyEditList[index]; } }
	
	public bool NeedTransition { get { return TransitionTime > 0; } }

	#endregion


	#region apply functions

	/// <summary>
	/// PropertyEditから変化量（EditValue）を取得するために、
	/// ゲーム側から渡される値（InputType. string or float）から、データ保存に適した値（OutputType. 配列のインデックスや正規化された数値）に変換する
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public abstract OutputType Process(InputType input);
	/// <summary>
	/// Processの結果を用いてInputから実際に色の変化に使うEditValueに変換する。
	/// </summary>
	/// <param name="index">PropertyEditListに対応するインデックス</param>
	/// <param name="input">入力</param>
	/// <returns></returns>
	public float GetEditValue(int index, InputType input)
	{
		OutputType output = Process(input);
		return PropertyEditList[index].GetEditValue(output);
	}
	/// <summary>
	/// PropertyEditList全ての変化量（EditValue）を適用する。
	/// </summary>
	/// <param name="input"></param>
	/// <param name="h"></param>
	/// <param name="s"></param>
	/// <param name="v"></param>
	/// <param name="a"></param>
	public void Apply(InputType input, ref float h, ref float s, ref float v, ref float a)
	{
		for( int i = 0; i < PropertyEditList.Length; ++i )
		{
			float editValue = GetEditValue(i, IsGlobal ? GlobalValue : input);
			PropertyEditList[i].Apply(editValue, ref h, ref s, ref v, ref a);
		}
	}

	#endregion


	#region reference

	[SerializeField, NonEditable]
	protected List<ColorSourceBase> beReferencedColors_ = new List<ColorSourceBase>();

	public IEnumerable<ColorSourceBase> GetReferenceColors()
	{
		return beReferencedColors_.AsEnumerable<ColorSourceBase>();
	}

	public void RemoveEmptyRefereces()
	{
		List<ColorSourceBase> removeRefList = null;
		foreach( ColorSourceBase beRefColor in beReferencedColors_ )
		{
			if( beRefColor == null )
			{
				if( removeRefList == null )
				{
					removeRefList = new List<ColorSourceBase>();
				}
				removeRefList.Add(beRefColor);
				continue;
			}

			bool isStillReferenced = false;
			if( this is ColorGameSyncByState )
			{
				isStillReferenced = beRefColor.HasState(Name);
			}
			else if( this is ColorGameSyncByParameter )
			{
				isStillReferenced = beRefColor.HasParameter(Name);
			}

			if( isStillReferenced == false )
			{
				if( removeRefList == null )
				{
					removeRefList = new List<ColorSourceBase>();
				}
				removeRefList.Add(beRefColor);
			}
		}

		if( removeRefList != null )
		{
			beReferencedColors_.RemoveAll((c) => removeRefList.Contains(c));
		}
	}

	public void BeReferencedBy(ColorSourceBase colorSource)
	{
		if( beReferencedColors_.Contains(colorSource) == false )
		{
			beReferencedColors_.Add(colorSource);
		}
	}

	public void NotBeReferencedBy(ColorSourceBase colorSource)
	{
		if( beReferencedColors_.Contains(colorSource) )
		{
			beReferencedColors_.Remove(colorSource);
		}
	}

	public void SetReferenceColorDirty()
	{
		foreach( ColorSourceBase refColor in beReferencedColors_ )
		{
			if( refColor != null )
			{
				refColor.SetReferenceColorDirty();
			}
		}
	}

	public void RecalcurateReferenceColor()
	{
		foreach( ColorSourceBase refColor in beReferencedColors_ )
		{
			if( refColor != null )
			{
				refColor.RecalculateColor();
				refColor.RecalculateReferencedColors();
			}
		}
	}

	#endregion
}

/// <summary>
/// ColorのGameSyncで何のパラメータをどれだけ変化させるかのデータを保持する。
/// GameSyncはこれを複数持つことで、複数のパラメータ変更に対応する。
/// </summary>
/// <typeparam name="InputType">変化量を決定するためにGameSyncから受け取る変数のタイプ（ColorGameSyncBaseクラスのOutputTypeに対応）</typeparam>
[Serializable]
public abstract class ColorPropertyEditBase<InputType>
{
	public ColorPropertyType PropertyType; // 何のパラメータを
	public ColorPropertyEditType EditType; // どのように（足す？掛ける？上書き？）
	public abstract float GetEditValue(InputType input); // どれだけ動かすか？

	/// <summary>
	/// 変化量を適用。
	/// InputTypeからeditValue（変化量）を算出する。
	/// </summary>
	public void Apply(InputType input, ref float h, ref float s, ref float v, ref float a)
	{
		Apply(GetEditValue(input), ref h, ref s, ref v, ref a);
	}

	/// <summary>
	/// 変化量を適用。
	/// editValue（変化量）を直接指定。
	/// 変化をアニメーションさせるためにColorPropertyTransitionクラスから利用する。
	/// </summary>
	public void Apply(float editValue, ref float h, ref float s, ref float v, ref float a)
	{
		switch( PropertyType )
		{
			case ColorPropertyType.H:
				ColorPropertyUtil.ApplyEdit(ref h, PropertyType, EditType, editValue);
				break;
			case ColorPropertyType.S:
				ColorPropertyUtil.ApplyEdit(ref s, PropertyType, EditType, editValue);
				break;
			case ColorPropertyType.V:
				ColorPropertyUtil.ApplyEdit(ref v, PropertyType, EditType, editValue);
				break;
			case ColorPropertyType.A:
				ColorPropertyUtil.ApplyEdit(ref a, PropertyType, EditType, editValue);
				break;
		}
	}
}