using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnhanceIcons : MonoBehaviour {

    public List<Color> colors;
    public Color minusColor;
    public List<SpriteRenderer> icons;
    public List<Sprite> iconSprites;
    public List<GameObject> lines;

    Player Player;
    Vector3 maxLineScale;
    readonly int maxTurnCount = 4;
    //List<EnhanceParameter> enhanceParams = new List<EnhanceParameter>();

	// Use this for initialization
    void Start()
    {
        Player = GameObject.Find( "Main Camera" ).GetComponent<Player>();
        maxLineScale = lines[0].transform.localScale;
        maxLineScale.y = 1;
	}
	
	// Update is called once per frame
	void Update () {
        for( EnhanceParamType type = EnhanceParamType.Brave; (int)type <= (int)EnhanceParamType.Regene; type++ )
        {
            EnhanceParameter enhance = Player.GetActiveEnhance( type );
            if( enhance != null )
            {
                Color baseColor = enhance.phase > 0 ? colors[(int)type] : minusColor;
                icons[(int)type].color = baseColor
                    * ( 0.7f + 0.3f * Mathf.Abs( Mathf.Sin( Mathf.PI * (float)Music.MusicalTime / (enhance.remainTurn > 1 ? 16 : 4) ) ) );
                Vector3 lineScale = maxLineScale;
                lineScale.y = (float)Mathf.Min( maxTurnCount, enhance.remainTurn ) / maxTurnCount;
                lines[(int)type].transform.localScale = lineScale;
            }
            else
            {
                icons[(int)type].color = Color.Lerp( icons[(int)type].color, Color.clear, 0.1f );
                lines[(int)type].transform.localScale = Vector3.zero;
            }
        }
	}

    public void OnUpdateParam( EnhanceParameter enhance )
    {
        if( enhance != null && enhance.phase != 0 )
        {
            Sprite iconSpr = null;
            char iconChar = ' ';
            switch( enhance.type )
            {
            case EnhanceParamType.Brave: iconChar = 'P'; break;
            case EnhanceParamType.Faith: iconChar = 'M'; break;
            case EnhanceParamType.Shield: iconChar = 'S'; break;
            case EnhanceParamType.Regene: iconChar = 'R'; break;
            }
            string iconName = new string( iconChar, Mathf.Abs( enhance.phase ) );
            if( enhance.phase < 0 ) iconName = iconName.Insert( 0, "-" );
            iconSpr = iconSprites.Find( ( Sprite spr ) => spr.name == iconName );
            if( iconSpr != null )
            {
                icons[(int)enhance.type].sprite = iconSpr;
            }
        }
    }
}
