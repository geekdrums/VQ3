using UnityEngine;
using System.Collections;

public class Player : Character {
	Vector3 initialPosition;
    GUILayer guiLayer;

    public GameObject DefendAnimPrefab;

	// Use this for initialization
	void Start()
    {
        Initialize();
		guiLayer = GetComponent<GUILayer>();
        initialPosition = guiLayer.transform.position;
        debugText.text = HitPoint.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		if ( damageTime > 0 )
		{
			if ( (int)( damageTime/0.05f ) != (int)( (damageTime+Time.deltaTime)/0.05f ) )
			{
				guiLayer.transform.position = initialPosition + Random.insideUnitSphere;
			}
			damageTime -= Time.deltaTime;
			if ( damageTime <= 0 )
			{
				guiLayer.transform.position = initialPosition;
			}
		}
	}

	public override string ToString()
	{
		return "Player";
	}

    protected override void BeDamaged( int damage, Skill skill )
    {
        base.BeDamaged( damage, skill );
        debugText.text = HitPoint.ToString() + ", " + damage + " damage!";

        if( damage <= 0 )
        {
            (Instantiate( DefendAnimPrefab, skill.OwnerCharacter.transform.position + new Vector3( 0, 0, -0.1f ), DefendAnimPrefab.transform.rotation ) as GameObject).transform.parent = transform;
        }

        if( HitPoint <= 0 )
        {
            GameContext.BattleConductor.OnPlayerLose();
        }
    }

    public void OnBattleStart()
    {
        TurnInit();
        HitPoint = MaxHP;
    }
}
