using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemyState
{
    Default,
    State1,
    State2,
    State3,
    //etc...
}

public class Enemy : Character {

    public EnemyCommand[] Commands;

    public EnemyCommand currentCommand { get; protected set; }
    protected int commandExecBar;
    protected EnemyState currentState = EnemyState.Default;

    SpriteRenderer HPCircle;
    Vector3 baseHPCircleScale;

    //todo: multiply EnemyConductor's coeff
    float targetHPCircleSize;// { get { return Mathf.Sqrt( HitPoint ); } }
    Color targetHPCircleColor;// { get { return Color.Lerp( Color.clear, GameContext.EnemyConductor.baseColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) ); } }

	// Use this for initialization
	void Start()
	{
        base.Initialize();
        HPCircle = GetComponentsInChildren<SpriteRenderer>()[1];
        if( HPCircle != null )
        {
            baseHPCircleScale = HPCircle.transform.localScale / Mathf.Sqrt( GameContext.EnemyConductor.baseHP );
            targetHPCircleSize = Mathf.Sqrt( MaxHP );
            HPCircle.transform.localScale = baseHPCircleScale * targetHPCircleSize;
            targetHPCircleColor = GameContext.EnemyConductor.baseColor;
            HPCircle.color = targetHPCircleColor;
        }
        foreach( EnemyCommand c in Commands )
        {
            c.Parse();
        }

        debugText = gameObject.AddComponent<GUIText>();
        debugText.transform.position = Vector3.one / 2;
        debugText.fontSize = 24;
        debugText.color = Color.red;
        debugText.text = HitPoint.ToString();
    }

	// Update is called once per frame
	void Update ()
	{
		if ( damageTime > 0 )
		{
            renderer.material.color = (damageTime % 0.1f > 0.05f ? Color.clear : targetHPCircleColor);
			damageTime -= Time.deltaTime;
			if ( damageTime <= 0 )
			{
				if ( HitPoint <= 0 )
				{
                    renderer.material.color = Color.clear;
                    Destroy( this.gameObject );
				}
				else
				{
                    renderer.material.color = targetHPCircleColor;
				}
			}
		}
        UpdateAnimation();
	}

    void UpdateAnimation()
    {
        HPCircle.transform.localScale = Vector3.Lerp( HPCircle.transform.localScale, baseHPCircleScale * targetHPCircleSize, 0.1f );
        HPCircle.color = Color.Lerp( targetHPCircleColor, HPCircle.color, 0.1f );
    }

    public EnemyCommand CheckCommand()
    {
        if( currentCommand != null && currentCommand.nextCommand != null )
        {
            currentCommand = currentCommand.nextCommand;
        }
        else
        {
            List<int> probabilityies = new List<int>();
            int probabilitySum = 0;
            foreach( EnemyCommand c in Commands )
            {
                probabilitySum += c.GetProbability( (int)currentState );
                probabilityies.Add( probabilitySum );
            }
            int rand = Random.Range( 0, probabilitySum );
            for( int i = 0; i < probabilityies.Count; i++ )
            {
                if( rand < probabilityies[i] )
                {
                    currentCommand = Commands[i];
                    break;
                }
            }
        }
        return currentCommand;
    }
    public void SetExecBar( int bar )
    {
        commandExecBar = bar;
    }
    public Skill GetCurrentSkill()
    {
        return currentCommand.GetCurrentSkill( commandExecBar );
    }

    protected override void BeDamaged( int damage )
    {
        base.BeDamaged( damage );
        debugText.text = HitPoint.ToString() + ", " + damage + " damage!";
        if( HPCircle != null )
        {
            targetHPCircleSize = Mathf.Sqrt( HitPoint );
            targetHPCircleColor = Color.Lerp( Color.clear, GameContext.EnemyConductor.baseColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) );
        }
    }
    public void OnBaseColorChanged( Color newColor )
    {
        renderer.material.color = newColor;
        targetHPCircleColor = Color.Lerp( Color.clear, newColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) );
        HPCircle.color = targetHPCircleColor;
    }


	// ======================
	// Utils
	// ======================
	public override string ToString()
	{
        return name;
	}
}
