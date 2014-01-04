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

    public override void BeAttacked( AttackModule attack, Skill command )
    {
        int damage = Mathf.Max( 0, attack.AttackPower - DefendPower );
        BeDamaged( damage );
        if( damage <= 0 )
        {
            SEPlayer.Play( ActionResult.Guarded, true );
            (Instantiate( DefendAnimPrefab, command.OwnerCharacter.transform.position + new Vector3( 0, 0, -0.1f ), DefendAnimPrefab.transform.rotation ) as GameObject).transform.parent = transform;
        }
        else
        {
            SEPlayer.Play( ActionResult.Damaged, this is Player );
        }

        if( HitPoint <= 0 )
        {
            GameContext.BattleConductor.OnPlayerLose();
        }
    }
}
