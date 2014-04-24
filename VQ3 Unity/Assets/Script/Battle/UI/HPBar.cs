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
        UpdateHPText();
        CurrentBar.transform.localScale = targetScale;

        //Vector3 nextDamageTextPosition = (latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.GetComponent<DamageText>().initialPosition + Vector3.right);
        //latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        //latestDamageText.GetComponent<DamageText>().Initialize( damage, nextDamageTextPosition );
    }
    public void OnHeal( int heal )
    {
        UpdateHPText();
        CurrentBar.transform.localScale = targetScale;
        if( Player.HitPoint > TurnStartHP )
        {
            TurnStartHP = Player.HitPoint;
            RedBar.transform.localScale = targetScale;
        }

        //Vector3 nextDamageTextPosition = (latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.transform.position + Vector3.right);
        //latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        //latestDamageText.GetComponent<DamageText>().Initialize( -heal, nextDamageTextPosition );
    }
    public void OnUpdateHP()
    {
        UpdateHPText();
        CurrentBar.transform.localScale = targetScale;
        if( Player.HitPoint > TurnStartHP )
        {
            TurnStartHP = Player.HitPoint;
            RedBar.transform.localScale = targetScale;
        }
    }
    public void OnTurnStart()
    {
        UpdateHPText();
        TurnStartHP = Player.HitPoint;
        CurrentBar.transform.localScale = targetScale;
        RedBar.transform.localScale = targetScale;
    }

    void UpdateHPText()
    {
        hpText.text = "HP:" + Player.HitPoint + "/" + Player.MaxHP;
        //if( (float)Player.HitPoint / Player.MaxHP <= 0.25f )
        //{
        //    hpText.color = Color.gray;
        //}
        //else
        //{
        //    hpText.color = Color.white;
        //}
    }
}
