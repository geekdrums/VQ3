using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public AudioClip DamageSound, DefendSound, HealSound;
	public AudioSource AudioSource;

	public int HitPoint { get; protected set; }
	int DefendPower;

	float damageTime;
	Vector3 initialPosition;
	GUILayer guiLayer;

	// Use this for initialization
	void Start()
	{
		HitPoint = 10;
		guiLayer = GetComponent<GUILayer>();
		initialPosition = guiLayer.transform.position;
		DefendPower = 0;
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

	public void OnBarStarted( int CurrentIndex )
	{
		DefendPower = 0;
	}

	public void BeAttacked( AttackModule attack )
	{
		int damage = Mathf.Max( 0, attack.AttackPower - DefendPower );
		HitPoint -= damage;
		Debug.Log( this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint );
		if ( damage == 0 )
		{
			AudioSource.clip = DefendSound;
		}
		else
		{
			AudioSource.clip = DamageSound;
		}
		AudioSource.Play();
		damageTime = 0.2f + damage*0.2f;
		if ( HitPoint <= 0 )
		{
			GameContext.BattleConductor.OnPlayerLose();
			GameContext.CommandController.OnPlayerLose();
		}
	}
	public void Defend( DefendModule defend )
	{
		DefendPower = defend.DefendPower;
	}
	public void Heal( HealModule heal )
	{
		HitPoint += heal.HealPoint;
		AudioSource.PlayOneShot( HealSound );
	}
}
