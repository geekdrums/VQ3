using UnityEngine;
using System.Collections;

public class HPCircle : MonoBehaviour {

    public MidairPrimitive CurrentCircle;
    //public GameObject MaxCircle;
    public MidairPrimitive RedCircle;
    public float RedCircleScaleCoeff;
    public MidairPrimitive[] WaveCircles;
    public float WavePerMaxDamage;
    public float WaveAnimRadius;
    //Color initialColor;
    //public Color invertColor;

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
        foreach( MidairPrimitive wave in WaveCircles )
        {
            wave.SetSize( 0 );
        }
        //initialColor = CurrentCircle.GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCircle.transform.localScale = Vector3.Lerp( CurrentCircle.transform.localScale, targetCurrentScale, 0.2f );
        RedCircle.transform.localScale = Vector3.Lerp( RedCircle.transform.localScale, targetRedScale * RedCircleScaleCoeff, 0.2f );
        //MaxCircle.transform.localEulerAngles = Vector3.Lerp( MaxCircle.transform.localScale, initialScale, 0.2f );
    }

    public void OnDamage(int damage)
    {
        if( isActive )
        {
            targetRedScale = initialScale * Mathf.Sqrt( (float)(TurnStartHP - ownerEnemy.HitPoint) / (float)ownerEnemy.MaxHP );
            int waveNum = (int)Mathf.Clamp( WavePerMaxDamage * (float)damage / ownerEnemy.MaxHP, 1, WaveCircles.Length );
            for( int i = 0; i < waveNum; ++i )
            {
                WaveCircles[i].SetAnimationColor( CurrentCircle.Color, Color.clear );
                WaveCircles[i].SetAnimationSize( CurrentCircle.Radius - i, WaveAnimRadius );
                WaveCircles[i].SetLinearFactor( 0.2f/(i+1) );
            }
        }
    }
    public void OnHeal( int healPoint )
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
            if( healPoint > 0 )
            {
                int waveNum = (int)Mathf.Clamp( WavePerMaxDamage * (float)healPoint / ownerEnemy.MaxHP, 1, WaveCircles.Length );
                for( int i = 0; i < waveNum; ++i )
                {
                    WaveCircles[i].SetAnimationColor( Color.clear, CurrentCircle.Color );
                    WaveCircles[i].SetAnimationSize( WaveAnimRadius + i * 0.3f, 0 );
                    WaveCircles[i].SetLinearFactor( 0.3f - i * 0.05f );
                }
            }
        }
    }
    public void OnUpdateHP()
    {
        OnHeal(0);//TEMP
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
