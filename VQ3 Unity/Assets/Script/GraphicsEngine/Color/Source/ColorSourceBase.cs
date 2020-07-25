using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColorSourceBase : MonoBehaviour
{
	#region color interfaces

	public virtual Color SourceColor
	{
		get
		{
			return Color.clear; // Not implemented
		}
	}
	public virtual void GetSourceHSVA(out float H, out float S, out float V, out float A)
	{
		ColorPropertyUtil.ToHSVA(SourceColor, out H, out S, out V, out A);
	}

	/// <summary>
	/// StaticEditまでを適用した色
	/// </summary>
	public Color StaticEditedColor
	{
		get
		{
			return staticEditedColor_;
		}
	}
	/// <summary>
	/// StaticEditとInteractiveEditを適用した結果の色
	/// </summary>
	public Color ResultColor
	{
		get
		{
			return color_;
		}
	}
	/// <summary>
	/// 結果の色に変換する暗黙的キャスト
	/// </summary>
	/// <param name="source"></param>
	public static implicit operator Color(ColorSourceBase source)
	{
		return source.color_;
	}

	[SerializeField]
	protected Component Target;

	/// <summary>
	/// 指定されれば、Sourceのパラメータを（State,Parameterなどのインタラクティブな要素を除いて）コピーする。
	/// </summary>
	[SerializeField]
	protected ColorSourceBase SourceInstance;

	#endregion


	#region static edit

	[SerializeField]
	protected ColorPropertyEditType EditTypeH;
	[SerializeField]
	protected ColorPropertyEditType EditTypeS;
	[SerializeField]
	protected ColorPropertyEditType EditTypeV;
	[SerializeField]
	protected ColorPropertyEditType EditTypeA;

	[SerializeField]
	[Range(-1, 1)]
	protected float EditValueH = 0;
	[SerializeField]
	[Range(-1, 1)]
	protected float EditValueS = 0;
	[SerializeField]
	[Range(-1, 1)]
	protected float EditValueV = 0;
	[SerializeField]
	[Range(-1, 1)]
	protected float EditValueA = 0;

	#endregion


	#region interactive edit

	// dictionary
	[SerializeField]
	protected SerializableStringDictionary StateDict = new SerializableStringDictionary();
	[SerializeField]
	protected SerializableStringToFloatDictionary ParameterDict = new SerializableStringToFloatDictionary();

	// transition
	protected Dictionary<string, ColorStateTransition> stateTransitionDict_ = new Dictionary<string, ColorStateTransition>();
	protected Dictionary<string, ColorParameterTransition> parameterTransitionDict_ = new Dictionary<string, ColorParameterTransition>();
	protected List<ColorTransitionBase> playingTransitions_ = new List<ColorTransitionBase>();

	#endregion


	#region params
	
	protected Color color_;
	protected Color staticEditedColor_;
	protected bool isDirty_ = true;

	// 自分が参照「されている」（子供側の）色
	[SerializeField, NonEditable]
	protected List<ColorSourceBase> beReferencedColors_ = new List<ColorSourceBase>();
	protected bool hasReferenceLoop_ = false;

	// HSVA
	// H: Hue, S:Saturation, V:Value(Brightness), A:Alpha
	protected float h_, s_, v_, a_;
	public float H { get { return h_; } protected set { h_ = value; } }
	public float S { get { return s_; } protected set { s_ = value; } }
	public float V { get { return v_; } protected set { v_ = value; } }
	public float A { get { return a_; } protected set { a_ = value; } }

	#endregion


	#region unity functions

	protected virtual void Awake()
	{
		isDirty_ = true;

		if( CheckReferenceLoop() )
		{
			return;
		}

		RemoveEmptyRefereces();
		NotifyMyReferences();

		ExcludeInvalidGameSyncs();
		InitTransitionDict();
	}

	protected virtual void Start()
	{
		CheckReferenceColorDirty();
		RecalculateColor();
	}

	protected virtual void OnValidate()
	{
		if( CheckReferenceLoop() )
		{
			return;
		}

		RemoveEmptyRefereces();
		NotifyMyReferences();

		StateDict.OnValidate();
		ParameterDict.OnValidate();

		CheckReferenceColorDirty();
		RecalculateColor();
		RecalculateReferencedColors();
	}

	void Update()
	{
		playingTransitions_.RemoveAll(transition => transition.RemainingTime() == false);
		if( playingTransitions_.Count > 0 )
		{
			SetReferenceColorDirty();
			foreach( ColorTransitionBase transition in playingTransitions_ )
			{
				transition.Update();
			}
		}
	}

	void LateUpdate()
	{
		if( isDirty_ )
		{
			CheckReferenceColorDirty();
			RecalculateColor();
		}
	}

	#endregion


	#region game sync

	// state

	public virtual bool HasState(string stateGroupName) { return StateDict.ContainsKey(stateGroupName); }
	public string GetState(string stateGroupName)
	{
		if( StateDict.ContainsKey(stateGroupName) )
		{
			return StateDict[stateGroupName];
		}
		else
		{
			return "";
		}
	}
	public virtual void SetState(string stateGroupName, string stateName)
	{
		if( StateDict.ContainsKey(stateGroupName) )
		{
			StateDict[stateGroupName] = stateName;

			if( stateTransitionDict_.ContainsKey(stateGroupName) )
			{
				ColorStateTransition stateTransition = stateTransitionDict_[stateGroupName];
				stateTransition.SetTarget(stateName);
				if( playingTransitions_.Contains(stateTransition) == false )
				{
					playingTransitions_.Add(stateTransition);
				}
			}
			
			SetReferenceColorDirty();
		}
		else
		{
			Debug.LogWarning(string.Format("{0} has no such state group: {1}", name, stateGroupName));
		}
	}
	protected bool IsStateTransitioning(string stateGroupName)
	{
		return stateTransitionDict_.ContainsKey(name) && playingTransitions_.Contains(stateTransitionDict_[name]);
	}
	
	// parameter

	public virtual bool HasParameter(string parameterName) { return ParameterDict.ContainsKey(parameterName); }
	public float GetParameter(string parameterName)
	{
		if( ParameterDict.ContainsKey(parameterName) )
		{
			return ParameterDict[parameterName];
		}
		else
		{
			return 0;
		}
	}
	public virtual void SetParameter(string parameterName, float value)
	{
		if( ParameterDict.ContainsKey(parameterName) )
		{
			ParameterDict[parameterName] = value;

			if( parameterTransitionDict_.ContainsKey(parameterName) )
			{
				ColorParameterTransition paramTransition = parameterTransitionDict_[parameterName];
				paramTransition.SetTarget(value);
				if( playingTransitions_.Contains(paramTransition) == false )
				{
					playingTransitions_.Add(paramTransition);
				}
			}

			SetReferenceColorDirty();
		}
		else
		{
			Debug.LogWarning(string.Format("{0} has no such parameter: {1}", name, parameterName));
		}
	}
	protected bool IsParameterTransitioning(string stateGroupName)
	{
		return parameterTransitionDict_.ContainsKey(name) && playingTransitions_.Contains(parameterTransitionDict_[name]);
	}

	/// <summary>
	/// ColorManagerに登録されてないStateやParameterは無効なので消す
	/// </summary>
	void ExcludeInvalidGameSyncs()
	{
		List<string> removeStateList = new List<string>();
		foreach( KeyValuePair<string, string> stateGroupAndName in StateDict )
		{
			if( ColorManager.ContainsStateGroup(stateGroupAndName.Key) == false )
			{
				removeStateList.Add(stateGroupAndName.Key);
			}
		}
		foreach( string removeState in removeStateList )
		{
			StateDict.Remove(removeState);
		}

		List<string> removeParameterList = new List<string>();
		foreach( KeyValuePair<string, float> parameterNameAndValue in ParameterDict )
		{
			if( ColorManager.ContainsParameter(parameterNameAndValue.Key) == false )
			{
				removeParameterList.Add(parameterNameAndValue.Key);
			}
		}
		foreach( string removeParam in removeParameterList )
		{
			ParameterDict.Remove(removeParam);
		}
	}

	/// <summary>
	/// Transitionが必要な（TransitionTimeが設定されている）StateやParameterは
	/// Transition用の構造体を用意しておく
	/// </summary>
	protected virtual void InitTransitionDict()
	{
		foreach( string stateGroupName in StateDict.Keys )
		{
			ColorGameSyncByState stateGroup = ColorManager.GetStateGroup(stateGroupName);
			if( stateGroup != null && stateGroup.NeedTransition )
			{
				ColorStateTransition colorStateTransition = new ColorStateTransition();
				colorStateTransition.Init(stateGroup, initialValue: StateDict[stateGroupName]);
				stateTransitionDict_.Add(stateGroupName, colorStateTransition);
			}
		}
		foreach( string parameterName in ParameterDict.Keys )
		{
			ColorGameSyncByParameter parameter = ColorManager.GetParameter(parameterName);
			if( parameter != null && parameter.NeedTransition )
			{
				ColorParameterTransition colorParameterTransition = new ColorParameterTransition();
				colorParameterTransition.Init(parameter, initialValue: ParameterDict[parameterName]);
				parameterTransitionDict_.Add(parameterName, colorParameterTransition);
			}
		}
	}

	#endregion


	#region reference

	/// <summary>
	/// 自分が参照「されている」（子供側の）リファレンスを更新する。
	/// まだ存在するか、まだ参照されているかチェックして、されていなければ外す。
	/// </summary>
	void RemoveEmptyRefereces()
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
			foreach( ColorSourceBase refColor in beRefColor.GetReferenceColors() )
			{
				if( refColor == this )
				{
					isStillReferenced = true;
					break;
				}
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

	/// <summary>
	/// 自分が参照「している」（親の）リファレンスに対して、参照している事を通知する。
	/// </summary>
	protected virtual void NotifyMyReferences()
	{
		foreach( ColorSourceBase refColor in this.GetReferenceColors() )
		{
			if( refColor != null )
			{
				refColor.BeReferencedBy(this);
			}
		}

		foreach( string stateGroup in StateDict.Keys )
		{
			if( ColorManager.ContainsStateGroup(stateGroup) )
			{
				ColorManager.GetStateGroup(stateGroup).BeReferencedBy(this);
			}
		}
		foreach( string parameterName in ParameterDict.Keys )
		{
			if( ColorManager.ContainsParameter(parameterName) )
			{
				ColorManager.GetParameter(parameterName).BeReferencedBy(this);
			}
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

	/// <summary>
	/// ループ参照検出。検出されたら危ない操作を受け付けなくなる。
	/// </summary>
	/// <returns></returns>
	public virtual bool CheckReferenceLoop()
	{
		hasReferenceLoop_ = false;
		foreach( ColorSourceBase refColor in GetReferenceColors() )
		{
			if( refColor == this )
			{
				hasReferenceLoop_ = true;
				Debug.LogWarning(string.Format("Color Source [{0}] has Reference Loop!", name));
				break;
			}
		}
		return hasReferenceLoop_;
	}

	/// <summary>
	/// 自分が参照「している」（親側の）色。
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerable<ColorSourceBase> GetReferenceColors()
	{
		yield break;
	}

	/// <summary>
	/// 自分が参照「している」色に変化がないか確認し、あれば更新する。
	/// これによってUpdate順に関わらず、必ず親側の色が先に更新される。
	/// </summary>
	protected void CheckReferenceColorDirty()
	{
		if( hasReferenceLoop_ )
		{
			return;
		}

		foreach( ColorSourceBase refColor in GetReferenceColors() )
		{
			if( refColor.isDirty_ )
			{
				refColor.CheckReferenceColorDirty();
				refColor.RecalculateColor();
			}
		}
	}

	/// <summary>
	/// 自分が参照「されている」色をすべて更新する。
	/// Validate時など1つのオブジェクトにしか更新が来ない時にこれを使って強制的に更新をかける。
	/// </summary>
	public void RecalculateReferencedColors()
	{
		if( hasReferenceLoop_ )
		{
			return;
		}
		foreach( ColorSourceBase refColor in beReferencedColors_ )
		{
#if UNITY_EDITOR
			if( refColor == null ) continue;
#endif
			refColor.RecalculateColor();
			refColor.RecalculateReferencedColors();
		}
	}

	/// <summary>
	/// 自分が参照「されている」色をすべてDirty設定にする。
	/// インゲーム中などで複数の変化が同時に起こり得るときに、各自のLateUpdateに任せる形で更新をかける。
	/// </summary>
	public void SetReferenceColorDirty()
	{
		isDirty_ = true;
		foreach( ColorSourceBase refColor in beReferencedColors_ )
		{
#if UNITY_EDITOR
			if( refColor == null ) continue;
#endif
			refColor.SetReferenceColorDirty();
		}
	}

	#endregion


	#region apply

	public void RecalculateColor()
	{
		isDirty_ = false;
		if( hasReferenceLoop_ )
		{
			return;
		}

		// 変更が何も無ければ余計な計算を省く。
		if( EditTypeH == ColorPropertyEditType.None &&
			EditTypeS == ColorPropertyEditType.None &&
			EditTypeV == ColorPropertyEditType.None &&
			EditTypeA == ColorPropertyEditType.None && 
			StateDict.Count == 0 &&
			ParameterDict.Count == 0 )
		{
			GetSourceHSVA(out h_, out s_, out v_, out a_);
			color_ = staticEditedColor_ = SourceColor;
			ApplyToTarget();
			return;
		}

		// 元となるHSVAを取得
		GetSourceHSVA(out h_, out s_, out v_, out a_);

		// static edit
		// 固定で設定されている変化を適用する
		ApplyStaticEdit(ref h_, ref s_, ref v_, ref a_);
		staticEditedColor_ = ColorPropertyUtil.FromHSVA(H, S, V, A);

		// interactive edit
		// StateやParameterによる変化を適用する
		ApplyGameSyncEdit(ref h_, ref s_, ref v_, ref a_);

		// update
		color_ = ColorPropertyUtil.FromHSVA(H, S, V, A);
		ApplyToTarget();
	}

	void ApplyStaticEdit(ref float h, ref float s, ref float v, ref float a)
	{
		ColorPropertyUtil.ApplyEdit(ref h, ColorPropertyType.H, EditTypeH, EditValueH);
		ColorPropertyUtil.ApplyEdit(ref s, ColorPropertyType.S, EditTypeS, EditValueS);
		ColorPropertyUtil.ApplyEdit(ref v, ColorPropertyType.V, EditTypeV, EditValueV);
		ColorPropertyUtil.ApplyEdit(ref a, ColorPropertyType.A, EditTypeA, EditValueA);
	}

	void ApplyGameSyncEdit(ref float h, ref float s, ref float v, ref float a)
	{
#if UNITY_EDITOR
		if( UnityEditor.EditorApplication.isPlaying == false )
		{
			foreach( KeyValuePair<string, string> stateGroupAndName in StateDict )
			{
				// Edit中では、まだColorManagerにちゃんとStateの登録ができていない場合もある
				if( ColorManager.ContainsStateGroup(stateGroupAndName.Key) )
				{
					ColorGameSyncByState stateGroup = ColorManager.GetStateGroup(stateGroupAndName.Key);
					stateGroup.Apply(stateGroupAndName.Value, ref h_, ref s_, ref v_, ref a_);
				}
			}
			foreach( KeyValuePair<string, float> parameterNameAndValue in ParameterDict )
			{
				if( ColorManager.ContainsParameter(parameterNameAndValue.Key) )
				{
					ColorGameSyncByParameter parameter = ColorManager.GetParameter(parameterNameAndValue.Key);
					parameter.Apply(parameterNameAndValue.Value, ref h_, ref s_, ref v_, ref a_);
				}
			}
			return;
		}
#endif
		foreach( KeyValuePair<string, string> stateGroupAndName in StateDict )
		{
			if( this.IsStateTransitioning(stateGroupAndName.Key) )
			{
				// インゲーム中は、Transitionが起こっている場合がある
				stateTransitionDict_[stateGroupAndName.Key].Apply(ref h_, ref s_, ref v_, ref a_);
			}
			else if( ColorManager.IsStateTransitioning(stateGroupAndName.Key) )
			{
				// インゲーム中は、GlobalなTransitionが起こっている場合もある
				ColorStateTransition globalStateTransition = ColorManager.GetStateTransition(stateGroupAndName.Key);
				globalStateTransition.Apply(ref h_, ref s_, ref v_, ref a_);
			}
			else
			{
				// インゲーム中はColorManagerに登録されているState以外は除外されている事が補償されている。
				ColorGameSyncByState stateGroup = ColorManager.GetStateGroup(stateGroupAndName.Key);
				stateGroup.Apply(stateGroupAndName.Value, ref h_, ref s_, ref v_, ref a_);
			}
		}
		foreach( KeyValuePair<string, float> parameterNameAndValue in ParameterDict )
		{
			if( IsParameterTransitioning(parameterNameAndValue.Key) )
			{
				parameterTransitionDict_[parameterNameAndValue.Key].Apply(ref h_, ref s_, ref v_, ref a_);
			}
			else if( ColorManager.IsParameterTransitioning(parameterNameAndValue.Key) )
			{
				ColorParameterTransition globalParameterTransition = ColorManager.GetParameterTransition(parameterNameAndValue.Key);
				globalParameterTransition.Apply(ref h_, ref s_, ref v_, ref a_);
			}
			else
			{
				ColorGameSyncByParameter parameter = ColorManager.GetParameter(parameterNameAndValue.Key);
				parameter.Apply(parameterNameAndValue.Value, ref h_, ref s_, ref v_, ref a_);
			}
		}
	}

	void ApplyToTarget()
	{
		if( Target != null && Target is IColoredObject )
		{
			(Target as IColoredObject).SetColor(this);
		}
	}

	public virtual void ApplySourceInstance()
	{
		this.EditTypeH = SourceInstance.EditTypeH;
		this.EditTypeS = SourceInstance.EditTypeS;
		this.EditTypeV = SourceInstance.EditTypeV;
		this.EditTypeA = SourceInstance.EditTypeA;

		this.EditValueH = SourceInstance.EditValueH;
		this.EditValueS = SourceInstance.EditValueS;
		this.EditValueV = SourceInstance.EditValueV;
		this.EditValueA = SourceInstance.EditValueA;
		
		if( StateDict.KeyEquals(SourceInstance.StateDict) == false )
		{
			SerializableStringDictionary oldDict = new SerializableStringDictionary();
			oldDict.Clone(StateDict);
			StateDict.Clone(SourceInstance.StateDict);
			foreach( var key in StateDict.Keys )
			{
				if( oldDict.ContainsKey(key) )
				{
					StateDict[key] = oldDict[key];
				}
			}
		}
		if( ParameterDict.KeyEquals(SourceInstance.ParameterDict) == false )
		{
			SerializableStringToFloatDictionary oldDict = new SerializableStringToFloatDictionary();
			oldDict.Clone(ParameterDict);
			ParameterDict.Clone(SourceInstance.ParameterDict);
			foreach( var key in ParameterDict.Keys )
			{
				if( oldDict.ContainsKey(key) )
				{
					ParameterDict[key] = oldDict[key];
				}
			}
		}
	}

	#endregion

}
