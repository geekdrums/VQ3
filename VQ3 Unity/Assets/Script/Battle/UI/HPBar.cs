using UnityEngine;
using System.Collections;

public class HPBar : MonoBehaviour {

    public GameObject CurrentBar;
    public GameObject RedBar;

    Player Player;
    TextMesh hpText;
    int TurnStartHP;
    Vector3 initialScale;
    Vector3 targetScale { get { return new Vector3( initialScale.x * ((float)Player.HitPoint / (float)Player.MaxHP), initialScale.y, initialScale.z ); } }

	// Use this for initialization
    void Start()
    {
        hpText = GetComponent<TextMesh>();
        Player = GameObject.Find( "Main Camera" ).GetComponent<Player>();
        initialScale = CurrentBar.transform.localScale;
        TurnStartHP = Player.HitPoint;
	}
	
	// Update is called once per frame
    void Update()
    {
	}

    public void OnDamage()
    {
        hpText.text = Player.HitPoint + "/" + Player.MaxHP;
        CurrentBar.transform.localScale = targetScale;
    }
    public void OnHeal()
    {
        hpText.text = Player.HitPoint + "/" + Player.MaxHP;
        CurrentBar.transform.localScale = targetScale;
        if( Player.HitPoint > TurnStartHP )
        {
            TurnStartHP = Player.HitPoint;
            RedBar.transform.localScale = targetScale;
        }
    }
    public void OnTurnStart()
    {
        hpText.text = Player.HitPoint + "/" + Player.MaxHP;
        TurnStartHP = Player.HitPoint;
        CurrentBar.transform.localScale = targetScale;
        RedBar.transform.localScale = targetScale;
    }
}
