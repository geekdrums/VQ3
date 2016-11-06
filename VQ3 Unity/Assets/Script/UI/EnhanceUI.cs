using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnhanceUI : MonoBehaviour {

	public EnhanceParamType paramType = EnhanceParamType.Count;
	public int remainTurns;

	bool isExecuting_;
	GaugeRenderer line_;
	SpriteRenderer sprite_;

	public void Set(EnhanceParamType type, int turns = 1)
	{
		paramType = type;
		sprite_.sprite = GameContext.PlayerConductor.EnhIcons[(int)paramType];
		line_.SetRate(1);
		remainTurns = turns;
	}

	public void Reset()
	{
		paramType = EnhanceParamType.Count;
		sprite_.sprite = null;
		line_.SetRate(0);
		remainTurns = 0;
	}

	public void Execute()
	{
		isExecuting_ = true;
	}

	// Use this for initialization
	void Awake () {
		line_ = GetComponentInChildren<GaugeRenderer>();
		sprite_ = GetComponentInChildren<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if( isExecuting_ )
		{
			if( Music.IsJustChangedAt(CommandGraph.AllowInputEnd) && GameContext.BattleState != BattleState.Wait )
			{
				--remainTurns;
				print(remainTurns);
				if( remainTurns <= 0 )
				{
					Destroy(this.gameObject);
					return;
				}
			}

			line_.SetColor(ColorManager.Accent.Buff * (0.8f + 0.2f * Music.MusicalCos(8)));
		}
	}
}
