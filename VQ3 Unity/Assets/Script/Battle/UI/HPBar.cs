using UnityEngine;
using System.Collections;

public class HPBar : MonoBehaviour {

    public GameObject CurrentBar;
    public GameObject RedBar;
    public GameObject damageTextPrefab;

    Player Player;
    TextMesh hpText;
    int TurnStartHP;
    Vector3 initialScale;
    Vector3 targetScale { get { return new Vector3( initialScale.x * ((float)Player.HitPoint / (float)Player.MaxHP), initialScale.y, initialScale.z ); } }
    GameObject latestDamageText;

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

    public void OnDamage( int damage )
    {
        hpText.text = "HP:" + Player.HitPoint + "/" + Player.MaxHP;
        CurrentBar.transform.localScale = targetScale;

        Vector3 nextDamageTextPosition =(latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.transform.position + Vector3.right);
        latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        latestDamageText.GetComponent<DamageText>().Initialize( damage, nextDamageTextPosition );
    }
    public void OnHeal( int heal )
    {
        hpText.text = "HP:" + Player.HitPoint + "/" + Player.MaxHP;
        CurrentBar.transform.localScale = targetScale;
        if( Player.HitPoint > TurnStartHP )
        {
            TurnStartHP = Player.HitPoint;
            RedBar.transform.localScale = targetScale;
        }

        Vector3 nextDamageTextPosition = (latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.transform.position + Vector3.right);
        latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        latestDamageText.GetComponent<DamageText>().Initialize( -heal, nextDamageTextPosition );
    }
    public void OnTurnStart()
    {
        hpText.text = "HP:" + Player.HitPoint + "/" + Player.MaxHP;
        TurnStartHP = Player.HitPoint;
        CurrentBar.transform.localScale = targetScale;
        RedBar.transform.localScale = targetScale;
    }
}
