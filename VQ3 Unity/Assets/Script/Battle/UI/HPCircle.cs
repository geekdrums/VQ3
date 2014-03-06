using UnityEngine;
using System.Collections;

public class HPCircle : MonoBehaviour {

    public GameObject CurrentCircle;
    public GameObject RedCircle;
    public float RedCircleScaleCoeff;
    public Color invertColor;
    Color initialColor;

    Enemy ownerEnemy;

    int TurnStartHP;
    Vector3 initialScale;
    Vector3 targetCurrentScale;
    Vector3 targetRedScale;
    bool isActive = true;

    // Use this for initialization
    void Start()
    {
    }

    public void Initialize( Enemy owner )
    {
        ownerEnemy = owner;
        TurnStartHP = ownerEnemy.HitPoint;
        initialScale = CurrentCircle.transform.localScale;
        RedCircle.transform.localScale = Vector3.zero;
        targetCurrentScale = initialScale;
        targetRedScale = Vector3.zero;

        CurrentCircle.transform.localScale = -Vector3.one;
        initialColor = CurrentCircle.GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCircle.transform.localScale = Vector3.Lerp( CurrentCircle.transform.localScale, targetCurrentScale, 0.2f );
        RedCircle.transform.localScale = Vector3.Lerp( RedCircle.transform.localScale, targetRedScale * RedCircleScaleCoeff, 0.2f );
    }

    public void OnDamage()
    {
        if( isActive ) targetRedScale = initialScale * Mathf.Sqrt((float)(TurnStartHP - ownerEnemy.HitPoint) / (float)ownerEnemy.MaxHP);
    }
    public void OnHeal()
    {
        if( isActive )
        {
            if( ownerEnemy.HitPoint > TurnStartHP )
            {
                TurnStartHP = ownerEnemy.HitPoint;
                targetCurrentScale = initialScale * Mathf.Sqrt( (float)ownerEnemy.HitPoint / (float)ownerEnemy.MaxHP );
                targetRedScale = Vector3.zero;
            }
            else
            {
                targetRedScale = initialScale * Mathf.Sqrt( (float)(TurnStartHP - ownerEnemy.HitPoint) / (float)ownerEnemy.MaxHP );
            }
        }
    }
    public void OnTurnStart()
    {
        if( isActive )
        {
            TurnStartHP = ownerEnemy.HitPoint;
            targetCurrentScale = initialScale * Mathf.Sqrt( (float)ownerEnemy.HitPoint / (float)ownerEnemy.MaxHP );
            targetRedScale = Vector3.zero;
        }
    }
    public void OnBaseColorChanged()
    {
        //TEMP!!
        //if( GameContext.VoxSystem.state == VoxState.Eclipse )
        //{
        //    CurrentCircle.GetComponent<SpriteRenderer>().color = invertColor;
        //}
        //else
        //{
        //    CurrentCircle.GetComponent<SpriteRenderer>().color = initialColor;
        //}
    }
    public void SetActive( bool active )
    {
        isActive = active;
        if( isActive )
        {
            targetCurrentScale = initialScale * Mathf.Sqrt( (float)ownerEnemy.HitPoint / (float)ownerEnemy.MaxHP );
            targetRedScale = initialScale * Mathf.Sqrt( (float)(TurnStartHP - ownerEnemy.HitPoint) / (float)ownerEnemy.MaxHP );
        }
        else
        {
            targetCurrentScale = Vector3.zero;
            targetRedScale = Vector3.zero;
        }
    }
}
