using UnityEngine;
using System.Collections;

public enum IconReactType
{
    OnAttack,
    OnMagic,
    OnDamage,
    OnHeal,
    OnBrave,
    OnFaith,
    OnShield,
    OnRegene,
    OnEsna,
    OnInvert,
    None,
    ForceNone,
}
public class StatusIcon : MonoBehaviour {

    public IconReactType reactType;

    EStatusIcon icon;
    Color targetColor;
    SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        switch( reactType )
        {
        case IconReactType.None:
            spriteRenderer.color = targetColor;
            break;
        case IconReactType.ForceNone:
            spriteRenderer.color = Color.Lerp( targetColor, Color.white, 0.2f );
            break;
        case IconReactType.OnDamage:
        case IconReactType.OnHeal:
            spriteRenderer.color = Color.Lerp( spriteRenderer.color,
                Color.Lerp( Color.clear, targetColor, (0.5f + 0.5f * Mathf.Abs( Mathf.Sin( Mathf.PI * (float)Music.MusicalTime / 16 ) )) ), 0.1f );
            break;
        default:
            spriteRenderer.color = Color.Lerp( spriteRenderer.color, Color.white, 0.03f );
            break;
        }
	}

    public void SetSprite( Sprite sprite, EStatusIcon icon, Color color, IconReactType type )
    {
        spriteRenderer.sprite = sprite;
        this.icon = icon;
        this.targetColor = color;
        this.reactType = ( reactType == IconReactType.ForceNone ? IconReactType.ForceNone : type );
        if( reactType == IconReactType.None )
        {
            spriteRenderer.color = targetColor;
        }
    }
    public void ReactEvent( IconReactType type )
    {
        if( reactType == type )
        {
            spriteRenderer.color = targetColor;
        }
    }
}
