using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnhanceUI : MonoBehaviour {

	public EnhanceParamType paramType = EnhanceParamType.Count;

	GaugeRenderer line_;
	SpriteRenderer sprite_;

	public void Set(EnhanceParamType type)
	{
		paramType = type;
		sprite_.sprite = GameContext.PlayerConductor.EnhIcons[(int)paramType];
		line_.SetRate(1);
	}

	public void Reset()
	{
		paramType = EnhanceParamType.Count;
		sprite_.sprite = null;
		line_.SetRate(0);
	}

	// Use this for initialization
	void Awake () {
		line_ = GetComponentInChildren<GaugeRenderer>();
		sprite_ = GetComponentInChildren<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
