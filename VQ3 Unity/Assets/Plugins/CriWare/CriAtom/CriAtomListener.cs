/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Atom
 * Module   : CRI Atom for Unity
 * File     : CriAtomListener.cs
 *
 ****************************************************************************/
using UnityEngine;
using System;
using System.Collections;

/// \addtogroup CRIATOM_UNITY_COMPONENT
/// @{

/**
 * <summary>3Dリスナーを表すコンポーネントです。</summary>
 * \par 説明:
 * 通常、カメラやメインキャラクタのGameObjectに付与して使用します。
 * 現在位置の更新は自動的に行われるため、特に操作や設定を行う必要はありません。
 */
[AddComponentMenu("CRIWARE/CRI Atom Listener")]
public class CriAtomListener : MonoBehaviour
{
	#region Enumlators
	public static CriAtomListener instance {
		get; private set;
	}
	public CriAtomEx3dListener internalListener {
		get; private set;
	}
	#endregion

	#region Variables
	private Vector3 lastPosition;
	#endregion

	#region Functions
	void OnEnable()
	{
		if (CriAtomListener.instance != null) {
			Debug.LogError("Multiple listener instances.");
		}
		CriAtomListener.instance = this;
		CriAtomPlugin.InitializeLibrary();
		this.internalListener = new CriAtomEx3dListener();
		this.lastPosition = this.transform.position;
	}

	void OnDisable()
	{
		if (this.internalListener != null) {
			this.internalListener.Dispose();
			this.internalListener = null;
			CriAtomPlugin.FinalizeLibrary();
		}
		CriAtomListener.instance = null;
	}

	void LateUpdate()
	{
		Vector3 position = this.transform.position;
		Vector3 velocity = (position - this.lastPosition) / Time.deltaTime;
		Vector3 front = this.transform.forward;
		Vector3 up = this.transform.up;
		this.lastPosition = position;
		this.internalListener.SetPosition(position.x, position.y, position.z);
		this.internalListener.SetVelocity(velocity.x, velocity.y, velocity.z);
		this.internalListener.SetOrientation(front.x, front.y, front.z, up.x, up.y, up.z);
		this.internalListener.Update();
	}
	#endregion
} // end of class

/// @}
/* end of file */
